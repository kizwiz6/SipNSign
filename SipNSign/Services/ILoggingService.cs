namespace com.kizwiz.sipnsign.Services
{
    public interface ILoggingService
    {
        void Debug(string message);
        void Error(string message);
        void Info(string message);
        /// <summary>
        /// Retrieves the log file contents for viewing in the app
        /// </summary>
        /// <returns>The contents of the log file</returns>
        Task<string> GetLogContents();

        /// <summary>
        /// Cleans up old log files
        /// </summary>
        Task CleanupLogs();
    }
}
