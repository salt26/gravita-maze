using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using Interhaptics.Internal;

public class HapticMediate : MonoBehaviour
{
    public EventHapticSource[] eventHapticSource;
#if UNITY_ANDROID && !UNITY_EDITOR
    private GameObject _hapticManager;
    private GameObject _hapticEvent;
    private GameObject _hapticBodyPart;
#endif
    private void Awake()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        _hapticManager = transform.GetChild(1).gameObject;
        _hapticManager.SetActive(true);

        _hapticBodyPart = transform.GetChild(0).gameObject;
        _hapticBodyPart.SetActive(true);

        _hapticEvent = transform.GetChild(2).gameObject;
        _hapticEvent.SetActive(true);
#endif
    }
     
    public void hmPlayHapticOnce(float delayTime, int num)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        eventHapticSource[num].delayPlay = delayTime;
        eventHapticSource[num].PlayEventVibration();
#endif
    }
    public void hmStopHaptic(int num)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        eventHapticSource[num].Stop();
#endif
    }
    public void hmPlayHaptic(int num)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        eventHapticSource[num].Play();
#endif
    }

}
