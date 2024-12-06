using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.kizwiz.sipnsign.Services
{
    public interface IVideoService
    {
        Task InitializeVideos();
        Task<string> GetVideoPath(string videoFileName);
    }
}
