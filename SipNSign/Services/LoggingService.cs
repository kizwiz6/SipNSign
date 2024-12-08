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

        public void Debug(string message)
        {
#if ANDROID
            Android.Util.Log.Debug(TAG, message);
#else
            System.Diagnostics.Debug.WriteLine($"Debug: {message}");
#endif
        }

        public void Error(string message)
        {
#if ANDROID
            Android.Util.Log.Error(TAG, message);
#else
            System.Diagnostics.Debug.WriteLine($"Error: {message}");
#endif
        }

        public void Info(string message)
        {
#if ANDROID
            Android.Util.Log.Info(TAG, message);
#else
            System.Diagnostics.Debug.WriteLine($"Info: {message}");
#endif
        }
    }
}
