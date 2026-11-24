#include <algorithm>
#include <cmath>

// Stateless C ABI used by the C# prototype. Speed is in metres/second.
extern "C" __attribute__((visibility("default"))) float driving_step(
    float speed, float throttle, float dt, int handbrake) {
    dt = std::clamp(dt, 0.0f, 0.05f);
    throttle = std::clamp(throttle, -1.0f, 1.0f);
    speed += throttle * 13.0f * dt;
    speed *= std::exp(-(handbrake ? 1.5f : 0.38f) * dt);
    return std::clamp(speed, -8.0f, 24.0f);
}
