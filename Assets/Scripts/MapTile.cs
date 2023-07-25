using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapTile : MonoBehaviour
{
    // 나중에 벽 종류를 추가할 때 여기 RenderingWallFlag 및 PlayingWallFlag에 벽 종류를 추가하면 됩니다.
    // 추가할 때에는 맨 뒤에 번호가 1씩 증가하도록 추가하면 좋습니다.

    /// <summary>
    /// 그래픽으로 그릴 때 필요한 벽 정보
    /// </summary>
    public enum RenderingWallFlag { None = 0, Wall = 1, Shutter = 2, Glass = 3, OneWayIn = 4, OneWayOut = 5, ClosedShutter = 6, BrokenGlass = 7 }

    /// <summary>
    /// 게임플레이 메커니즘을 구현할 때 필요한 벽 정보
    /// </summary>
    public enum PlayingWallFlag { None = 0, Wall = 1, Shutter = 2, Glass = 3, OneWayIn = 4, OneWayOut = 5 }

    // 나중에 FixedObject 종류를 추가할 때 여기 FixedObjectFlag에 물체 종류를 추가하면 됩니다.
    // 추가할 때에는 맨 뒤에 번호가 1씩 증가하도록 추가하면 좋습니다.
    public enum FixedObjectFlag {
        None = 0, Fire = 1, Hole = 2, QuitGame = 3, MapEditor = 4,
        Adventure = 5, Tutorial = 6, Custom = 7, Training = 8, Setting = 9,
        AdvEasy = 10, AdvNormal = 11, AdvHard = 12, AdvInsane = 13
    }

    public int x;
    public int y;
    public RenderingWallFlag topWall;
    public RenderingWallFlag bottomWall;
    public RenderingWallFlag leftWall;
    public RenderingWallFlag rightWall;
    public FixedObjectFlag fixedObject;
    private bool dirtyBit = false;

    public SpriteRenderer topWallSpriteRenderer;
    public SpriteRenderer bottomWallSpriteRenderer;
    public SpriteRenderer leftWallSpriteRenderer;
    public SpriteRenderer rightWallSpriteRenderer;

    public Sprite wallSprite;
    public Sprite shutterSprite;
    public Sprite glassSprite;
    public Sprite oneWayInSprite;
    public Sprite oneWayOutSprite;
    public Sprite closedShutterSprite;
    public Sprite brokenGlassSprite;

    public void Initialize(int x, int y, RenderingWallFlag top, RenderingWallFlag bottom,
        RenderingWallFlag left, RenderingWallFlag right, FixedObjectFlag fixedObject)
    {
        if (x < 0 || y < 0)
        {
            Debug.LogWarning("Tile warining: invalid x or y position");
            return;
        }
        this.x = x;
        this.y = y;
        topWall = top;
        bottomWall = bottom;
        leftWall = left;
        rightWall = right;
        this.fixedObject = fixedObject;
        dirtyBit = true;
    }

    void Update()
    {
        if (dirtyBit)
        {
            dirtyBit = false;
            ChangeWallSprite(topWall, topWallSpriteRenderer);
            ChangeWallSprite(bottomWall, bottomWallSpriteRenderer);
            ChangeWallSprite(leftWall, leftWallSpriteRenderer);
            ChangeWallSprite(rightWall, rightWallSpriteRenderer);
        }
    }

    private void ChangeWallSprite(RenderingWallFlag rwf, SpriteRenderer sr)
    {
        switch (rwf)
        {
            case RenderingWallFlag.None: sr.sprite = null; break;
            case RenderingWallFlag.Wall: sr.sprite = wallSprite; break;
            case RenderingWallFlag.Shutter: sr.sprite = shutterSprite; break;
            case RenderingWallFlag.Glass: sr.sprite = glassSprite; break;
            case RenderingWallFlag.OneWayIn: sr.sprite = oneWayInSprite; break;
            case RenderingWallFlag.OneWayOut: sr.sprite = oneWayOutSprite; break;
            case RenderingWallFlag.ClosedShutter: sr.sprite = closedShutterSprite; break;
            case RenderingWallFlag.BrokenGlass: sr.sprite = brokenGlassSprite; break;
            default:
                Debug.LogWarning("Tile warning: invalid wall flag");
                break;
        }
    }

    public PlayingWallFlag GetTopPlayingWallFlag()
    {
        switch (topWall)
        {
            case RenderingWallFlag.None: return PlayingWallFlag.None;
            case RenderingWallFlag.Wall: return PlayingWallFlag.Wall;
            case RenderingWallFlag.Shutter: return PlayingWallFlag.Shutter;
            case RenderingWallFlag.Glass: return PlayingWallFlag.Glass;
            case RenderingWallFlag.OneWayIn: return PlayingWallFlag.OneWayIn;
            case RenderingWallFlag.OneWayOut: return PlayingWallFlag.OneWayOut;
            case RenderingWallFlag.ClosedShutter: return PlayingWallFlag.Wall;
            case RenderingWallFlag.BrokenGlass: return PlayingWallFlag.None;
            default:
                Debug.LogWarning("Tile warning: invalid wall flag");
                return PlayingWallFlag.None;
        }
    }

    public PlayingWallFlag GetBottomPlayingWallFlag()
    {
        switch (bottomWall)
        {
            case RenderingWallFlag.None: return PlayingWallFlag.None;
            case RenderingWallFlag.Wall: return PlayingWallFlag.Wall;
            case RenderingWallFlag.Shutter: return PlayingWallFlag.Shutter;
            case RenderingWallFlag.Glass: return PlayingWallFlag.Glass;
            case RenderingWallFlag.OneWayIn: return PlayingWallFlag.OneWayIn;
            case RenderingWallFlag.OneWayOut: return PlayingWallFlag.OneWayOut;
            case RenderingWallFlag.ClosedShutter: return PlayingWallFlag.Wall;
            case RenderingWallFlag.BrokenGlass: return PlayingWallFlag.None;
            default:
                Debug.LogWarning("Tile warning: invalid wall flag");
                return PlayingWallFlag.None;
        }
    }

    public PlayingWallFlag GetLeftPlayingWallFlag()
    {
        switch (leftWall)
        {
            case RenderingWallFlag.None: return PlayingWallFlag.None;
            case RenderingWallFlag.Wall: return PlayingWallFlag.Wall;
            case RenderingWallFlag.Shutter: return PlayingWallFlag.Shutter;
            case RenderingWallFlag.Glass: return PlayingWallFlag.Glass;
            case RenderingWallFlag.OneWayIn: return PlayingWallFlag.OneWayIn;
            case RenderingWallFlag.OneWayOut: return PlayingWallFlag.OneWayOut;
            case RenderingWallFlag.ClosedShutter: return PlayingWallFlag.Wall;
            case RenderingWallFlag.BrokenGlass: return PlayingWallFlag.None;
            default:
                Debug.LogWarning("Tile warning: invalid wall flag");
                return PlayingWallFlag.None;
        }
    }

    public PlayingWallFlag GetRightPlayingWallFlag()
    {
        switch (rightWall)
        {
            case RenderingWallFlag.None: return PlayingWallFlag.None;
            case RenderingWallFlag.Wall: return PlayingWallFlag.Wall;
            case RenderingWallFlag.Shutter: return PlayingWallFlag.Shutter;
            case RenderingWallFlag.Glass: return PlayingWallFlag.Glass;
            case RenderingWallFlag.OneWayIn: return PlayingWallFlag.OneWayIn;
            case RenderingWallFlag.OneWayOut: return PlayingWallFlag.OneWayOut;
            case RenderingWallFlag.ClosedShutter: return PlayingWallFlag.Wall;
            case RenderingWallFlag.BrokenGlass: return PlayingWallFlag.None;
            default:
                Debug.LogWarning("Tile warning: invalid wall flag");
                return PlayingWallFlag.None;
        }
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
        return (long)topWall * kinds * kinds * kinds + (long)bottomWall * kinds * kinds + (long)leftWall * kinds + (long)rightWall;
    }
}
