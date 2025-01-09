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
            Debug.WriteLine($"Video directory initialized: {_videoDirectory}");
        }

        public async Task InitializeVideos()
        {
            try
            {
                await _initializationLock.WaitAsync();

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
                throw;
            }
            finally
            {
                _initializationLock.Release();
            }
        }

        public async Task<string> GetVideoPath(string videoFileName)
        {
            try
            {
                if (!_isInitialized)
                {
                    await InitializeVideos();
                }

                Debug.WriteLine($"Attempting to get video path for: {videoFileName}");
                var path = Path.Combine(_videoDirectory, videoFileName);
                Debug.WriteLine($"Combined path: {path}");
                Debug.WriteLine($"File exists: {File.Exists(path)}");

                return File.Exists(path) ? path : throw new FileNotFoundException($"Video not found: {videoFileName}");
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
    }
}