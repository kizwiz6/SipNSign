namespace com.kizwiz.sipnsign.Services
{
    public interface IShareService
    {
        Task ShareTextAsync(string text, string title);
        Task ShareImageAsync(string imagePath, string title);
    }

    public class ShareService : IShareService
    {
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
    }
}