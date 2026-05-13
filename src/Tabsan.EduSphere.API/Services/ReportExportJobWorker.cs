using Tabsan.EduSphere.Application.DTOs.Reports;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Services;

public sealed class ReportExportJobWorker : BackgroundService
{
    private readonly ReportExportJobQueue _queue;
    private readonly ReportExportJobStore _store;
    private readonly IServiceProvider _services;
    private readonly ILogger<ReportExportJobWorker> _logger;

    public ReportExportJobWorker(
        ReportExportJobQueue queue,
        ReportExportJobStore store,
        IServiceProvider services,
        ILogger<ReportExportJobWorker> logger)
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
                await _store.SetStateAsync(new ReportExportJobState
                {
                    JobId = request.JobId,
                    RequestedByUserId = request.RequestedByUserId,
                    Status = "running"
                }, stoppingToken);

                using var scope = _services.CreateScope();
                var reportService = scope.ServiceProvider.GetRequiredService<IReportService>();
                var reportRequest = new ResultSummaryRequest(request.SemesterId, request.DepartmentId, request.CourseOfferingId, request.StudentProfileId, request.InstitutionType);

                byte[] bytes;
                string contentType;
                string extension;
                switch (request.Format)
                {
                    case ReportExportFormat.Csv:
                        bytes = await reportService.ExportResultSummaryCsvAsync(reportRequest, stoppingToken);
                        contentType = "text/csv";
                        extension = "csv";
                        break;
                    case ReportExportFormat.Pdf:
                        bytes = await reportService.ExportResultSummaryPdfAsync(reportRequest, stoppingToken);
                        contentType = "application/pdf";
                        extension = "pdf";
                        break;
                    default:
                        bytes = await reportService.ExportResultSummaryExcelAsync(reportRequest, stoppingToken);
                        contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        extension = "xlsx";
                        break;
                }

                var fileName = $"result-summary-{request.JobId:N}.{extension}";
                await _store.SetPayloadAsync(request.JobId, bytes, stoppingToken);
                await _store.SetStateAsync(new ReportExportJobState
                {
                    JobId = request.JobId,
                    RequestedByUserId = request.RequestedByUserId,
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
                _logger.LogError(ex, "Report export job {JobId} failed.", request.JobId);
                await _store.SetStateAsync(new ReportExportJobState
                {
                    JobId = request.JobId,
                    RequestedByUserId = request.RequestedByUserId,
                    Status = "failed",
                    Error = ex.Message,
                    CompletedAt = DateTimeOffset.UtcNow
                }, stoppingToken);
            }
        }
    }
}