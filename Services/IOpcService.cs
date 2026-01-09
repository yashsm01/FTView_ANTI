namespace AlarmMonitor.Services
{
    public interface IOpcService
    {
        Task<string> ReadTagValueAsync(string tagName);
        Task<bool> IsConnectedAsync();
    }
}
