using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Services;

public sealed class AnalyticsExportJobWorker : BackgroundService
{
    private readonly AnalyticsExportJobQueue _queue;
    private readonly AnalyticsExportJobStore _store;
    private readonly IServiceProvider _services;
    private readonly ILogger<AnalyticsExportJobWorker> _logger;

    public AnalyticsExportJobWorker(
        AnalyticsExportJobQueue queue,
        AnalyticsExportJobStore store,
        IServiceProvider services,
        ILogger<AnalyticsExportJobWorker> logger)
    {
        _queue = queue;
        _store = store;
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var request in _queue.DequeueAllAsync(stoppingToken))
        {
            try
            {
                await _store.SetStateAsync(new AnalyticsExportJobState
                {
                    JobId = request.JobId,
                    RequestedByUserId = request.RequestedByUserId,
                    ReportType = request.ReportType,
                    Format = request.Format,
                    Status = "running"
                }, stoppingToken);

                using var scope = _services.CreateScope();
                var analytics = scope.ServiceProvider.GetRequiredService<IAnalyticsService>();

                byte[] bytes;
                string contentType;
                string extension;

                switch (request.ReportType)
                {
                    case AnalyticsExportReportType.Attendance when request.Format == AnalyticsExportFormat.Excel:
                        bytes = await analytics.ExportAttendanceExcelAsync(request.DepartmentId, request.InstitutionType, stoppingToken);
                        contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        extension = "xlsx";
                        break;
                    case AnalyticsExportReportType.Attendance:
                        bytes = await analytics.ExportAttendancePdfAsync(request.DepartmentId, request.InstitutionType, stoppingToken);
                        contentType = "application/pdf";
                        extension = "pdf";
                        break;
                    case AnalyticsExportReportType.Performance when request.Format == AnalyticsExportFormat.Excel:
                        bytes = await analytics.ExportPerformanceExcelAsync(request.DepartmentId, request.InstitutionType, stoppingToken);
                        contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        extension = "xlsx";
                        break;
                    default:
                        bytes = await analytics.ExportPerformancePdfAsync(request.DepartmentId, request.InstitutionType, stoppingToken);
                        contentType = "application/pdf";
                        extension = "pdf";
                        break;
                }

                var fileName = $"analytics-{request.ReportType.ToString().ToLowerInvariant()}-{request.JobId:N}.{extension}";
                await _store.SetPayloadAsync(request.JobId, bytes, stoppingToken);
                await _store.SetStateAsync(new AnalyticsExportJobState
                {
                    JobId = request.JobId,
                    RequestedByUserId = request.RequestedByUserId,
                    ReportType = request.ReportType,
                    Format = request.Format,
                    Status = "completed",
                    ContentType = contentType,
                    FileName = fileName,
                    CompletedAt = DateTimeOffset.UtcNow
                }, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Analytics export job {JobId} failed.", request.JobId);
                await _store.SetStateAsync(new AnalyticsExportJobState
                {
                    JobId = request.JobId,
                    RequestedByUserId = request.RequestedByUserId,
                    ReportType = request.ReportType,
                    Format = request.Format,
                    Status = "failed",
                    Error = ex.Message,
                    CompletedAt = DateTimeOffset.UtcNow
                }, stoppingToken);
            }
        }
    }
}
