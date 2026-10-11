using UnityEngine;

namespace KaramaniReborn
{
    // SotN kompozisyonu, The Last Faith görüntüsü: dünya PixelPipeline ile 360x270 piksele indirilir.
    // Tek ve eşit bir izdüşüm çizimleri, fiziği ve arayüzü aynı oranda tutar.
    public static class GameView
    {
        public const float Aspect = 4f / 3;
        public const float OrthographicSize = 5.4f;
        public const float GroundCameraY = -.4f;
        public const int CanvasWidth = 1920, CanvasHeight = 1440;
        public const int RenderWidth = 1440, RenderHeight = 1080;
        public const int MotionWidth = 960, MotionHeight = 720;
        public const float MiddleOffset = (CanvasHeight - 1080) * .5f;
        public const float BottomOffset = CanvasHeight - 1080;

        public static Rect ViewportFor(int screenWidth, int screenHeight)
        {
            float screenAspect = Mathf.Max(1, screenWidth) / (float)Mathf.Max(1, screenHeight);
            return screenAspect > Aspect
                ? new Rect((1 - Aspect / screenAspect) * .5f, 0, Aspect / screenAspect, 1)
                : new Rect(0, (1 - screenAspect / Aspect) * .5f, 1, screenAspect / Aspect);
        }

        public static void Fit(Camera camera, int screenWidth, int screenHeight)
        {
            camera.rect = ViewportFor(screenWidth, screenHeight);
            camera.aspect = Aspect;
        }
    }
}
