using Il2Cpp;
using MelonLoader;
using System.Collections;
using UnityEngine;

namespace Striker.BrightnessFix
{
    public class BrightnessFix : MelonMod
    {
        float defaultExposure;
        float targetExposure;
        int lastBrightnessSetting = -999;

        bool isAdjusting = false;

        public override void OnSceneWasLoaded(int buildindex, string SceneName)
        {
            if (GodConstant.Instance != null)
            {
                MelonCoroutines.Start(SceneExposureRoutine(0.5f)); // delay to allow subscenes to load
            }
        }

        public override void OnUpdate()
        {
            if (GodConstant.Instance == null || isAdjusting) return;

            int currentBrightnessSetting = GodConstant.Instance.gameSettings.image_brightness;

            if (currentBrightnessSetting != lastBrightnessSetting)
            {
                lastBrightnessSetting = currentBrightnessSetting;
                ApplyExposure(currentBrightnessSetting);
            }
        }

        IEnumerator SceneExposureRoutine(float delay)
        {
            isAdjusting = true;
            yield return new WaitForSeconds(delay);

            if (GodConstant.Instance != null)
            {
                float liveValue = GodConstant.Instance.local_colorGradeStartingExposure;
                float currentOffset = (lastBrightnessSetting == -999) ? 0 : (float)lastBrightnessSetting / 10f; // dont ask

                if (Mathf.Abs(liveValue - targetExposure) > 0.001f) // to account for float shenanigans
                {
                    defaultExposure = liveValue + 0.001f;
                }
                else
                {
                    defaultExposure = liveValue - currentOffset + 0.001f;
                }

                ApplyExposure(lastBrightnessSetting);
            }
            isAdjusting = false;
        }

        void ApplyExposure(int brightnessSetting)
        {
            float offset = (float)brightnessSetting / 10f;
            targetExposure = defaultExposure + offset;

            GodConstant.Instance.local_colorGradeStartingExposure = targetExposure;
            GodConstant.Instance.targetExposure = targetExposure;
            GodConstant.Instance.apply_brightnessLocal(null);

            MelonDebug.Msg($"Recalibrated: Base({defaultExposure}) + Offset({offset}) = {targetExposure}");
        }
    }
}