namespace JobApplication.Application.Interfaces
{
    /// <summary>
    /// Notifies a candidate about a change relevant to them (e.g. their application
    /// was closed because the job expired). Designed to be invoked as a Hangfire
    /// background job via IBackgroundJobClient.Enqueue&lt;INotificationService&gt;(...).
    /// </summary>
    public interface INotificationService
    {
        Task NotifyCandidate(int candidateId);
    }
}
