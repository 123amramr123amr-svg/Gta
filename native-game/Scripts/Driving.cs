using System;
using System.Runtime.InteropServices;

// Uses C++ on devices where libdriving.so is bundled. A matching C# fallback
// keeps editor builds and APKs without the optional library playable.
public static class Driving
{
    [DllImport("driving", EntryPoint = "driving_step")]
    private static extern float NativeStep(float speed, float throttle, float dt, int handbrake);
    private static bool nativeAvailable = true;

    public static float Step(float speed, float throttle, float dt, bool brake)
    {
        if (nativeAvailable)
        {
            try { return NativeStep(speed, throttle, dt, brake ? 1 : 0); }
            catch (DllNotFoundException) { nativeAvailable = false; }
            catch (EntryPointNotFoundException) { nativeAvailable = false; }
            catch (BadImageFormatException) { nativeAvailable = false; }
        }
        dt = Math.Clamp(dt, 0f, 0.05f);
        throttle = Math.Clamp(throttle, -1f, 1f);
        speed += throttle * 13f * dt;
        speed *= MathF.Exp(-(brake ? 1.5f : 0.38f) * dt);
        return Math.Clamp(speed, -8f, 24f);
    }
}
