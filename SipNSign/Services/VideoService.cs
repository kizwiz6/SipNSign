using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.kizwiz.sipnsign.Services
{
    public class VideoService : IVideoService
    {
        private bool _isInitialized = false;
        private readonly string _videoDirectory;
        private readonly SemaphoreSlim _initializationLock = new SemaphoreSlim(1, 1);

        public VideoService()
        {
            _videoDirectory = FileSystem.AppDataDirectory;
            Debug.WriteLine($"Video directory: {_videoDirectory}");
        }

        public async Task InitializeVideos()
        {
            try
            {
                if (_isInitialized)
                {
                    Debug.WriteLine("Videos already initialized");
                    return;
                }

                Debug.WriteLine("Starting video initialization");
                var signs = new SignRepository().GetSigns();
                Debug.WriteLine($"Found {signs.Count} signs to initialize");

                foreach (var sign in signs)
                {
                    string filename = Path.GetFileName(sign.VideoPath);
                    string targetPath = Path.Combine(_videoDirectory, filename);

                    if (!File.Exists(targetPath))
                    {
                        Debug.WriteLine($"Copying video: {filename}");
                        await CopyVideoToAppData(filename);
                    }
                }

                _isInitialized = true;
                Debug.WriteLine("Video initialization complete");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in InitializeVideos: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<string> GetVideoPath(string videoFileName)
        {
            try
            {
                var path = Path.Combine(FileSystem.AppDataDirectory, videoFileName);
                if (!File.Exists(path))
                {
                    Debug.WriteLine($"Video file not found at {path}, attempting to copy");
                    await CopyVideoToAppData(videoFileName);
                }

                if (File.Exists(path))
                {
                    var fileInfo = new FileInfo(path);
                    Debug.WriteLine($"Video file exists at {path}. Size: {fileInfo.Length} bytes");
                    return path;
                }
                else
                {
                    Debug.WriteLine($"Failed to find or copy video file: {videoFileName}");
                    throw new FileNotFoundException($"Video file not found: {videoFileName}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetVideoPath: {ex.Message}");
                throw;
            }
        }

        private async Task CopyVideoToAppData(string videoFileName)
        {
            try
            {
                Debug.WriteLine($"=== Starting video copy: {videoFileName} ===");

                var targetPath = Path.Combine(FileSystem.AppDataDirectory, videoFileName);
                Debug.WriteLine($"Target path: {targetPath}");

                if (File.Exists(targetPath))
                {
                    Debug.WriteLine($"File already exists, size: {new FileInfo(targetPath).Length} bytes");
                    return;
                }

                using var stream = await FileSystem.OpenAppPackageFileAsync(videoFileName);
                if (stream == null)
                {
                    Debug.WriteLine("Failed to open source video file!");
                    return;
                }

                using var fileStream = File.Create(targetPath);
                await stream.CopyToAsync(fileStream);

                if (File.Exists(targetPath))
                {
                    Debug.WriteLine($"File copied successfully, size: {new FileInfo(targetPath).Length} bytes");
                }
                else
                {
                    Debug.WriteLine("File copy failed - file does not exist after copy");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error copying video: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
