using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;
using static MapManager;
using static MapTile;

public class MapTile : MonoBehaviour
{
    // 나중에 벽 종류를 추가할 때 여기 RenderingWallFlag 및 PlayingWallFlag에 벽 종류를 추가하면 됩니다.
    // 추가할 때에는 맨 뒤에 번호가 1씩 증가하도록 추가하면 좋습니다.

    public enum DirectionFlag { Top = 3, Up = 3, Bottom = 2, Down = 2, Left = 1, Right = 0 };

    /// <summary>
    /// 그래픽으로 그릴 때 필요한 벽 정보
    /// </summary>
    public enum WallFlag { None = 0, Wall = 1, Shutter = 2, Glass = 3, OneWayIn = 4, OneWayOut = 5, Exit = 6, ClosedShutter = 7, BrokenGlass = 8 }

    /// <summary>
    /// 그래픽으로 그릴 때 필요한 코너 벽 정보
    /// </summary>
    public enum CornerWallFlag { None = 0, Normal = 1, Glitter = 2 }

    /// <summary>
    /// 그래픽으로 그릴 때 필요한 바닥 정보
    /// </summary>
    public enum FloorFlag { Hole = 0, Floor = 1 }

    /// <summary>
    /// 게임플레이 메커니즘을 구현할 때 필요한 벽 정보
    /// </summary>
    public enum PlayingWallFlag { None = 0, Wall = 1, Shutter = 2, Glass = 3, OneWayIn = 4, OneWayOut = 5 }

    // 나중에 FixedObject 종류를 추가할 때 여기 FixedObjectFlag에 물체 종류를 추가하면 됩니다.
    // 추가할 때에는 맨 뒤에 번호가 1씩 증가하도록 추가하면 좋습니다.
    public enum FixedObjectFlag {
        None = 0, Fire = 1, Hole = 2, QuitGame = 3, MapEditor = 4,
        Adventure = 5, Tutorial = 6, Custom = 7, Training = 8, Setting = 9,
        AdvEasy = 10, AdvNormal = 11, AdvHard = 12, AdvInsane = 13,
        TopArrow = 14, BottomArrow = 15, LeftArrow = 16, RightArrow = 17
    }

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

    public void Initialize(int x, int y, FloorFlag floor, long wallCode)
    {
        X = x;
        Y = y;
        Floor = floor;

        long kinds = GetRenderingWallKinds();
        TopWall = (WallFlag)(wallCode / kinds / kinds / kinds);
        BottomWall = (WallFlag)(wallCode / kinds / kinds % kinds);
        LeftWall = (WallFlag)(wallCode / kinds % kinds);
        RightWall = (WallFlag)(wallCode % kinds);
        TopLeftCorner = CornerWallFlag.Normal;
        TopRightCorner = CornerWallFlag.Normal;
        BottomRightCorner = CornerWallFlag.Normal;
        BottomLeftCorner = CornerWallFlag.Normal;
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

    /*
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
    */

    public static PlayingWallFlag WallFlagToPlayingWallFlag(WallFlag wallFlag)
    {
        switch (wallFlag)
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

    public static long WallFlagToTileCode(WallFlag wallFlag, DirectionFlag directionFlag)
    {
        long kinds = GetRenderingWallKinds();
        long factor = 1;
        for (int i = 0; i < (int)directionFlag; i++)
        {
            factor *= kinds;
        }
        return factor * (long)wallFlag;
    }

    public static long FixedObjectFlagToTileCode(FixedObjectFlag fixedObjectFlag)
    {
        long kinds4 = GetKinds4();
        long factor = 1;
        for (int i = 0; i < (int)fixedObjectFlag; i++)
        {
            factor *= kinds4;
        }
        return factor;
    }

    /// <summary>
    /// 게임플레이 메커니즘을 구현할 때 현재 타일이 가진 상하좌우 벽 정보를 하나의 수로 표현합니다.
    /// 총 (PlayingWallFlag의 종류 개수) ^ 4 가지의 상태를 가집니다.
    /// </summary>
    /// <returns>(PlayingWallFlag의 종류 개수)진법으로 표현한 네 자리 수 (most significant bit부터 차례로 상, 하, 좌, 우)</returns>
    public long GetPlayingWallFlagCode()
    {
        long kinds = Enum.GetNames(typeof(PlayingWallFlag)).Count();
        Debug.Log(kinds);
        return (long)TopWall * kinds * kinds * kinds + (long)BottomWall * kinds * kinds + (long)LeftWall * kinds + (long)RightWall;
    }

    public static long GetRenderingWallKinds()
    {
        return Enum.GetNames(typeof(WallFlag)).Count();
    }

    public static long GetKinds4()
    {
        long kinds = GetRenderingWallKinds();
        return kinds * kinds * kinds * kinds;
    }

    // tileCode는 WallFlag 기준
    public static bool CheckTileFlag(long tileCode, PlayingWallFlag playingWallFlag, DirectionFlag directionFlag)
    {
        long kinds = GetRenderingWallKinds();
        long kinds4 = GetKinds4();
        long factor = 1;
        for (int i = 0; i < (int)directionFlag; i++)
        {
            factor *= kinds;
        }
        return WallFlagToPlayingWallFlag((WallFlag)(tileCode % kinds4 / factor % kinds)) == playingWallFlag;
    }

    public static bool CheckTileFlag(long tileCode, FixedObjectFlag fixedObjectFlag)
    {
        long kinds4 = GetKinds4();
        long factor = 1;
        for (int i = 0; i < (int)fixedObjectFlag; i++)
        {
            factor *= kinds4;
        }
        return tileCode % (factor * kinds4) / factor == 1;
    }
}
