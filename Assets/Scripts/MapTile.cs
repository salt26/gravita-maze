using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapTile : MonoBehaviour
{
    // ���߿� �� ������ �߰��� �� ���� RenderingWallFlag �� PlayingWallFlag�� �� ������ �߰��ϸ� �˴ϴ�.
    // �߰��� ������ �� �ڿ� ��ȣ�� 1�� �����ϵ��� �߰��ϸ� �����ϴ�.

    /// <summary>
    /// �׷������� �׸� �� �ʿ��� �� ����
    /// </summary>
    public enum WallFlag { None = 0, Wall = 1, Shutter = 2, Glass = 3, OneWayIn = 4, OneWayOut = 5, Exit = 6, ClosedShutter = 7, BrokenGlass = 8 }

    /// <summary>
    /// �׷������� �׸� �� �ʿ��� �ڳ� �� ����
    /// </summary>
    public enum CornerWallFlag { None = 0, Normal = 1, Glitter = 2 }

    /// <summary>
    /// �׷������� �׸� �� �ʿ��� �ٴ� ����
    /// </summary>
    public enum FloorFlag { Hole = 0, Floor = 1 }

    /// <summary>
    /// �����÷��� ��Ŀ������ ������ �� �ʿ��� �� ����
    /// </summary>
    public enum PlayingWallFlag { None = 0, Wall = 1, Shutter = 2, Glass = 3, OneWayIn = 4, OneWayOut = 5 }

    /*
    // ���߿� FixedObject ������ �߰��� �� ���� FixedObjectFlag�� ��ü ������ �߰��ϸ� �˴ϴ�.
    // �߰��� ������ �� �ڿ� ��ȣ�� 1�� �����ϵ��� �߰��ϸ� �����ϴ�.
    public enum FixedObjectFlag {
        None = 0, Fire = 1, Hole = 2, QuitGame = 3, MapEditor = 4,
        Adventure = 5, Tutorial = 6, Custom = 7, Training = 8, Setting = 9,
        AdvEasy = 10, AdvNormal = 11, AdvHard = 12, AdvInsane = 13,
        TopArrow = 14, BottomArrow = 15, LeftArrow = 16, RightArrow = 17
    }
    */

    private int _x, _y;
    private WallFlag _topWall, _bottomWall, _leftWall, _rightWall;
    private CornerWallFlag _topLeftCorner, _topRightCorner, _bottomRightCorner, _bottomLeftCorner;
    private FloorFlag _floorFlag;
    //private FixedObjectFlag _fixedObject;
    public int X
    {
        get
        {
            return _x;
        }
        private set
        {
            _x = value;
        }
    }
    public int Y
    {
        get
        {
            return _y;
        }
        private set
        {
            _y = value;
        }
    }
    public WallFlag TopWall
    {
        get
        {
            return _topWall;
        }
        private set
        {
            _topWall = value;
            ChangeWallSprite(TopWall, topWallSpriteRenderer);
        }
    }
    public WallFlag BottomWall
    {
        get
        {
            return _bottomWall;
        }
        private set
        {
            _bottomWall = value;
            ChangeWallSprite(BottomWall, bottomWallSpriteRenderer);
        }
    }
    public WallFlag LeftWall
    {
        get
        {
            return _leftWall;
        }
        private set
        {
            _leftWall = value;
            ChangeWallSprite(LeftWall, leftWallSpriteRenderer);
        }
    }
    public WallFlag RightWall
    {
        get
        {
            return _rightWall;
        }
        private set
        {
            _rightWall = value;
            ChangeWallSprite(RightWall, rightWallSpriteRenderer);
        }
    }

    public CornerWallFlag TopLeftCorner
    {
        get
        {
            return _topLeftCorner;
        }
        private set
        {
            _topLeftCorner = value;
            ChangeCornerSprite(TopLeftCorner, topLeftCornerSpriteRenderer);
        }
    }

    public CornerWallFlag TopRightCorner
    {
        get
        {
            return _topRightCorner;
        }
        private set
        {
            _topRightCorner = value;
            ChangeCornerSprite(TopRightCorner, topRightCornerSpriteRenderer);
        }
    }

    public CornerWallFlag BottomRightCorner
    {
        get
        {
            return _bottomRightCorner;
        }
        private set
        {
            _bottomRightCorner = value;
            ChangeCornerSprite(BottomRightCorner, bottomRightCornerSpriteRenderer);
        }
    }

    public CornerWallFlag BottomLeftCorner
    {
        get
        {
            return _bottomLeftCorner;
        }
        private set
        {
            _bottomLeftCorner = value;
            ChangeCornerSprite(BottomLeftCorner, bottomLeftCornerSpriteRenderer);
        }
    }
    public FloorFlag Floor
    {
        get
        {
            return _floorFlag;
        }
        private set
        {
            _floorFlag = value;
            ChangeFloorSprite();
        }
    }
    /*
    public FixedObjectFlag FixedObject
    {
        get
        {
            return _fixedObject;
        }
        private set
        {
            _fixedObject = value;
            // ChangeFloorSprite(); TODO
        }
    }
    */
    public SpriteRenderer topWallSpriteRenderer;
    public SpriteRenderer bottomWallSpriteRenderer;
    public SpriteRenderer leftWallSpriteRenderer;
    public SpriteRenderer rightWallSpriteRenderer;
    public SpriteRenderer topLeftCornerSpriteRenderer;
    public SpriteRenderer topRightCornerSpriteRenderer;
    public SpriteRenderer bottomRightCornerSpriteRenderer;
    public SpriteRenderer bottomLeftCornerSpriteRenderer;

    public Sprite wallSprite;
    public Sprite shutterSprite;
    public Sprite glassSprite;
    public Sprite oneWayInSprite;
    public Sprite oneWayOutSprite;
    public Sprite exitSprite;
    public Sprite closedShutterSprite;
    public Sprite brokenGlassSprite;
    public Sprite floorSprite;
    public Sprite normalCornerSprite;
    public Sprite glitterCornerSprite;

    public void Initialize(int x, int y, FloorFlag floor, WallFlag top, WallFlag bottom, WallFlag left, WallFlag right,
        CornerWallFlag topLeft = CornerWallFlag.Normal, CornerWallFlag topRight = CornerWallFlag.Normal,
        CornerWallFlag bottomRight = CornerWallFlag.Normal, CornerWallFlag bottomLeft = CornerWallFlag.Normal)
    {
        if (x < 0 || y < 0)
        {
            Debug.LogWarning("Tile warining: invalid x or y position");
            return;
        }
        X = x;
        Y = y;
        Floor = floor;
        TopWall = top;
        BottomWall = bottom;
        LeftWall = left;
        RightWall = right;
        TopLeftCorner = topLeft;
        TopRightCorner = topRight;
        BottomRightCorner = bottomRight;
        BottomLeftCorner = bottomLeft;
        //this.FixedObject = fixedObject;
        ChangeFloorSprite();
        ChangeWallSprite(TopWall, topWallSpriteRenderer);
        ChangeWallSprite(BottomWall, bottomWallSpriteRenderer);
        ChangeWallSprite(LeftWall, leftWallSpriteRenderer);
        ChangeWallSprite(RightWall, rightWallSpriteRenderer);
        ChangeCornerSprite(TopLeftCorner, topLeftCornerSpriteRenderer);
        ChangeCornerSprite(TopRightCorner, topRightCornerSpriteRenderer);
        ChangeCornerSprite(BottomRightCorner, bottomRightCornerSpriteRenderer);
        ChangeCornerSprite(BottomLeftCorner, bottomLeftCornerSpriteRenderer);
    }

    private void ChangeWallSprite(WallFlag rwf, SpriteRenderer sr)
    {
        switch (rwf)
        {
            case WallFlag.None: sr.sprite = null; break;
            case WallFlag.Wall: sr.sprite = wallSprite; break;
            case WallFlag.Exit: sr.sprite = exitSprite; break;
            case WallFlag.Shutter: sr.sprite = shutterSprite; break;
            case WallFlag.Glass: sr.sprite = glassSprite; break;
            case WallFlag.OneWayIn: sr.sprite = oneWayInSprite; break;
            case WallFlag.OneWayOut: sr.sprite = oneWayOutSprite; break;
            case WallFlag.ClosedShutter: sr.sprite = closedShutterSprite; break;
            case WallFlag.BrokenGlass: sr.sprite = brokenGlassSprite; break;
            default:
                Debug.LogWarning("Tile warning: invalid wall flag");
                break;
        }
    }

    private void ChangeFloorSprite()
    {
        switch (Floor)
        {
            case FloorFlag.Hole:
                GetComponent<SpriteRenderer>().sprite = null;
                break;
            case FloorFlag.Floor:
                GetComponent<SpriteRenderer>().sprite = floorSprite;
                break;
            default:
                GetComponent<SpriteRenderer>().sprite = floorSprite;
                break;
        }
    }

    private void ChangeCornerSprite(CornerWallFlag cwf, SpriteRenderer sr)
    {

        switch (cwf)
        {
            case CornerWallFlag.None: sr.sprite = null; break;
            case CornerWallFlag.Normal: sr.sprite = normalCornerSprite; break;
            case CornerWallFlag.Glitter: sr.sprite = glitterCornerSprite; break;
            default:
                Debug.LogWarning("Tile warning: invalid corner flag");
                break;
        }
    }

    public PlayingWallFlag GetTopPlayingWallFlag()
    {
        switch (TopWall)
        {
            case WallFlag.None: return PlayingWallFlag.None;
            case WallFlag.Wall: return PlayingWallFlag.Wall;
            case WallFlag.Exit: return PlayingWallFlag.None;
            case WallFlag.Shutter: return PlayingWallFlag.Shutter;
            case WallFlag.Glass: return PlayingWallFlag.Glass;
            case WallFlag.OneWayIn: return PlayingWallFlag.OneWayIn;
            case WallFlag.OneWayOut: return PlayingWallFlag.OneWayOut;
            case WallFlag.ClosedShutter: return PlayingWallFlag.Wall;
            case WallFlag.BrokenGlass: return PlayingWallFlag.None;
            default:
                Debug.LogWarning("Tile warning: invalid wall flag");
                return PlayingWallFlag.None;
        }
    }

    public PlayingWallFlag GetBottomPlayingWallFlag()
    {
        switch (BottomWall)
        {
            case WallFlag.None: return PlayingWallFlag.None;
            case WallFlag.Wall: return PlayingWallFlag.Wall;
            case WallFlag.Exit: return PlayingWallFlag.None;
            case WallFlag.Shutter: return PlayingWallFlag.Shutter;
            case WallFlag.Glass: return PlayingWallFlag.Glass;
            case WallFlag.OneWayIn: return PlayingWallFlag.OneWayIn;
            case WallFlag.OneWayOut: return PlayingWallFlag.OneWayOut;
            case WallFlag.ClosedShutter: return PlayingWallFlag.Wall;
            case WallFlag.BrokenGlass: return PlayingWallFlag.None;
            default:
                Debug.LogWarning("Tile warning: invalid wall flag");
                return PlayingWallFlag.None;
        }
    }

    public PlayingWallFlag GetLeftPlayingWallFlag()
    {
        switch (LeftWall)
        {
            case WallFlag.None: return PlayingWallFlag.None;
            case WallFlag.Wall: return PlayingWallFlag.Wall;
            case WallFlag.Exit: return PlayingWallFlag.None;
            case WallFlag.Shutter: return PlayingWallFlag.Shutter;
            case WallFlag.Glass: return PlayingWallFlag.Glass;
            case WallFlag.OneWayIn: return PlayingWallFlag.OneWayIn;
            case WallFlag.OneWayOut: return PlayingWallFlag.OneWayOut;
            case WallFlag.ClosedShutter: return PlayingWallFlag.Wall;
            case WallFlag.BrokenGlass: return PlayingWallFlag.None;
            default:
                Debug.LogWarning("Tile warning: invalid wall flag");
                return PlayingWallFlag.None;
        }
    }

    public PlayingWallFlag GetRightPlayingWallFlag()
    {
        switch (RightWall)
        {
            case WallFlag.None: return PlayingWallFlag.None;
            case WallFlag.Wall: return PlayingWallFlag.Wall;
            case WallFlag.Exit: return PlayingWallFlag.None;
            case WallFlag.Shutter: return PlayingWallFlag.Shutter;
            case WallFlag.Glass: return PlayingWallFlag.Glass;
            case WallFlag.OneWayIn: return PlayingWallFlag.OneWayIn;
            case WallFlag.OneWayOut: return PlayingWallFlag.OneWayOut;
            case WallFlag.ClosedShutter: return PlayingWallFlag.Wall;
            case WallFlag.BrokenGlass: return PlayingWallFlag.None;
            default:
                Debug.LogWarning("Tile warning: invalid wall flag");
                return PlayingWallFlag.None;
        }
    }

    /// <summary>
    /// �����÷��� ��Ŀ������ ������ �� ���� Ÿ���� ���� �����¿� �� ������ �ϳ��� ���� ǥ���մϴ�.
    /// �� (PlayingWallFlag�� ���� ����) ^ 4 ������ ���¸� �����ϴ�.
    /// </summary>
    /// <returns>(PlayingWallFlag�� ���� ����)�������� ǥ���� �� �ڸ� �� (most significant bit���� ���ʷ� ��, ��, ��, ��)</returns>
    public long GetPlayingWallFlagCode()
    {
        long kinds = Enum.GetNames(typeof(PlayingWallFlag)).Count();
        Debug.Log(kinds);
        return (long)TopWall * kinds * kinds * kinds + (long)BottomWall * kinds * kinds + (long)LeftWall * kinds + (long)RightWall;
    }
}
