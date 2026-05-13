using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tabsan.EduSphere.Domain.Academic;
using Tabsan.EduSphere.Domain.Enums;
using Tabsan.EduSphere.Infrastructure.Persistence;
using Tabsan.EduSphere.IntegrationTests.Infrastructure;

namespace Tabsan.EduSphere.IntegrationTests;

[Collection(EduSphereCollection.Name)]
public class PerformanceQueryValidationIntegrationTests
{
    private readonly EduSphereWebFactory _factory;

    public PerformanceQueryValidationIntegrationTests(EduSphereWebFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task InstituteFilteredQueries_ParityIndexes_RegisterReadUsage()
    {
        var seed = await SeedQueryValidationDataAsync();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await db.Database.OpenConnectionAsync();
        try
        {
            var connection = (SqlConnection)db.Database.GetDbConnection();

            var programsBefore = await GetIndexReadCountAsync(connection, "academic_programs", "IX_academic_programs_dept_active");
            var coursesBefore = await GetIndexReadCountAsync(connection, "courses", "IX_courses_dept_active");
            var offeringsBefore = await GetIndexReadCountAsync(connection, "course_offerings", "IX_course_offerings_semester_open");

            await ExecuteScalarAsync(
                connection,
                "SELECT COUNT(1) FROM academic_programs WHERE DepartmentId = @departmentId AND IsActive = 1 OPTION (RECOMPILE);",
                new SqlParameter("@departmentId", seed.CollegeDepartmentId));

            await ExecuteScalarAsync(
                connection,
                "SELECT COUNT(1) FROM courses WHERE DepartmentId = @departmentId AND IsActive = 1 OPTION (RECOMPILE);",
                new SqlParameter("@departmentId", seed.CollegeDepartmentId));

            await ExecuteScalarAsync(
                connection,
                "SELECT COUNT(1) FROM course_offerings WHERE SemesterId = @semesterId AND IsOpen = 1 OPTION (RECOMPILE);",
                new SqlParameter("@semesterId", seed.ActiveSemesterId));

            var programsAfter = await GetIndexReadCountAsync(connection, "academic_programs", "IX_academic_programs_dept_active");
            var coursesAfter = await GetIndexReadCountAsync(connection, "courses", "IX_courses_dept_active");
            var offeringsAfter = await GetIndexReadCountAsync(connection, "course_offerings", "IX_course_offerings_semester_open");

            Assert.True(programsAfter > programsBefore, "Expected IX_academic_programs_dept_active to register read usage.");
            Assert.True(coursesAfter > coursesBefore, "Expected IX_courses_dept_active to register read usage.");
            Assert.True(offeringsAfter > offeringsBefore, "Expected IX_course_offerings_semester_open to register read usage.");
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
    }

    [Fact]
    public async Task DashboardAndReportPaths_AdminRemainWithinNoRegressionLatencyBudget()
    {
        using var client = CreateClient(role: "Admin", institutionType: (int)InstitutionType.College);

        var targets = new[]
        {
            "api/analytics/assignments",
            "api/v1/reports",
            "api/v1/attendance/below-threshold"
        };

        foreach (var target in targets)
        {
            // Warm-up request to avoid counting first-call startup overhead.
            using var warmup = await client.GetAsync(target);
            Assert.NotEqual(HttpStatusCode.Unauthorized, warmup.StatusCode);
            Assert.NotEqual(HttpStatusCode.Forbidden, warmup.StatusCode);

            var elapsed = new List<long>(capacity: 5);
            for (var i = 0; i < 5; i++)
            {
                var watch = Stopwatch.StartNew();
                using var response = await client.GetAsync(target);
                watch.Stop();

                Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
                Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
                Assert.True((int)response.StatusCode < 500, $"Expected non-5xx for {target}, got {(int)response.StatusCode}.");

                elapsed.Add(watch.ElapsedMilliseconds);
            }

            var average = elapsed.Average();
            var max = elapsed.Max();

            Assert.True(average < 2500, $"Average latency regression detected for {target}: avg={average:F2}ms.");
            Assert.True(max < 6000, $"Peak latency regression detected for {target}: max={max}ms.");
        }
    }

    private HttpClient CreateClient(string role, int institutionType)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            JwtTestHelper.GenerateToken(
                role: role,
                userId: Guid.NewGuid().ToString(),
                institutionType: institutionType));
        return client;
    }

