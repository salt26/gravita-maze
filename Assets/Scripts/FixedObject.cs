using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedObject : MonoBehaviour
{
    public enum Type { Fire = 3, QuitGame = 11, MapEditor = 12 }
    public Type type;
}
