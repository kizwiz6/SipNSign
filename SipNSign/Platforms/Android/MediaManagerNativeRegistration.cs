using System;

namespace com.kizwiz.sipnsign.Platforms.Android
{
    // Minimal safe stub so builds do not fail while we decide on a long-term fix.
    // The previous runtime registration approach caused platform-specific type issues.
    internal static class MediaManagerNativeRegistration
    {
        public static void Register()
        {
            // Intentionally empty — no-op placeholder.
            // Remove this file completely once you implement a tested JNI registration
            // or resolve the upstream dependency mismatch in CommunityToolkit.MediaElement.
        }
    }
}