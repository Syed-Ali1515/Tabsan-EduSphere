namespace Tabsan.EduSphere.API.Services;

public sealed class BackgroundJobReliabilityOptions
{
    public const string SectionName = "BackgroundJobReliability";

    public int MaxRetryAttempts { get; set; } = 3;
    public int BaseDelayMilliseconds { get; set; } = 250;
    public int AlertConsecutiveFailureThreshold { get; set; } = 3;
}

public sealed class BackgroundJobHealthTracker
{
    private long _resultPublishProcessed;
    private long _resultPublishSucceeded;
    private long _resultPublishFailed;
    private long _resultPublishRetried;
    private long _resultPublishConsecutiveFailures;

    private long _reportExportProcessed;
    private long _reportExportSucceeded;
    private long _reportExportFailed;
    private long _reportExportRetried;
    private long _reportExportConsecutiveFailures;

    private long _analyticsExportProcessed;
    private long _analyticsExportSucceeded;
    private long _analyticsExportFailed;
    private long _analyticsExportRetried;
    private long _analyticsExportConsecutiveFailures;

    public void RecordResultPublishSuccess()
    {
        Interlocked.Increment(ref _resultPublishProcessed);
        Interlocked.Increment(ref _resultPublishSucceeded);
        Interlocked.Exchange(ref _resultPublishConsecutiveFailures, 0);
    }

    public void RecordResultPublishFailure()
    {
        Interlocked.Increment(ref _resultPublishProcessed);
        Interlocked.Increment(ref _resultPublishFailed);
        Interlocked.Increment(ref _resultPublishConsecutiveFailures);
    }

    public void RecordResultPublishRetry() => Interlocked.Increment(ref _resultPublishRetried);

    public long GetResultPublishConsecutiveFailures() => Volatile.Read(ref _resultPublishConsecutiveFailures);

    public void RecordReportExportSuccess()
    {
        Interlocked.Increment(ref _reportExportProcessed);
        Interlocked.Increment(ref _reportExportSucceeded);
        Interlocked.Exchange(ref _reportExportConsecutiveFailures, 0);
    }

    public void RecordReportExportFailure()
    {
        Interlocked.Increment(ref _reportExportProcessed);
        Interlocked.Increment(ref _reportExportFailed);
        Interlocked.Increment(ref _reportExportConsecutiveFailures);
    }

    public void RecordReportExportRetry() => Interlocked.Increment(ref _reportExportRetried);

    public long GetReportExportConsecutiveFailures() => Volatile.Read(ref _reportExportConsecutiveFailures);

    public void RecordAnalyticsExportSuccess()
    {
        Interlocked.Increment(ref _analyticsExportProcessed);
        Interlocked.Increment(ref _analyticsExportSucceeded);
        Interlocked.Exchange(ref _analyticsExportConsecutiveFailures, 0);
    }

    public void RecordAnalyticsExportFailure()
    {
        Interlocked.Increment(ref _analyticsExportProcessed);
        Interlocked.Increment(ref _analyticsExportFailed);
        Interlocked.Increment(ref _analyticsExportConsecutiveFailures);
    }

    public void RecordAnalyticsExportRetry() => Interlocked.Increment(ref _analyticsExportRetried);

    public long GetAnalyticsExportConsecutiveFailures() => Volatile.Read(ref _analyticsExportConsecutiveFailures);

    public object GetSnapshot()
    {
        return new
        {
            resultPublish = new
            {
                processed = Volatile.Read(ref _resultPublishProcessed),
                succeeded = Volatile.Read(ref _resultPublishSucceeded),
                failed = Volatile.Read(ref _resultPublishFailed),
                retried = Volatile.Read(ref _resultPublishRetried),
                consecutiveFailures = Volatile.Read(ref _resultPublishConsecutiveFailures)
            },
            reportExport = new
            {
                processed = Volatile.Read(ref _reportExportProcessed),
                succeeded = Volatile.Read(ref _reportExportSucceeded),
                failed = Volatile.Read(ref _reportExportFailed),
                retried = Volatile.Read(ref _reportExportRetried),
                consecutiveFailures = Volatile.Read(ref _reportExportConsecutiveFailures)
            },
            analyticsExport = new
            {
                processed = Volatile.Read(ref _analyticsExportProcessed),
                succeeded = Volatile.Read(ref _analyticsExportSucceeded),
                failed = Volatile.Read(ref _analyticsExportFailed),
                retried = Volatile.Read(ref _analyticsExportRetried),
                consecutiveFailures = Volatile.Read(ref _analyticsExportConsecutiveFailures)
            }
        };
    }
}