    private static async Task<long> GetIndexReadCountAsync(SqlConnection connection, string tableName, string indexName)
    {
        const string sql = @"
SELECT COALESCE(us.user_seeks, 0) + COALESCE(us.user_scans, 0) + COALESCE(us.user_lookups, 0)
FROM sys.indexes i
LEFT JOIN sys.dm_db_index_usage_stats us
    ON us.database_id = DB_ID()
    AND us.object_id = i.object_id
    AND us.index_id = i.index_id
WHERE i.object_id = OBJECT_ID(@tableName)
  AND i.name = @indexName;";

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@tableName", tableName);
        command.Parameters.AddWithValue("@indexName", indexName);

        var value = await command.ExecuteScalarAsync();
        Assert.NotNull(value);
        return Convert.ToInt64(value);
    }

    private static async Task ExecuteScalarAsync(SqlConnection connection, string sql, params SqlParameter[] parameters)
    {
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddRange(parameters);
        await command.ExecuteScalarAsync();
    }

    private async Task<(Guid CollegeDepartmentId, Guid ActiveSemesterId)> SeedQueryValidationDataAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var suffix = Guid.NewGuid().ToString("N")[..6];

        var collegeDepartment = new Department($"Perf College {suffix}", $"PFC{suffix}", InstitutionType.College);
        var universityDepartment = new Department($"Perf University {suffix}", $"PFU{suffix}", InstitutionType.University);
        db.Departments.AddRange(collegeDepartment, universityDepartment);

        var activeSemester = new Semester($"Perf Active {suffix}", DateTime.UtcNow.Date.AddDays(-20), DateTime.UtcNow.Date.AddDays(60));
        var closedSemester = new Semester($"Perf Closed {suffix}", DateTime.UtcNow.Date.AddDays(-120), DateTime.UtcNow.Date.AddDays(-30));
        closedSemester.Close();
        db.Semesters.AddRange(activeSemester, closedSemester);

        for (var i = 0; i < 60; i++)
        {
            var collegeProgram = new AcademicProgram($"Perf College Program {suffix}-{i}", $"PC{suffix}{i:00}", collegeDepartment.Id, 8);
            if (i % 3 == 0)
            {
                collegeProgram.Deactivate();
            }

            var universityProgram = new AcademicProgram($"Perf University Program {suffix}-{i}", $"PU{suffix}{i:00}", universityDepartment.Id, 8);
            if (i % 4 == 0)
            {
                universityProgram.Deactivate();
            }

            db.AcademicPrograms.AddRange(collegeProgram, universityProgram);

            var collegeCourse = new Course($"Perf College Course {suffix}-{i}", $"CC{suffix}{i:00}", 3, collegeDepartment.Id);
            var universityCourse = new Course($"Perf University Course {suffix}-{i}", $"UC{suffix}{i:00}", 3, universityDepartment.Id);

            if (i % 5 == 0)
            {
                collegeCourse.Deactivate();
            }

            if (i % 6 == 0)
            {
                universityCourse.Deactivate();
            }

            db.Courses.AddRange(collegeCourse, universityCourse);

            var activeOffering = new CourseOffering(collegeCourse.Id, activeSemester.Id, 60);
            var closedOffering = new CourseOffering(universityCourse.Id, closedSemester.Id, 40);
            closedOffering.Close();

            db.CourseOfferings.AddRange(activeOffering, closedOffering);
        }

        await db.SaveChangesAsync();

        return (collegeDepartment.Id, activeSemester.Id);
    }
}
