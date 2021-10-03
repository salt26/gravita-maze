using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallInfo : IEquatable<WallInfo>
{
    public enum Type { Horizontal = 1, Vertical = 2, ExitHorizontal = 3, ExitVertical = 4 }

    public Type type;
    public int x;
    public int y;

    public WallInfo(Type type, int x, int y)
    {
        this.type = type;
        this.x = x;
        this.y = y;
    }

    public bool Equals(WallInfo other)
    {
        if (this.type == other.type && this.x == other.x && this.y == other.y) return true;
        else return false;
    }
}
