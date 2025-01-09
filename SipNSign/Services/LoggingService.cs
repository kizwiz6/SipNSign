using com.kizwiz.sipnsign.Services;
using System.Text;

public class LoggingService : ILoggingService
{
    private const string TAG = "SipNSignApp";
    private readonly string _logFile;
    private const int MAX_LOG_AGE_DAYS = 7;

    /// <summary>
    /// Initializes a new instance of the LoggingService class
    /// </summary>
    public LoggingService()
    {
        _logFile = Path.Combine(FileSystem.AppDataDirectory, $"app_{DateTime.Now:yyyyMMdd}.log");
        // Ensure the directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(_logFile));
    }

    /// <summary>
    /// Writes a message to the log file
    /// </summary>
    /// <param name="level">The log level (DEBUG, ERROR, INFO)</param>
    /// <param name="message">The message to log</param>
    private async Task WriteToLogFile(string level, string message)
    {
        try
        {
            var logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}{Environment.NewLine}";
            await File.AppendAllTextAsync(_logFile, logMessage);
            System.Diagnostics.Debug.WriteLine($"[{level}] {message}"); // Also write to Debug for when connected
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error writing to log file: {ex.Message}");
        }
    }

    /// <summary>
    /// Logs a debug message
    /// </summary>
    /// <param name="message">The message to log</param>
    public async void Debug(string message)
    {
#if ANDROID
        try
        {
            Android.Util.Log.Debug(TAG, message);
            System.Console.WriteLine($"Debug: {message}");
            await WriteToLogFile("DEBUG", message);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Logging failed: {ex.Message}");
        }
#else
        await WriteToLogFile("DEBUG", message);
#endif
    }

    /// <summary>
    /// Logs an error message
    /// </summary>
    /// <param name="message">The message to log</param>
    public async void Error(string message)
    {
#if ANDROID
        try
        {
            Android.Util.Log.Error(TAG, message);
            System.Console.WriteLine($"Error: {message}");
            await WriteToLogFile("ERROR", message);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Logging failed: {ex.Message}");
        }
#else
        await WriteToLogFile("ERROR", message);
#endif
    }

    /// <summary>
    /// Logs an info message
    /// </summary>
    /// <param name="message">The message to log</param>
    public async void Info(string message)
    {
#if ANDROID
        Android.Util.Log.Info(TAG, message);
        await WriteToLogFile("INFO", message);
#else
        await WriteToLogFile("INFO", message);
#endif
    }

    /// <summary>
    /// Retrieves the contents of all log files
    /// </summary>
    /// <returns>A string containing all log contents</returns>
    public async Task<string> GetLogContents()
    {
        try
        {
            var logFiles = Directory.GetFiles(FileSystem.AppDataDirectory, "app_*.log");
            var allLogs = new StringBuilder();

            foreach (var file in logFiles.OrderByDescending(f => f))
            {
                allLogs.AppendLine($"=== {Path.GetFileName(file)} ===");
                allLogs.AppendLine(await File.ReadAllTextAsync(file));
                allLogs.AppendLine();
            }

            return allLogs.Length > 0 ? allLogs.ToString() : "No logs available";
        }
        catch (Exception ex)
        {
            return $"Error reading logs: {ex.Message}";
        }
    }

    /// <summary>
    /// Removes log files older than MAX_LOG_AGE_DAYS
    /// </summary>
    public async Task CleanupLogs()
    {
        try
        {
            var logDirectory = Path.GetDirectoryName(_logFile);
            var currentFileName = Path.GetFileName(_logFile);

            // Delete all existing log files
            var logFiles = Directory.GetFiles(FileSystem.AppDataDirectory, "app_*.log");
            foreach (var file in logFiles)
            {
                await Task.Run(() => File.Delete(file));
            }

            // Create a new empty log file
            var newLogFile = Path.Combine(logDirectory, $"app_{DateTime.Now:yyyyMMdd}.log");
            await File.WriteAllTextAsync(newLogFile, ""); // Create empty file

            await WriteToLogFile("INFO", "Log files cleared successfully");
        }
        catch (Exception ex)
        {
            await WriteToLogFile("ERROR", $"Error clearing logs: {ex.Message}");
            throw;
        }
    }
}