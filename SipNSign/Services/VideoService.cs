using System.Diagnostics;

namespace com.kizwiz.sipnsign.Services
{
    /// <summary>
    /// Service for managing video file operations and initialization
    /// </summary>
    public class VideoService : IVideoService
    {
        #region Fields
        private bool _isInitialized = false;
        private readonly string _videoDirectory;
        private readonly SemaphoreSlim _initializationLock = new SemaphoreSlim(1, 1);
        private readonly ILoggingService _logger;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes the video service with app's data directory
        /// </summary>
        public VideoService(ILoggingService logger)
        {
            _logger = logger;
            _videoDirectory = FileSystem.AppDataDirectory;
            Debug.WriteLine($"Video directory initialized: {_videoDirectory}");
        }
        #endregion

        #region Public Methods
        public Task InitializeVideos()
        {
            // No need to copy files on Android as we access raw resources directly
            _isInitialized = true;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets the full path of a video file
        /// </summary>
        /// <param name="videoFileName">Name of the video file to locate</param>
        /// <returns>Full path to the video file</returns>
        public async Task<string> GetVideoPath(string videoFileName)
        {
            try
            {
                _logger.Debug($"GetVideoPath called for: {videoFileName}");

#if ANDROID
                var context = Android.App.Application.Context;
                var resourceName = Path.GetFileNameWithoutExtension(videoFileName).ToLower();
                var resourceId = context.Resources.GetIdentifier(
                    resourceName,
                    "raw",
                    context.PackageName);

                if (resourceId == 0)
                {
                    _logger.Error($"Resource not found for: {videoFileName}");
                    throw new FileNotFoundException($"Video resource not found: {videoFileName}");
                }

                var uri = $"android.resource://{context.PackageName}/{resourceId}";
                _logger.Debug($"Resource URI created: {uri}");
                return uri;
#else
                var assetPath = $"Resources/Raw/{videoFileName}";
                _logger.Debug($"Looking for video at: {assetPath}");

                using var stream = await FileSystem.OpenAppPackageFileAsync(videoFileName);
                var tempPath = Path.Combine(_videoDirectory, videoFileName);

                using (var fileStream = File.Create(tempPath))
                {
                    await stream.CopyToAsync(fileStream);
                }

                _logger.Debug($"Video copied to: {tempPath}");
                return tempPath;
#endif
            }
            catch (Exception ex)
            {
                _logger.Error($"Error in GetVideoPath: {ex.Message}");
                _logger.Error($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Copies a video file from app package to local storage
        /// </summary>
        private async Task CopyVideoToAppData(string videoFileName)
        {
            try
            {
                var targetPath = Path.Combine(_videoDirectory, videoFileName);

                if (File.Exists(targetPath))
                {
                    Debug.WriteLine($"File already exists: {targetPath}");
                    return;
                }

                using var sourceStream = await FileSystem.OpenAppPackageFileAsync(videoFileName);
                if (sourceStream == null)
                {
                    throw new FileNotFoundException($"Video file not found in app package: {videoFileName}");
                }

                using var fileStream = File.Create(targetPath);
                await sourceStream.CopyToAsync(fileStream);

                if (!File.Exists(targetPath))
                {
                    throw new IOException($"Failed to create video file: {videoFileName}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error copying video {videoFileName}: {ex.Message}");
                throw; 
            }
        }
        #endregion
    }
}