using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.kizwiz.sipnsign.Services
{
    public class LoggingService : ILoggingService
    {
        private const string TAG = "SipNSignApp";
        private readonly string _logFile;

        public LoggingService()
        {
            _logFile = Path.Combine(FileSystem.AppDataDirectory, "app.log");
        }

        private async Task WriteToLogFile(string level, string message)
        {
            try
            {
                var logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}{Environment.NewLine}";
                await File.AppendAllTextAsync(_logFile, logMessage);
            }
            catch
            {
                // Fail silently if we can't write to the log file
            }
        }

        public void Debug(string message)
        {
#if ANDROID
        try
        {
            // Use Android's native logging
            Android.Util.Log.Debug(TAG, message);
            // Also try to write to system log
            System.Console.WriteLine($"Debug: {message}");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Logging failed: {ex.Message}");
        }
#else
            System.Diagnostics.Debug.WriteLine($"Debug: {message}");
#endif
        }

        public void Error(string message)
        {
#if ANDROID
        try
        {
            Android.Util.Log.Error(TAG, message);
            System.Console.WriteLine($"Error: {message}");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Logging failed: {ex.Message}");
        }
#else
            System.Diagnostics.Debug.WriteLine($"Error: {message}");
#endif
        }

        public async void Info(string message)
        {
#if ANDROID
        Android.Util.Log.Info(TAG, message);
#else
            System.Diagnostics.Debug.WriteLine($"Info: {message}");
#endif
            await WriteToLogFile("INFO", message);
        }
    }
}
