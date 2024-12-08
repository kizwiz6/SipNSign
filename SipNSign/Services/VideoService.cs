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
        public async Task InitializeVideos()
        {
            try
            {
                var videoFiles = Directory.GetFiles(FileSystem.AppDataDirectory, "*.mp4");
                foreach (var file in videoFiles)
                {
                    File.Delete(file);
                }

                var signs = new SignRepository().GetSigns();
                foreach (var sign in signs)
                {
                    string filename = Path.GetFileName(sign.VideoPath);
                    await CopyVideoToAppData(filename);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing videos: {ex.Message}");
            }
        }

        public async Task<string> GetVideoPath(string videoFileName)
        {
            string localPath = Path.Combine(FileSystem.AppDataDirectory, videoFileName);
            if (!File.Exists(localPath))
            {
                await CopyVideoToAppData(videoFileName);
            }
            return localPath;
        }

        private async Task CopyVideoToAppData(string videoFileName)
        {
            try
            {
                Debug.WriteLine($"Attempting to copy video: {videoFileName}");
                using var stream = await FileSystem.OpenAppPackageFileAsync(videoFileName);
                var targetPath = Path.Combine(FileSystem.AppDataDirectory, videoFileName);

                using var fileStream = File.Create(targetPath);
                await stream.CopyToAsync(fileStream);

                Debug.WriteLine($"Successfully copied video to: {targetPath}");
                Debug.WriteLine($"File exists: {File.Exists(targetPath)}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error copying video file {videoFileName}: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
