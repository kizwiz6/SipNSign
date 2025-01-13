namespace com.kizwiz.sipnsign.Services
{
    /// <summary>
    /// Interface for handling sharing functionality in the application
    /// </summary>
    public interface IShareService
    {
        /// <summary>
        /// Shares text content with a specified title
        /// </summary>
        Task ShareTextAsync(string text, string title);
        /// <summary>
        /// Shares an image with a specified title
        /// </summary>
        Task ShareImageAsync(string imagePath, string title);
    }

    /// <summary>
    /// Implementation of sharing functionality using MAUI Share API
    /// </summary>
    public class ShareService : IShareService
    {
        #region Public Methods
        /// <summary>
        /// Shares text content using the native share dialog
        /// </summary>
        public async Task ShareTextAsync(string text, string title)
        {
            try
            {
                await Share.Default.RequestAsync(new ShareTextRequest
                {
                    Text = text,
                    Title = title
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Sharing failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Shares an image file using the native share dialog
        /// </summary>
        public async Task ShareImageAsync(string imagePath, string title)
        {
            try
            {
                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = title,
                    File = new ShareFile(imagePath)
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Image sharing failed: {ex.Message}");
                throw;
            }
        }
        #endregion
    }
}