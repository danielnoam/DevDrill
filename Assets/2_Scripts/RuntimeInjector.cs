

using UnityEngine;

internal static class RuntimeInjector
{
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
                SetTargetFPS();
        }

        private static void SetTargetFPS()
        {
                if (Application.isMobilePlatform)
                {
                        Application.targetFrameRate = 120;
                }
        }
}