namespace com.kizwiz.sipnsign.Services
{
    /// <summary>
    /// Interface for managing video content and playback
    /// </summary>
    public interface IVideoService
    {
        /// <summary>
        /// Initializes video files by copying them to app storage
        /// </summary>
        Task InitializeVideos();
        /// <summary>
        /// Gets the full path for a video file
        /// </summary>
        /// <param name="videoFileName">Name of the video file</param>
        Task<string> GetVideoPath(string videoFileName);
    }
}
