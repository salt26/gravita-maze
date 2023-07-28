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
    public enum RenderingWallFlag { None = 0, Wall = 1, Shutter = 2, Glass = 3, OneWayIn = 4, OneWayOut = 5, ClosedShutter = 6, BrokenGlass = 7 }

    /// <summary>
    /// �����÷��� ��Ŀ������ ������ �� �ʿ��� �� ����
    /// </summary>
    public enum PlayingWallFlag { None = 0, Wall = 1, Shutter = 2, Glass = 3, OneWayIn = 4, OneWayOut = 5 }

    // ���߿� FixedObject ������ �߰��� �� ���� FixedObjectFlag�� ��ü ������ �߰��ϸ� �˴ϴ�.
    // �߰��� ������ �� �ڿ� ��ȣ�� 1�� �����ϵ��� �߰��ϸ� �����ϴ�.
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
    /// �����÷��� ��Ŀ������ ������ �� ���� Ÿ���� ���� �����¿� �� ������ �ϳ��� ���� ǥ���մϴ�.
    /// �� (PlayingWallFlag�� ���� ����) ^ 4 ������ ���¸� �����ϴ�.
    /// </summary>
    /// <returns>(PlayingWallFlag�� ���� ����)�������� ǥ���� �� �ڸ� �� (most significant bit���� ���ʷ� ��, ��, ��, ��)</returns>
    public long GetPlayingWallFlagCode()
    {
        long kinds = Enum.GetNames(typeof(PlayingWallFlag)).Count();
        Debug.Log(kinds);
        return (long)topWall * kinds * kinds * kinds + (long)bottomWall * kinds * kinds + (long)leftWall * kinds + (long)rightWall;
    }
}
