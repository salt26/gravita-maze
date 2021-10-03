using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectInfo : IEquatable<ObjectInfo>
{
    public enum Type { Ball = 1, Iron = 2, Fire = 3, QuitGame = 11, MapEditor = 12 }
    
    public Type type;
    public int x;
    public int y;
    public ObjectInfo(Type type, int x, int y)
    {
        this.type = type;
        this.x = x;
        this.y = y;
    }

    public bool Equals(ObjectInfo other)
    {
        if (this.type == other.type && this.x == other.x && this.y == other.y) return true;
        else return false;
    }
}
