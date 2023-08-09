/* ​
* Copyright (c) 2023 Go Touch VR SAS. All rights reserved. ​
* ​
*/

namespace Interhaptics
{

    [UnityEngine.AddComponentMenu("Interhaptics/HapticManager")]
    public sealed class HapticManager : Internal.Singleton<HapticManager>
    {
        public void Init()
        {
            Core.HAR.Init();
#if UNITY_ANDROID && !UNITY_EDITOR
            Internal.HapticDeviceManager.DeviceInitLoop();
#endif

#if UNITY_ANDROID && !ENABLE_METAQUEST
            Platforms.Mobile.GenericAndroidHapticAbstraction.Initialize();
#elif UNITY_IPHONE
            UnityCoreHaptics.UnityCoreHapticsProxy.CreateEngine();
#endif
        }

        override protected void OnAwake()
        {
#if !UNITY_EDITOR_OSX
            Init();
#endif
        }

        private void LateUpdate()
        {
            //Compute all haptics event
            Core.HAR.ComputeAllEvents(UnityEngine.Time.realtimeSinceStartup);
            //Insert device rendering loop here
#if UNITY_ANDROID && !UNITY_EDITOR
            Internal.HapticDeviceManager.DeviceRenderLoop();
#endif
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            // TODO pause the haptic playback
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            // TODO pause the haptic playback
        }

        protected override void OnOnApplicationQuit()
        {
#if !UNITY_EDITOR_OSX 
#if UNITY_ANDROID && !UNITY_EDITOR
            Internal.HapticDeviceManager.DeviceCleanLoop();
#endif
            Core.HAR.ClearActiveEvents();
            Core.HAR.ClearInactiveEvents();
            Core.HAR.Quit();
#endif
        }

    }

}