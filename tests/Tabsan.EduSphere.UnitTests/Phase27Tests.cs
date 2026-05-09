using FluentAssertions;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Application.Services;
using Tabsan.EduSphere.Domain.Enums;
using Xunit;

namespace Tabsan.EduSphere.UnitTests;

// Phase 27 — University Portal Parity and Student Experience Unit Tests

public class PortalCapabilityMatrixServiceTests
{
    [Fact]
    public async Task GetMatrixAsync_ReturnsExpectedCoreRows()
    {
        var sut = new PortalCapabilityMatrixService(new StubModuleRegistryService());
        var policy = new InstitutionPolicySnapshot(IncludeSchool: true, IncludeCollege: true, IncludeUniversity: true);

        var matrix = await sut.GetMatrixAsync(policy);

        matrix.Rows.Should().NotBeEmpty();
        matrix.Rows.Should().Contain(r => r.CapabilityKey == "dashboard_overview");
        matrix.Rows.Should().Contain(r => r.CapabilityKey == "ai_assistant");
    }

    [Fact]
    public async Task FypCapability_IsUniversityOnly()
    {
        var sut = new PortalCapabilityMatrixService(new StubModuleRegistryService());
        var policy = new InstitutionPolicySnapshot(IncludeSchool: true, IncludeCollege: true, IncludeUniversity: true);

        var matrix = await sut.GetMatrixAsync(policy);
        var fyp = matrix.Rows.Single(r => r.CapabilityKey == "fyp_workspace");

        fyp.University.Should().BeTrue();
        fyp.School.Should().BeFalse();
        fyp.College.Should().BeFalse();
    }

    [Fact]
    public async Task AiAssistant_IsLicenseGated()
    {
        var sut = new PortalCapabilityMatrixService(new StubModuleRegistryService());
        var policy = InstitutionPolicySnapshot.Default;

        var matrix = await sut.GetMatrixAsync(policy);
        var ai = matrix.Rows.Single(r => r.CapabilityKey == "ai_assistant");

        ai.IsLicenseGated.Should().BeTrue();
    }

    [Fact]
    public async Task DisabledModule_MarksRoleAccessAsFalse()
    {
        var registry = new StubModuleRegistryService();
        registry.SetInactive("results");
        var sut = new PortalCapabilityMatrixService(registry);

        var matrix = await sut.GetMatrixAsync(InstitutionPolicySnapshot.Default);
        var transcript = matrix.Rows.Single(r => r.CapabilityKey == "results_transcript");

        transcript.IsModuleActive.Should().BeFalse();
        transcript.Student.Should().BeFalse();
        transcript.Faculty.Should().BeFalse();
        transcript.Admin.Should().BeFalse();
        transcript.SuperAdmin.Should().BeFalse();
    }
}

file sealed class StubModuleRegistryService : IModuleRegistryService
{
    private readonly Dictionary<string, bool> _inactive = new(StringComparer.OrdinalIgnoreCase);

    public void SetInactive(string key) => _inactive[key] = true;

    public Task<IReadOnlyList<ModuleVisibilityResult>> GetVisibleModulesAsync(
        string role,
        InstitutionPolicySnapshot policy,
        CancellationToken ct = default)
    {
        var all = new List<ModuleVisibilityResult>
        {
            Make("authentication", "Authentication", role, policy),
            Make("courses", "Courses", role, policy),
            Make("assignments", "Assignments", role, policy),
            Make("quizzes", "Quizzes", role, policy),
            Make("attendance", "Attendance", role, policy),
            Make("results", "Results", role, policy),
            Make("sis", "Student Information", role, policy),
            Make("notifications", "Notifications", role, policy),
            Make("ai_chat", "AI Chatbot", role, policy),
            Make("fyp", "Final Year Projects", role, policy),
            Make("reports", "Reports", role, policy),
        };

        return Task.FromResult<IReadOnlyList<ModuleVisibilityResult>>(all);
    }

    public async Task<bool> IsAccessibleAsync(string key, string role, InstitutionPolicySnapshot policy, CancellationToken ct = default)
    {
        var modules = await GetVisibleModulesAsync(role, policy, ct);
        return modules.Any(m => string.Equals(m.Key, key, StringComparison.OrdinalIgnoreCase) && m.IsActive && m.IsAccessible);
    }

    private ModuleVisibilityResult Make(string key, string name, string role, InstitutionPolicySnapshot policy)
    {
        var active = !_inactive.ContainsKey(key);
        var accessible = IsRoleAccessible(key, role) && IsPolicyAccessible(key, policy);
        return new ModuleVisibilityResult(key, name, active, accessible);
    }

    private static bool IsRoleAccessible(string key, string role)
    {
        return key switch
        {
            "reports" => role is "SuperAdmin" or "Admin" or "Faculty",
            "fyp" => role is "SuperAdmin" or "Admin" or "Faculty" or "Student",
            _ => role is "SuperAdmin" or "Admin" or "Faculty" or "Student"
        };
    }

    private static bool IsPolicyAccessible(string key, InstitutionPolicySnapshot policy)
    {
        if (key.Equals("fyp", StringComparison.OrdinalIgnoreCase))
            return policy.IncludeUniversity;

        return true;
    }
}
