using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedObject : MonoBehaviour
{
    public enum Type { Fire = 3, QuitGame = 11, MapEditor = 12, Setting = 13, Adventure = 21, Tutorial = 22, Custom = 23, Training = 24, 
        AdvEasy = 31, AdvNormal = 32, AdvHard = 33, AdvInsane = 34 }
    public Type type;
}
