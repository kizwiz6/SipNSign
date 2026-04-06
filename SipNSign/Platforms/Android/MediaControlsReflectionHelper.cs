using com.kizwiz.sipnsign.Platforms.Android;
using System.Reflection;
using System;

namespace com.kizwiz.sipnsign.Platforms.Android
{
    internal static class MediaControlsReflectionHelper
    {
        // Try to safely null internal AudioManager / listener fields and then dispose the instance.
        // This is a defensive, temporary workaround until the upstream toolkit fixes the disposal ordering.
        public static void SanitizeAndDisposeMediaControlsInstance(object instance)
        {
            if (instance == null) return;

            try
            {
                var type = instance.GetType();

                // Try common field/property names used for audio manager / listener in various versions.
                var candidateNames = new[]
                {
                    "audioManager",
                    "_audioManager",
                    "audioManagerField",
                    "audioFocusChangeListener",
                    "_audioFocusChangeListener",
                    "audioFocusListener",
                    "_audioFocusListener",
                    "audioManagerListener"
                };

                foreach (var name in candidateNames)
                {
                    try
                    {
                        // Instance field
                        var f = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                        if (f != null)
                        {
                            try { f.SetValue(instance, null); } catch { /* swallow */ }
                        }

                        // Instance property
                        var p = type.GetProperty(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                        if (p != null && p.CanWrite)
                        {
                            try { p.SetValue(instance, null); } catch { /* swallow */ }
                        }
                    }
                    catch
                    {
                        // swallow per-field errors
                    }
                }

                // Try a generic stop/unregister API before Dispose (if present)
                try
                {
                    var stop = type.GetMethod("Stop", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    stop?.Invoke(instance, Array.Empty<object>());
                }
                catch { }

                try
                {
                    var unregister = type.GetMethod("Unregister", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    unregister?.Invoke(instance, Array.Empty<object>());
                }
                catch { }

                // Finally, attempt Dispose if available
                try
                {
                    if (instance is IDisposable d)
                    {
                        try { d.Dispose(); } catch { /* swallow */ }
                    }
                    else
                    {
                        var dispose = type.GetMethod("Dispose", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        dispose?.Invoke(instance, Array.Empty<object>());
                    }
                }
                catch
                {
                    // swallow dispose errors
                }
            }
            catch
            {
                // overall safety net
            }
        }

        // Scans loaded assemblies, finds MediaControlsService, and sanitizes its singleton instance(s)
        public static void SanitizeAllMediaControlsServices()
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var asmName = asm.GetName().Name;
                    if (string.IsNullOrEmpty(asmName)) continue;

                    if (!(asmName.StartsWith("CommunityToolkit.Maui.Media", StringComparison.OrdinalIgnoreCase) ||
                          asmName.StartsWith("CommunityToolkit.Maui", StringComparison.OrdinalIgnoreCase)))
                        continue;

                    var svcType = asm.GetType("CommunityToolkit.Maui.Media.Services.MediaControlsService");
                    if (svcType == null) continue;

                    // Try static Instance property / field patterns
                    object? instance = null;
                    try
                    {
                        var prop = svcType.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public);
                        if (prop != null) instance = prop.GetValue(null);
                    }
                    catch { }

                    if (instance == null)
                    {
                        try
                        {
                            var field = svcType.GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic)
                                     ?? svcType.GetField("Instance", BindingFlags.Static | BindingFlags.NonPublic);
                            if (field != null) instance = field.GetValue(null);
                        }
                        catch { }
                    }

                    if (instance != null)
                    {
                        SanitizeAndDisposeMediaControlsInstance(instance);
                    }

                    // Also try to find any other static fields that might hold service instances
                    try
                    {
                        var staticFields = svcType.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                        foreach (var sf in staticFields)
                        {
                            try
                            {
                                var val = sf.GetValue(null);
                                if (val != null)
                                    SanitizeAndDisposeMediaControlsInstance(val);
                            }
                            catch { }
                        }
                    }
                    catch { }
                }
                catch { }
            }
        }
    }
}