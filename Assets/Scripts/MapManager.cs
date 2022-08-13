using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour
{
    public enum Flag { Continued = 0, Escaped = 1, Burned = 2, Squashed = 3, TimeOver = 4, QuitGame = 5, MapEditor = 6,
        Adventure = 7, Tutorial = 8, Custom = 9, Survival = 10, AdvEasy = 11, AdvNormal = 12, AdvHard = 13, AdvInsane = 14 }
    public enum TileFlag { RightWall = 1, RightShutter = 2, LeftWall = 3, LeftShutter = 6, DownWall = 9, DownShutter = 18, UpWall = 27, UpShutter = 54,
        Fire = 81, QuitGame = 243, MapEditor = 729, Adventure = 2187, Tutorial = 6561, Custom = 19683, Survival = 59049, AdvEasy = 177147, 
        AdvNormal = 531441, AdvHard = 1594323, AdvInsane = 4782969 }
    // 기존의 방식: 2진법 이용, 따라서 켜고 끄는 것들만 있음..
    // 근데 3진법을 쓴다면 Shutter를 구현할 수 있다!!
    public enum OpenFileFlag { Failed = 0, Success = 1, Restore = 2 }
    public enum RotationStatus { Original = 0, Clockwise90 = 1, Clockwise180 = 2, Clockwise270 = 3,
        UpsideDown = 4, UpsideDown90 = 5, UpsideDown180 = 6, UpsideDown270 = 7 }

    public const int MIN_SIZE_X = 2;
    public const int MIN_SIZE_Y = 2;
    public const int MAX_SIZE_X = 9;
    public const int MAX_SIZE_Y = 9;

#if UNITY_ANDROID && !UNITY_EDITOR
    //public const string ROOT_PATH = "/storage/emulated/0/GravitaMaze/";
    public static string ROOT_PATH = Application.persistentDataPath.Substring(0, Application.persistentDataPath.IndexOf("Android", StringComparison.Ordinal)) + "/GravitaMaze/";
    public static string MAP_ROOT_PATH = ROOT_PATH + "Maps/";
#else
    public const string MAP_ROOT_PATH = "Maps/";
#endif
    public const float DEFAULT_TIME_LIMIT = 30f;
    public const float MIN_TIME_LIMIT = 3f;
    public const float MAX_TIME_LIMIT = 30f;

    [HideInInspector]
    public List<Movable> movables;
    [HideInInspector]
    public List<FixedObject> fixedObjects;
    [HideInInspector]
    public List<GameObject> traces;

    public Map map;
    public Movable[,] initialMovableCoord;
    public Movable[,] currentMovableCoord;
    public int[,] initialMapCoord;
    public int[,] currentMapCoord;

    public GameObject movableAndFixedGameObjects;
    public Camera mainCamera;

    public Button gravityRetryButton;
    public Button gravityUpButton;
    public Button gravityDownButton;
    public Button gravityLeftButton;
    public Button gravityRightButton;
    public RectTransform gravityBall;

    public GameObject ballPrefab;
    public GameObject ironPrefab;
    public GameObject firePrefab;

    public List<GameObject> ballTracePrefabs = new List<GameObject>();
    public List<GameObject> ironTracePrefabs = new List<GameObject>();

    public GameObject flagBurnedPrefab;
    public GameObject flagSquashedPrefab;

    public Tilemap tilemap;
    public List<Tile> tiles = new List<Tile>();

    public GameObject loadingPanel;
    public GameObject timeoutPanel;

    public delegate void AfterGravity(Flag flag);
    public AfterGravity afterGravity;

    private int _originalSizeX = 0;
    private int _originalSizeY = 0;
    private float _timeLimit;
    private bool _isReady = false;

    public int SizeX
    {
        get
        {
            return RotatedSizeX();
        }
        private set
        {
            _originalSizeX = value;
        }
    }
    public int SizeY
    {
        get
        {
            return RotatedSizeY();
        }
        private set
        {
            _originalSizeY = value;
        }
    }
    public int ExitX
    {
        get;
        private set;
    }
    public int ExitY
    {
        get;
        private set;
    }
    public float TimeLimit
    {
        get
        {
            return _timeLimit;
        }
        set
        {
            _timeLimit = Mathf.Max(3f, value);
        }
    }
    public float RemainingTime
    {
        get;
        private set;
    } = 0f;
    public bool IsTimePassing
    {
        get;
        private set;
    } = false;
    public bool HasTimePaused
    {
        get;
        private set;
    } = false;
    public bool IsTimeActivated
    {
        get;
        private set;
    } = false;

    public bool IsReady
    {
        get
        {
            return _isReady;
        }
        private set
        {
            _isReady = value;
            loadingPanel.SetActive(!value);
        }
    }

    public bool HasCleared
    {
        get;
        private set;
    } = false;

    public bool HasDied
    {
        get;
        private set;
    } = false;

    public string ActionHistory
    {
        get;
        private set;
    } = "";

    public RotationStatus Rotation
    {
        get;
        private set;
    } = RotationStatus.Original;

    void Update()
    {
        gravityRetryButton.interactable = IsReady && ActionHistory != "";
        if (IsReady && IsTimeActivated && IsTimePassing && !HasTimePaused && RemainingTime > 0f && !HasCleared)
        {
            RemainingTime -= Time.deltaTime;
            if (RemainingTime <= 0f)
            {
                timeoutPanel.SetActive(true);
                if (afterGravity.GetInvocationList().Length > 0)
                    afterGravity(Flag.TimeOver); // 사망판정을 해 주는 함수
                Debug.LogWarning("Map warning: Time over");
            }
        }
    }

    public void Initialize()
    {
        IsReady = false;

        SizeX = 0;
        SizeY = 0;
        ExitX = 0;
        ExitY = 0;
        _timeLimit = 0f;
        initialMovableCoord = null;
        map = null;
        movables = new List<Movable>();
        fixedObjects = new List<FixedObject>();
        traces = new List<GameObject>();
        ActionHistory = "";
        Rotation = RotationStatus.Original;
        foreach (Movable m in movableAndFixedGameObjects.GetComponentsInChildren<Movable>())
        {
            Destroy(m.gameObject);
        }
        foreach (FixedObject f in movableAndFixedGameObjects.GetComponentsInChildren<FixedObject>())
        {
            Destroy(f.gameObject);
        }
        HasCleared = false;
        HasDied = false;
        IsTimeActivated = false;
        IsTimePassing = false;
        HasTimePaused = false;
        RemainingTime = 0f;
        tilemap.ClearAllTiles();
        timeoutPanel.SetActive(false);
    }

    public void Initialize(int sizeX, int sizeY, List<WallInfo> walls, List<ObjectInfo> objects, string solution = "",
        float timeLimit = 0f, bool isValidation = false, bool canRotate = false)
    {
        IsReady = false;

        // movableGameObject와 fixedGameObject의 child로 등록된 Movable, FixedObject들은 ObjectInfo를 인자로 주지 않아도 자동으로 등록됨
        // 인자에서 ObjectInfo가 주어진 objects는 씬에 미리 배치된 오브젝트가 아니므로 여기에서 자동으로 생성됨
        // "Mode" 씬에서는 특별히 10 이상의 sizeX를 허용
        if (sizeX < 2 || (!SceneManager.GetActiveScene().name.Equals("Mode") && sizeX > MAX_SIZE_X) || sizeY < 2 || sizeY > MAX_SIZE_Y)
        {
            Debug.LogError("Map invalid: size");
            return;
        }

        // Caution: _originalSizeX == sizeX != SizeX, _originalSizeY == sizeY != SizeY
        this.SizeX = sizeX;
        this.SizeY = sizeY;
        ExitX = 0;
        ExitY = 0;

        if (canRotate && !(SceneManager.GetActiveScene().name.Equals("Main") ||
            SceneManager.GetActiveScene().name.Equals("Editor") || SceneManager.GetActiveScene().name.Equals("Mode") ||
            SceneManager.GetActiveScene().name.Equals("AdventureLevel") || SceneManager.GetActiveScene().name.Equals("Tutorial")))
        {
            int r = UnityEngine.Random.Range(0, 8);
            Rotation = (RotationStatus)r;
            Debug.Log(Rotation);
        }
        else
        {
            Rotation = RotationStatus.Original;
        }

        initialMapCoord = new int[SizeX, SizeY]; // MapCoord는 대체 어디에다가 쓰는 거지??
        initialMovableCoord = new Movable[SizeX, SizeY]; // Movable의 좌표가 될 예정. 2차원 배열의 측정 위치에다가 Movable의 오브젝트를 넣는 다는 소리인가?

        tilemap.ClearAllTiles();
        timeoutPanel.SetActive(false);

        int[,] horizontalWalls = new int[SizeX, SizeY + 1];
        int[,] verticalWalls = new int[SizeX + 1, SizeY];
        // 벽이 없는 경우: 0, 평범한 벽이 있는 경우: 1, Shutter가 있는 경우: 2

        for (int i = 0; i < sizeX; i++)
        {
            if (!RotatedHasTransposed()) // original, 즉 가로 세로 길이는 유지된 상태.
            {
                horizontalWalls[RotatedX(i, 0, true, true), RotatedY(i, 0, true, true)] = 1; // 맨 위, 맨 아래 외벽은 일반 벽으로.
                horizontalWalls[RotatedX(i, sizeY, true, true), RotatedY(i, sizeY, true, true)] = 1;
                for (int j = 1; j < sizeY; j++)
                {
                    horizontalWalls[RotatedX(i, j, true, true), RotatedY(i, j, true, true)] = 0; // 맨 위, 맨 아래가 아닌 곳은 일단 벽이 없음.
                }
            }
            else
            {
                verticalWalls[RotatedX(i, 0, true, true), RotatedY(i, 0, true, true)] = 1; // 가로세로가 바뀌는 회전이 있을 때: 맨 왼쪽, 맨 오른쪽 외벽은 일반 벽
                verticalWalls[RotatedX(i, sizeY, true, true), RotatedY(i, sizeY, true, true)] = 1;
                for (int j = 1; j < sizeY; j++)
                {
                    verticalWalls[RotatedX(i, j, true, true), RotatedY(i, j, true, true)] = 0; // 맨 왼쪽, 맨 오른쪽이 아닌 곳은 일단 벽이 없음.
                }
            }
        }
        for (int j = 0; j < sizeY; j++)
        {
            if (!RotatedHasTransposed())
            {
                verticalWalls[RotatedX(0, j, true, false), RotatedY(0, j, true, false)] = 1;
                verticalWalls[RotatedX(sizeX, j, true, false), RotatedY(sizeX, j, true, false)] = 1;
                for (int i = 1; i < sizeX; i++)
                {
                    verticalWalls[RotatedX(i, j, true, false), RotatedY(i, j, true, false)] = 0;
                }
            }
            else
            {
                horizontalWalls[RotatedX(0, j, true, false), RotatedY(0, j, true, false)] = 1;
                horizontalWalls[RotatedX(sizeX, j, true, false), RotatedY(sizeX, j, true, false)] = 1;
                for (int i = 1; i < sizeX; i++)
                {
                    horizontalWalls[RotatedX(i, j, true, false), RotatedY(i, j, true, false)] = 0;
                }
            }
        } // 여기까지는 외벽을 일반 벽으로, 안쪽에 있는 벽은 일단 없도록 하는 코드임.

        bool hasExit = false;
    
        foreach (WallInfo wi in walls)
        {
            switch (wi.type)
            {
                case WallInfo.Type.ExitHorizontal:
                    if (hasExit)
                    {
                        Debug.LogError("Map invalid: too many exits");
                        return;
                    }
                    if (wi.x < 1 || wi.x > sizeX || !(wi.y == 0 || wi.y == sizeY))
                    {
                        Debug.LogError("Map invalid: exit position at (" + wi.x + ", " + wi.y + ")");
                        return;
                    }

                    if (!RotatedHasTransposed())
                    {
                        horizontalWalls[RotatedX(wi.x - 1, wi.y, true, true), RotatedY(wi.x - 1, wi.y, true, true)] = 0;
                        ExitX = RotatedX(wi.x, wi.y, true, true, true);
                        if (RotatedY(wi.x, wi.y, true, true, true) == 0) ExitY = 0;
                        else ExitY = SizeY + 1;
                    }
                    else
                    {
                        verticalWalls[RotatedX(wi.x - 1, wi.y, true, true), RotatedY(wi.x - 1, wi.y, true, true)] = 0;
                        if (RotatedX(wi.x, wi.y, true, true, true) == 0) ExitX = 0;
                        else ExitX = SizeX + 1;
                        ExitY = RotatedY(wi.x, wi.y, true, true, true);
                    }
                    hasExit = true;
                    break;
                case WallInfo.Type.ExitVertical:
                    if (hasExit)
                    {
                        Debug.LogError("Map invalid: too many exits");
                        return;
                    }
                    if (!(wi.x == 0 || wi.x == sizeX) || wi.y < 1 || wi.y > sizeY)
                    {
                        Debug.LogError("Map invalid: exit position at (" + wi.x + ", " + wi.y + ")");
                        return;
                    }

                    if (!RotatedHasTransposed())
                    {
                        verticalWalls[RotatedX(wi.x, wi.y - 1, true, false), RotatedY(wi.x, wi.y - 1, true, false)] = 0;
                        if (RotatedX(wi.x, wi.y, true, false, true) == 0) ExitX = 0;
                        else ExitX = SizeX + 1;
                        ExitY = RotatedY(wi.x, wi.y, true, false, true);
                    }
                    else
                    {
                        horizontalWalls[RotatedX(wi.x, wi.y - 1, true, false), RotatedY(wi.x, wi.y - 1, true, false)] = 0;
                        ExitX = RotatedX(wi.x, wi.y, true, false, true);
                        if (RotatedY(wi.x, wi.y, true, false, true) == 0) ExitY = 0;
                        else ExitY = SizeY + 1;
                    }
                    hasExit = true;
                    break;
                    // 여기 위까지는 Exit을 다룰 때 벽을 어떻게 할 지에 관한 코드니까 Shutter와는 관계 없는 듯.
                    /* 여기부터는 평범한 벽. 여기에는 Shutter가 들어갈 수 있다.
                     */
                case WallInfo.Type.Horizontal: 
                    if (wi.x < 1 || wi.x > sizeX || wi.y < 1 || wi.y > sizeY - 1)
                    {
                        Debug.LogError("Map invalid: wall position at (" + wi.x + ", " + wi.y + ")");
                        return;
                    }
                    if (!RotatedHasTransposed())
                    {
                        if (horizontalWalls[RotatedX(wi.x - 1, wi.y, true, true), RotatedY(wi.x - 1, wi.y, true, true)] != 0)
                        {
                            Debug.LogError("Map invalid: wall overlapped at the same position (" + wi.x + ", " + wi.y + ")");
                            return;
                        }
                        else
                        { 
                            horizontalWalls[RotatedX(wi.x - 1, wi.y, true, true), RotatedY(wi.x - 1, wi.y, true, true)] = 1; 
                        }
                    }
                    else
                    {
                        if (verticalWalls[RotatedX(wi.x - 1, wi.y, true, true), RotatedY(wi.x - 1, wi.y, true, true)] != 0)
                        {
                            Debug.LogError("Map invalid: wall overlapped at the same position (" + wi.x + ", " + wi.y + ")");
                            return;
                        }
                        else
                        {
                            verticalWalls[RotatedX(wi.x - 1, wi.y, true, true), RotatedY(wi.x - 1, wi.y, true, true)] = 1;
                        }
                    }
                    break;
                case WallInfo.Type.Vertical:
                    if (wi.x < 1 || wi.x > sizeX - 1 || wi.y < 1 || wi.y > sizeY)
                    {
                        Debug.LogError("Map invalid: wall position at (" + wi.x + ", " + wi.y + ")");
                        return;
                    }

                    if (!RotatedHasTransposed())
                    {
                        if (verticalWalls[RotatedX(wi.x, wi.y - 1, true, false), RotatedY(wi.x, wi.y - 1, true, false)] != 0)
                        {
                            Debug.LogError("Map invalid: wall overlapped at the same position (" + wi.x + ", " + wi.y + ")");
                        }
                        else 
                        {
                            verticalWalls[RotatedX(wi.x, wi.y - 1, true, false), RotatedY(wi.x, wi.y - 1, true, false)] = 1;
                        }
                    }
                    else
                    {
                        if (horizontalWalls[RotatedX(wi.x, wi.y - 1, true, false), RotatedY(wi.x, wi.y - 1, true, false)] != 0)
                        {
                            Debug.LogError("Map invalid: wall overlapped at the same position (" + wi.x + ", " + wi.y + ")");
                        }
                        else
                        {
                            horizontalWalls[RotatedX(wi.x, wi.y - 1, true, false), RotatedY(wi.x, wi.y - 1, true, false)] = 1;
                        }
                    }
                    break;
                case WallInfo.Type.HorizontalShutter:
                    if (wi.x < 1 || wi.x > sizeX || wi.y < 1 || wi.y > sizeY - 1)
                    {
                        Debug.LogError("Map invalid: shutter position at (" + wi.x + ", " + wi.y + ")");
                        return;
                    }
                    if (!RotatedHasTransposed())
                    {
                        if (horizontalWalls[RotatedX(wi.x - 1, wi.y, true, true), RotatedY(wi.x - 1, wi.y, true, true)] != 0)
                        {
                            Debug.LogError("Map invalid: shutter overlapped at the same position (" + wi.x + ", " + wi.y + ")");
                        }
                        else
                        { 
                            horizontalWalls[RotatedX(wi.x - 1, wi.y, true, true), RotatedY(wi.x - 1, wi.y, true, true)] = 2; 
                        }
                    }
                    else
                    {
                        if (verticalWalls[RotatedX(wi.x - 1, wi.y, true, true), RotatedY(wi.x - 1, wi.y, true, true)] != 0)
                        {
                            Debug.LogError("Map invalid: shutter overlapped at the same position (" + wi.x + ", " + wi.y + ")");
                        }
                        else
                        {
                            verticalWalls[RotatedX(wi.x - 1, wi.y, true, true), RotatedY(wi.x - 1, wi.y, true, true)] = 2;
                        }
                    }
                    break;
                case WallInfo.Type.VerticalShutter:
                    if (wi.x < 1 || wi.x > sizeX - 1 || wi.y < 1 || wi.y > sizeY)
                    {
                        Debug.LogError("Map invalid: shutter position at (" + wi.x + ", " + wi.y + ")");
                        return;
                    }

                    if (!RotatedHasTransposed())
                    {
                        if (verticalWalls[RotatedX(wi.x, wi.y - 1, true, false), RotatedY(wi.x, wi.y - 1, true, false)] != 0)
                        {
                            Debug.LogError("Map invalid: shutter overlapped at the same position (" + wi.x + ", " + wi.y + ")");
                        }
                        else
                        {
                            verticalWalls[RotatedX(wi.x, wi.y - 1, true, false), RotatedY(wi.x, wi.y - 1, true, false)] = 2;
                        }
                    }
                    else
                    {
                        if (horizontalWalls[RotatedX(wi.x, wi.y - 1, true, false), RotatedY(wi.x, wi.y - 1, true, false)] != 0)
                        {
                            Debug.LogError("Map invalid: shutter overlapped at the same position (" + wi.x + ", " + wi.y + ")");
                        }
                        else
                        {
                            horizontalWalls[RotatedX(wi.x, wi.y - 1, true, false), RotatedY(wi.x, wi.y - 1, true, false)] = 2;
                        }
                    }
                    break;
            }
        }

        if (!hasExit && (SceneManager.GetActiveScene().name != "Editor" || isValidation))
        {
            Debug.LogError("Map invalid: no exit");
            return;
        }

        for (int i = 0; i <= SizeX + 1; i++)
        {
            for (int j = 0; j <= SizeY + 1; j++)
            {
                if (i == 0)
                {
                    if (ExitX == i && ExitY == j)
                        tilemap.SetTile(new Vector3Int(i - 1, j - 1, 0), tiles[89]);
                    else tilemap.SetTile(new Vector3Int(i - 1, j - 1, 0), tiles[81]);
                }
                else if (i == SizeX + 1)
                {
                    if (ExitX == i && ExitY == j)
                        tilemap.SetTile(new Vector3Int(i - 1, j - 1, 0), tiles[90]);
                    else tilemap.SetTile(new Vector3Int(i - 1, j - 1, 0), tiles[82]);
                }
                else if (j == SizeY + 1)
                {
                    if (ExitX == i && ExitY == j)
                        tilemap.SetTile(new Vector3Int(i - 1, j - 1, 0), tiles[91]);
                    else tilemap.SetTile(new Vector3Int(i - 1, j - 1, 0), tiles[83]);
                }
                else if (j == 0)
                {
                    if (ExitX == i && ExitY == j)
                        tilemap.SetTile(new Vector3Int(i - 1, j - 1, 0), tiles[92]);
                    else tilemap.SetTile(new Vector3Int(i - 1, j - 1, 0), tiles[84]);
                }
                // 여기 위에 있는 if문은 겉테두리 벽을 두르는 부분. 여기에는 Shutter가 존재하지 않는다.
                // 여기 아래부터는 평범한 벽을 두르는 부분. 상황에 따라 Shutter가 존재할 수 있다.
                else
                {
                    if (horizontalWalls[i - 1, j] == 1)
                        initialMapCoord[i - 1, j - 1] += (int)TileFlag.UpWall;    // 27
                    else if (horizontalWalls[i - 1, j] == 2)
                        initialMapCoord[i - 1, j - 1] += (int)TileFlag.UpShutter; // 54
                    if (horizontalWalls[i - 1, j - 1] == 1)
                        initialMapCoord[i - 1, j - 1] += (int)TileFlag.DownWall;  // 9
                    else if (horizontalWalls[i - 1, j - 1] == 2)
                        initialMapCoord[i - 1, j - 1] += (int)TileFlag.DownShutter;  // 18
                    if (verticalWalls[i - 1, j - 1] == 1)
                        initialMapCoord[i - 1, j - 1] += (int)TileFlag.LeftWall;  // 3
                    else if (verticalWalls[i - 1, j - 1] == 2)
                        initialMapCoord[i - 1, j - 1] += (int)TileFlag.LeftShutter;  // 6
                    if (verticalWalls[i, j - 1] == 1)
                        initialMapCoord[i - 1, j - 1] += (int)TileFlag.RightWall; // 1
                    else if (verticalWalls[i, j - 1] == 2)
                        initialMapCoord[i - 1, j - 1] += (int)TileFlag.RightShutter; // 2
                    tilemap.SetTile(new Vector3Int(i - 1, j - 1, 0), tiles[initialMapCoord[i - 1, j - 1] % 81]);
                }
            }
        }
        tilemap.SetTile(new Vector3Int(-1, -1, 0), tiles[85]);
        tilemap.SetTile(new Vector3Int(-1, SizeY, 0), tiles[86]);
        tilemap.SetTile(new Vector3Int(SizeX, -1, 0), tiles[87]);
        tilemap.SetTile(new Vector3Int(SizeX, SizeY, 0), tiles[88]);

        /* MapManager의 인스펙터에 있는 Tiles의 인덱스 번호를 바꿈. 벽과 Shutter 오브젝트를 0~80에 배치하고, 꼭짓점, Exit, 외벽 오브젝트 등등도
        인덱스를 바꿔 줘야 한다.,(81 이상 숫자로)
        일단 기존의 16, 17, 18, 19 (순서대로 오른쪽, 왼쪽, 아래, 위 방향 외벽) 은 81, 82, 83, 84로
             기존의 20, 21, 22, 23 (순서대로 1시, 5시, 11시, 7시 방향 꼭짓점) 은 85, 86, 87, 88로 
             기존의 24, 25, 26, 27 (순서대로 왼쪽, 오른쪽, 위, 아래 방향 화살표) 은 89, 90, 91, 92로
        여기 코드에서는 고쳤으나 MapManager의 인스펙터에서도 제 위치로 Tile들을 옮겨야 함. */
        bool hasBall = false;
        if (movables != null)
        {
            foreach (Movable m in movables)
            {
                Destroy(m.gameObject);
            }
        }
        if (fixedObjects != null)
        {
            foreach (FixedObject f in fixedObjects)
            {
                Destroy(f.gameObject);
            }
        }
        if (traces != null)
        {
            foreach (GameObject g in traces)
            {
                Destroy(g);
            }
        }
        movables = new List<Movable>();
        fixedObjects = new List<FixedObject>();
        traces = new List<GameObject>();

        if (SceneManager.GetActiveScene().name == "Main" || 
            SceneManager.GetActiveScene().name == "Mode" || 
            SceneManager.GetActiveScene().name == "AdventureLevel")
        {
            foreach (Movable m in movableAndFixedGameObjects.GetComponentsInChildren<Movable>())
            {
                int x = (int)m.GetComponent<Transform>().localPosition.x;
                int y = (int)m.GetComponent<Transform>().localPosition.y;

                if (x < 1 || x > SizeX || y < 1 || y > SizeY)
                {
                    Debug.LogError("Map invalid: object position at (" + x + ", " + y + ")");
                    return;
                }
                if (initialMovableCoord[x - 1, y - 1] != null || initialMapCoord[x - 1, y - 1] >= 81)
                {
                    Debug.LogError("Map invalid: objects overlapped at (" + x + ", " + y + ")");
                    return;
                }

                movables.Add(m);

                if (m is Ball)
                {
                    if (hasBall)
                    {
                        Debug.LogError("Map invalid: too many balls");
                        return;
                    }
                    initialMovableCoord[x - 1, y - 1] = m;
                    hasBall = true;
                }
                else if (m is Iron)
                {
                    initialMovableCoord[x - 1, y - 1] = m;
                }
            }
            foreach (FixedObject f in movableAndFixedGameObjects.GetComponentsInChildren<FixedObject>())
            {
                int x = (int)f.GetComponent<Transform>().localPosition.x;
                int y = (int)f.GetComponent<Transform>().localPosition.y;

                if (x < 1 || x > SizeX || y < 1 || y > SizeY)
                {
                    Debug.LogError("Map invalid: object position at (" + x + ", " + y + ")");
                    return;
                }
                if (initialMovableCoord[x - 1, y - 1] != null || initialMapCoord[x - 1, y - 1] >= 81)
                {
                    Debug.LogError("Map invalid: objects overlapped at (" + x + ", " + y + ")");
                    return;
                }

                fixedObjects.Add(f);

                switch (f.type)
                {
                    case FixedObject.Type.Fire:
                        initialMapCoord[x - 1, y - 1] += (int)TileFlag.Fire;       // 81
                        break;
                    case FixedObject.Type.QuitGame:
                        initialMapCoord[x - 1, y - 1] += (int)TileFlag.QuitGame;   // 243
                        break;
                    case FixedObject.Type.MapEditor:
                        initialMapCoord[x - 1, y - 1] += (int)TileFlag.MapEditor;  // 729
                        break;
                    case FixedObject.Type.Adventure:
                        initialMapCoord[x - 1, y - 1] += (int)TileFlag.Adventure;  // 2187
                        break;
                    case FixedObject.Type.Tutorial:
                        initialMapCoord[x - 1, y - 1] += (int)TileFlag.Tutorial;   // 6561
                        break;
                    case FixedObject.Type.Custom:
                        initialMapCoord[x - 1, y - 1] += (int)TileFlag.Custom;     // 19683
                        break;
                    case FixedObject.Type.Survival:
                        initialMapCoord[x - 1, y - 1] += (int)TileFlag.Survival;   // 59049
                        break;
                    case FixedObject.Type.AdvEasy:
                        initialMapCoord[x - 1, y - 1] += (int)TileFlag.AdvEasy;    // 177147
                        break;
                    case FixedObject.Type.AdvNormal:
                        initialMapCoord[x - 1, y - 1] += (int)TileFlag.AdvNormal;  // 531441
                        break;
                    case FixedObject.Type.AdvHard:
                        initialMapCoord[x - 1, y - 1] += (int)TileFlag.AdvHard;    // 1594323
                        break;
                    case FixedObject.Type.AdvInsane:
                        initialMapCoord[x - 1, y - 1] += (int)TileFlag.AdvInsane;  // 4782969
                        break;
                }
            }
        }

        foreach (ObjectInfo oi in objects)
        {
            if (oi.x < 1 || oi.x > sizeX || oi.y < 1 || oi.y > sizeY)
            {
                Debug.LogError("Map invalid: object position at (" + oi.x + ", " + oi.y + ")");
                return;
            }
            if (initialMovableCoord[RotatedX(oi.x - 1, oi.y - 1), RotatedY(oi.x - 1, oi.y - 1)] != null ||
                initialMapCoord[RotatedX(oi.x - 1, oi.y - 1), RotatedY(oi.x - 1, oi.y - 1)] >= 81)
            {
                Debug.LogError("Map invalid: objects overlapped at (" + oi.x + ", " + oi.y + ")");
                return;
            }

            GameObject g;

            switch (oi.type)
            {
                case ObjectInfo.Type.Ball:
                    if (hasBall)
                    {
                        Debug.LogError("Map invalid: too many balls");
                        return;
                    }
                    g = Instantiate(ballPrefab, new Vector3(), Quaternion.identity, movableAndFixedGameObjects.transform);
                    g.transform.localPosition = RotatedVector3(new Vector3(oi.x, oi.y, 0f));
                    movables.Add(g.GetComponent<Movable>());
                    initialMovableCoord[RotatedX(oi.x - 1, oi.y - 1), RotatedY(oi.x - 1, oi.y - 1)] = g.GetComponent<Movable>();
                    hasBall = true;
                    break;
                case ObjectInfo.Type.Iron:
                    g = Instantiate(ironPrefab, new Vector3(), Quaternion.identity, movableAndFixedGameObjects.transform);
                    g.transform.localPosition = RotatedVector3(new Vector3(oi.x, oi.y, 0f));
                    movables.Add(g.GetComponent<Movable>());
                    initialMovableCoord[RotatedX(oi.x - 1, oi.y - 1), RotatedY(oi.x - 1, oi.y - 1)] = g.GetComponent<Movable>();
                    break;
                case ObjectInfo.Type.Fire:
                    g = Instantiate(firePrefab, new Vector3(), Quaternion.identity, movableAndFixedGameObjects.transform);
                    g.transform.localPosition = RotatedVector3(new Vector3(oi.x, oi.y, 0f));
                    fixedObjects.Add(g.GetComponent<FixedObject>());
                    initialMapCoord[RotatedX(oi.x - 1, oi.y - 1), RotatedY(oi.x - 1, oi.y - 1)] += (int)TileFlag.Fire;         // 81
                    break;
                /*
                // 이 친구들은 맵 에디터에서 설치하거나 맵 파일에 기록되거나 자동으로 생성될 수 없음
                case ObjectInfo.Type.QuitGame:
                    mapCoord[RotatedX(oi.x - 1, oi.y - 1), RotatedY(oi.x - 1, oi.y - 1)] += (int)TileFlag.QuitGame;     // 243
                    break;
                case ObjectInfo.Type.MapEditor:
                    mapCoord[RotatedX(oi.x - 1, oi.y - 1), RotatedY(oi.x - 1, oi.y - 1)] += (int)TileFlag.MapEditor;    // 729
                    break;
                */
            }
        }

        if (!hasBall && (SceneManager.GetActiveScene().name != "Editor" || isValidation))
        {
            Debug.LogError("Map invalid: no ball");
            return;
        }


        currentMovableCoord = (Movable[,])initialMovableCoord.Clone();
        currentMapCoord = (int[,])initialMapCoord.Clone();

        map = new Map(SizeX, SizeY, currentMapCoord, ExitX, ExitY);
        if ((SceneManager.GetActiveScene().name != "Editor" || isValidation) && !Simulate(map, initialMovableCoord, RotatedSolution(solution)))
        {
            Debug.LogError("Map invalid: impossible to clear");
            return;
        }
        currentMapCoord = (int[,])initialMapCoord.Clone();
        map = new Map(SizeX, SizeY, currentMapCoord, ExitX, ExitY);

        mainCamera.transform.position = new Vector3((SizeX + 1) / 2f, (SizeY + 1) / 2f - 0.25f, -10f);
        if (Screen.height * 9f / Screen.width <= 18)
        {
            mainCamera.orthographicSize = Mathf.Max(SizeX, SizeY + 1) * 0.5f + 1f;
        }
        else if (Screen.height * 9f / Screen.width >= 21)
        {
            mainCamera.orthographicSize = Mathf.Max(SizeX + 1, SizeY) * 0.6f + 0.7f;
        }
        else
        {
            mainCamera.orthographicSize = Mathf.Max(SizeX, SizeY) * 0.55f + 1.3f;
        }

        gravityBall.anchoredPosition = new Vector3(0f, 0f);

        TimeLimit = Mathf.Max(3f, timeLimit);
        ActionHistory = "";
        IsReady = true;
        HasCleared = false;
        HasDied = false;
        IsTimeActivated = false;
        IsTimePassing = false;
        HasTimePaused = false;
        RemainingTime = 0f;
        //PrintMapCoord();
    }

    public OpenFileFlag InitializeFromFile(string path, out int tempSizeX, out int tempSizeY,
        out List<ObjectInfo> tempObjects, out List<WallInfo> tempWalls, 
        out string tempSolution, out float tempTimeLimit, StatusUI statusUI = null)
    {
        tempSizeX = 0;
        tempSizeY = 0;
        tempObjects = new List<ObjectInfo>();
        tempWalls = new List<WallInfo>();
        tempSolution = "";
        tempTimeLimit = DEFAULT_TIME_LIMIT;

        try
        {
            if (!File.Exists(path))
            {
                Debug.LogError("File invalid: there is no file \"" + Path.GetFileNameWithoutExtension(path) + "\"");
                statusUI.SetStatusMessageWithFlashing("The map doesn't exist anymore.", 2f);
                return OpenFileFlag.Failed;
            }
            else if (Path.GetExtension(path) != ".txt")
            {
                Debug.LogError("File invalid: \"" + Path.GetFileNameWithoutExtension(path) + "\" is not a .txt file");
                statusUI.SetStatusMessageWithFlashing("The file is not a valid map file.", 2f);
                return OpenFileFlag.Failed;
            }
        }
        catch (Exception)
        {
            Debug.LogError("File invalid: exception while checking a file");
            statusUI.SetStatusMessageWithFlashing("Something went wrong while checking a file.", 3f);
            throw;
        }

        FileStream fs = new FileStream(path, FileMode.Open);
        StreamReader sr = new StreamReader(fs, Encoding.UTF8);

#region parsing text file
        try
        {
            // sizeX, sizeY
            string text = sr.ReadToEnd();
            return InitializeFromText(text, out tempSizeX, out tempSizeY, out tempObjects, out tempWalls, out tempSolution, out tempTimeLimit, statusUI);
        }
        catch (Exception e)
        {
            Debug.LogError("File invalid: exception while opening a map");
            statusUI?.SetStatusMessageWithFlashing("Cannot open the map:\ninvalid map file", 1.5f);
            Debug.LogException(e);
            return OpenFileFlag.Restore;
        }
        finally
        {
            sr.Close();
            fs.Close();
        }
#endregion
    }

    public OpenFileFlag InitializeFromText(string text, out int tempSizeX, out int tempSizeY,
        out List<ObjectInfo> tempObjects, out List<WallInfo> tempWalls,
        out string tempSolution, out float tempTimeLimit, StatusUI statusUI = null)
    {
        tempSizeX = 0;
        tempSizeY = 0;
        tempObjects = new List<ObjectInfo>();
        tempWalls = new List<WallInfo>();
        tempSolution = "";
        tempTimeLimit = DEFAULT_TIME_LIMIT;

#region parsing text file
        // sizeX, sizeY
        string line = text.Substring(0, text.IndexOf('\n'));
        string[] token = line.Split(' ');

        if (token.Length != 2)
        {
            Debug.LogError("File invalid: map size (" + line + ")");
            statusUI?.SetStatusMessageWithFlashing("Cannot open the map:\nsize error", 1.5f);
            return OpenFileFlag.Failed;
        }

        tempSizeX = int.Parse(token[0]);
        tempSizeY = int.Parse(token[1]);

        bool hasSolution = false;
        string lines = text.Substring(text.IndexOf('\n') + 1);
        foreach (string l in lines.Split('\n'))
        {
            token = l.Split(' ');
            if (l == "" || token.Length == 0) continue;

            switch (token[0])
            {
                case "@":
                    if (token.Length != 3)
                    {
                        Debug.LogError("File invalid: ball (" + l + ")");
                        statusUI?.SetStatusMessageWithFlashing("Cannot open the map:\nball error", 1.5f);
                        return OpenFileFlag.Failed;
                    }
                    tempObjects.Add(new ObjectInfo(ObjectInfo.Type.Ball, int.Parse(token[1]), int.Parse(token[2])));
                    break;
                case "#":
                    if (token.Length != 3)
                    {
                        Debug.LogError("File invalid: iron (" + l + ")");
                        statusUI?.SetStatusMessageWithFlashing("Cannot open the map:\niron error", 1.5f);
                        return OpenFileFlag.Failed;
                    }
                    tempObjects.Add(new ObjectInfo(ObjectInfo.Type.Iron, int.Parse(token[1]), int.Parse(token[2])));
                    break;
                case "*":
                    if (token.Length != 3)
                    {
                        Debug.LogError("File invalid: fire (" + l + ")");
                        statusUI?.SetStatusMessageWithFlashing("Cannot open the map:\nfire error", 1.5f);
                        return OpenFileFlag.Failed;
                    }
                    tempObjects.Add(new ObjectInfo(ObjectInfo.Type.Fire, int.Parse(token[1]), int.Parse(token[2])));
                    break;
                case "$":
                    if (token.Length != 4)
                    {
                        Debug.LogError("File invalid: exit (" + l + ")");
                        statusUI?.SetStatusMessageWithFlashing("Cannot open the map:\nexit error", 1.5f);
                        return OpenFileFlag.Failed;
                    }

                    if (token[1].Equals("-"))
                    {
                        tempWalls.Add(new WallInfo(WallInfo.Type.ExitHorizontal, int.Parse(token[2]), int.Parse(token[3])));
                    }
                    else if (token[1].Equals("|"))
                    {
                        tempWalls.Add(new WallInfo(WallInfo.Type.ExitVertical, int.Parse(token[2]), int.Parse(token[3])));
                    }
                    else
                    {
                        Debug.LogError("File invalid: exit (" + l + ")");
                        statusUI?.SetStatusMessageWithFlashing("Cannot open the map:\nexit error", 1.5f);
                        return OpenFileFlag.Failed;
                    }
                    break;
                case "-":
                    if (token.Length != 3)
                    {
                        Debug.LogError("File invalid: horizontal wall (" + l + ")");
                        statusUI?.SetStatusMessageWithFlashing("Cannot open the map:\nwall error", 1.5f);
                        return OpenFileFlag.Failed;
                    }
                    tempWalls.Add(new WallInfo(WallInfo.Type.Horizontal, int.Parse(token[1]), int.Parse(token[2])));
                    break;
                case "|":
                    if (token.Length != 3)
                    {
                        Debug.LogError("File invalid: vertical wall (" + l + ")");
                        statusUI?.SetStatusMessageWithFlashing("Cannot open the map:\nwall error", 1.5f);
                        return OpenFileFlag.Failed;
                    }
                    tempWalls.Add(new WallInfo(WallInfo.Type.Vertical, int.Parse(token[1]), int.Parse(token[2])));
                    break;
                case ":":
                    if (token.Length != 4)
                    {
                        Debug.LogError("File Invalid: shutter (" + l + ")");
                        statusUI?.SetStatusMessageWithFlashing("Cannot open the map:\nshutter error", 1.5f);
                        return OpenFileFlag.Failed;
                    }
                    if (token[1].Equals("-"))
                    {
                        tempWalls.Add(new WallInfo(WallInfo.Type.HorizontalShutter, int.Parse(token[2]), int.Parse(token[3])));
                    }
                    else if (token[1].Equals("|"))
                    {
                        tempWalls.Add(new WallInfo(WallInfo.Type.VerticalShutter, int.Parse(token[2]), int.Parse(token[3])));
                    }
                    else
                    {
                        Debug.LogError("File invalid: shutter (" + l + ")");
                        statusUI?.SetStatusMessageWithFlashing("Cannot open the map:\nshutter error", 1.5f);
                        return OpenFileFlag.Failed;
                    }
                    break;
                case "t":
                    if (token.Length != 2)
                    {
                        Debug.LogError("File invalid: time limit (" + l + ")");
                        statusUI?.SetStatusMessageWithFlashing("Cannot open the map:\ntime limit error", 1.5f);
                        return OpenFileFlag.Failed;
                    }
                    tempTimeLimit = Mathf.Clamp(float.Parse(token[1]), MIN_TIME_LIMIT, MAX_TIME_LIMIT);
                    break;
                case "s":
                    if (token.Length != 2)
                    {
                        Debug.LogError("File invalid: solution (" + l + ")");
                        statusUI?.SetStatusMessageWithFlashing("Cannot open the map:\nsolution error", 1.5f);
                        return OpenFileFlag.Failed;
                    }
                    else if (hasSolution)
                    {
                        Debug.LogError("File invalid: solution already exists (" + l + ")");
                        statusUI?.SetStatusMessageWithFlashing("Cannot open the map:\nmultiple solutions", 1.5f);
                        return OpenFileFlag.Failed;
                    }
                    // http://www.nowan.hu/main.aspx?content=9cff1555-26ca-4e6a-910b-6a73463e22b2
                    try
                    {
                        byte[] tempByte = Convert.FromBase64String(token[1]);
                        UTF8Encoding encoder = new UTF8Encoding();
                        Decoder decoder = encoder.GetDecoder();
                        int count = decoder.GetCharCount(tempByte, 0, tempByte.Length);
                        char[] decodedChar = new char[count];
                        decoder.GetChars(tempByte, 0, tempByte.Length, decodedChar, 0);
                        tempSolution = new string(decodedChar);
                    }
                    catch (FormatException)
                    {
                        Debug.LogError("File invalid: wrong solution format (" + l + ")");
                        statusUI?.SetStatusMessageWithFlashing("Cannot open the map:\ninvalid solution", 1.5f);
                        return OpenFileFlag.Failed;
                    }
                    hasSolution = true;
                    break;
                default:
                    Debug.LogError("File invalid: unknown (" + l + ")");
                    statusUI?.SetStatusMessageWithFlashing("Cannot open the map:\nunknown symbols", 1.5f);
                    return OpenFileFlag.Failed;
            }
        }
#endregion

        // Map validation
        Initialize(tempSizeX, tempSizeY, tempWalls, tempObjects, tempSolution, tempTimeLimit, true, true);
        if (!IsReady)
        {
            Debug.LogError("File invalid: map validation failed");
            statusUI?.SetStatusMessageWithFlashing("Cannot open the map:\ninvalid map or impossible to clear", 3f);
            return OpenFileFlag.Restore;
        }

        return OpenFileFlag.Success;
    }

    /// <summary>
    /// 시간 흐름(게임 플레이) 활성화 함수. 반드시 Initialize() 호출 직후에만 호출할 것.
    /// </summary>
    public void TimeActivate()
    {
        if (!IsReady) return;

        gravityUpButton.interactable = true;
        gravityDownButton.interactable = true;
        gravityLeftButton.interactable = true;
        gravityRightButton.interactable = true;

        RemainingTime = TimeLimit;
        IsTimeActivated = true;
        IsTimePassing = false;
        HasTimePaused = false;
        timeoutPanel.SetActive(false);
        //Debug.Log("Remaining time: " + RemainingTime);
    }

    public void TimePause()
    {
        if (!IsReady || !IsTimeActivated) return;
        HasTimePaused = true;
    }

    public void TimeResume()
    {
        if (!IsReady || !IsTimeActivated) return;
        HasTimePaused = false;
    }

    private bool Simulate(Map map, Movable[,] initialMovableCoord, string solution)
    {
        Movable[,] mutableMovableCoord = (Movable[,])initialMovableCoord.Clone();

        foreach (char direction in solution.ToCharArray())
        {
            Flag flag;
            int ballX, ballY;
            switch (direction)
            {
                case 'a':
                    mutableMovableCoord = Gravity(map, mutableMovableCoord, GameManager.GravityDirection.Left, true, out flag, out ballX, out ballY);
                    break;
                case 's':
                    mutableMovableCoord = Gravity(map, mutableMovableCoord, GameManager.GravityDirection.Down, true, out flag, out ballX, out ballY);
                    break;
                case 'd':
                    mutableMovableCoord = Gravity(map, mutableMovableCoord, GameManager.GravityDirection.Right, true, out flag, out ballX, out ballY);
                    break;
                case 'w':
                    mutableMovableCoord = Gravity(map, mutableMovableCoord, GameManager.GravityDirection.Up, true, out flag, out ballX, out ballY);
                    break;
                default:
                    Debug.LogError("Map invalid: wrong solution format");
                    return false;
            }
            switch (flag)
            {
                case Flag.Escaped:
                    return true;
                case Flag.Burned:
                    return false;
                case Flag.Squashed:
                    return false;
            }
        }
        return false;
    }

    /// <summary>
    /// 현재 맵과 남은 시간을 초기 상태로 되돌리는 함수.
    /// </summary>
    public void RestartWithTime()
    {
        if (!IsReady || (RemainingTime > 0f && !HasCleared)) return;
        TimeActivate();
        RestartHelper();
    }

    /// <summary>
    /// 현재 맵을 초기 상태로 되돌리는 함수. 남은 시간은 되돌리지 않습니다.
    /// </summary>
    public void Restart()
    {
        if (!IsReady || RemainingTime <= 0f || HasCleared) return;
        RestartHelper();
    }

    private void RestartHelper()
    {
        IsReady = false;

        for (int i = 0; i < SizeX; i++)
        {
            for (int j = 0; j < SizeY; j++)
            {
                if (initialMovableCoord[i, j] != null)
                {
                    initialMovableCoord[i, j].gameObject.SetActive(true);
                    initialMovableCoord[i, j].transform.localPosition = new Vector3(i + 1, j + 1, 0f);
                }
            }
        }

        for (int i = 0; i < SizeX; i++)
        {
            for (int j = 0; j < SizeY; j++)
            {
                tilemap.SetTile(new Vector3Int(i, j, 0), tiles[initialMapCoord[i, j] % 81]);
            }
        }

        if (traces != null)
        {
            foreach (GameObject g in traces)
            {
                Destroy(g);
            }
        }
        traces = new List<GameObject>();

        currentMovableCoord = (Movable[,])initialMovableCoord.Clone();
        currentMapCoord = (int[,])initialMapCoord.Clone();

        map = new Map(SizeX, SizeY, currentMapCoord, ExitX, ExitY);

        gravityBall.anchoredPosition = new Vector3(0f, 0f);
        gravityUpButton.interactable = true;
        gravityDownButton.interactable = true;
        gravityLeftButton.interactable = true;
        gravityRightButton.interactable = true;
        if (afterGravity.GetInvocationList().Length > 0)
            afterGravity(Flag.Continued);

        ActionHistory = "";
        HasCleared = false;
        HasDied = false;
        IsReady = true;
    }

    public void ManipulateGravityUp()
    {
        if (!IsReady || HasCleared || HasDied || RemainingTime <= 0f) return;
        IsTimePassing = true;
        gravityBall.anchoredPosition = new Vector3(0f, 264f);
        gravityUpButton.interactable = false;
        gravityDownButton.interactable = true;
        gravityLeftButton.interactable = true;
        gravityRightButton.interactable = true;
        GameManager.mm.Gravity(GameManager.GravityDirection.Up, out Flag flag);
        if (afterGravity.GetInvocationList().Length > 0)
            afterGravity(flag);
    }

    public void ManipulateGravityDown()
    {
        if (!IsReady || HasCleared || HasDied || RemainingTime <= 0f) return;
        IsTimePassing = true;
        gravityBall.anchoredPosition = new Vector3(0f, -264f);
        gravityUpButton.interactable = true;
        gravityDownButton.interactable = false;
        gravityLeftButton.interactable = true;
        gravityRightButton.interactable = true;
        GameManager.mm.Gravity(GameManager.GravityDirection.Down, out Flag flag);
        if (afterGravity.GetInvocationList().Length > 0)
            afterGravity(flag);
    }

    public void ManipulateGravityLeft()
    {
        if (!IsReady || HasCleared || HasDied || RemainingTime <= 0f) return;
        IsTimePassing = true;
        gravityBall.anchoredPosition = new Vector3(-264f, 0f);
        gravityUpButton.interactable = true;
        gravityDownButton.interactable = true;
        gravityLeftButton.interactable = false;
        gravityRightButton.interactable = true;
        GameManager.mm.Gravity(GameManager.GravityDirection.Left, out Flag flag);
        if (afterGravity.GetInvocationList().Length > 0)
            afterGravity(flag);
    }

    public void ManipulateGravityRight()
    {
        if (!IsReady || HasCleared || HasDied || RemainingTime <= 0f) return;
        IsTimePassing = true;
        gravityBall.anchoredPosition = new Vector3(264f, 0f);
        gravityUpButton.interactable = true;
        gravityDownButton.interactable = true;
        gravityLeftButton.interactable = true;
        gravityRightButton.interactable = false;
        GameManager.mm.Gravity(GameManager.GravityDirection.Right, out Flag flag);
        if (afterGravity.GetInvocationList().Length > 0)
            afterGravity(flag);
    }

    /// <summary>
    /// 중력을 조작하여 게임에 변화를 가하는 함수. 공이 탈출한 상태에서는 호출해도 효과가 없습니다.
    /// </summary>
    /// <param name="gravityDirection">중력을 가하는 방향</param>
    /// <param name="flag">결과 플래그</param>
    public void Gravity(GameManager.GravityDirection gravityDirection, out Flag flag)
    {
        flag = Flag.Continued;
        if (!IsReady || HasCleared || HasDied || RemainingTime <= 0f) return;

        currentMovableCoord = Gravity(map, currentMovableCoord, gravityDirection, false, out flag, out _, out _);

        if (flag == Flag.Escaped)
        {
            HasCleared = true;
        }
        else if (flag == Flag.Burned || flag == Flag.Squashed)
        {
            HasDied = true;
        }
    }

    private Movable[,] Gravity(Map map, Movable[,] mutableMovableCoord,
        GameManager.GravityDirection gravityDirection, bool isSimulation, out Flag flag, out int ballX, out int ballY)
    {
        flag = Flag.Continued;
        ballX = -1;
        ballY = -1;

        if (!isSimulation)
        {
            if (traces != null)
            {
                foreach (GameObject g in traces)
                {
                    Destroy(g);
                }
            }
            traces = new List<GameObject>();
        }

        GameObject[,] traceCoord = new GameObject[map.sizeX, map.sizeY];

        switch (gravityDirection)
        {
            case GameManager.GravityDirection.Up:
                for (int i = SizeX - 1; i >= 0; i--)
                {
                    for (int j = SizeY - 1; j >= 0; j--)
                    {
                        if (mutableMovableCoord[i, j] != null)
                        {
                            // j++
                            #region Up
                            for (int k = j; k <= SizeY; k++)
                            {
                                if (k == SizeY)
                                {
                                    if (ExitX != i + 1 || ExitY != k + 1)
                                    {
                                        Debug.LogError("Gravity invalid: wrong exit at (" + (i + 1) + ", " + (k + 1) + ")");
                                        break;
                                    }
                                    if (mutableMovableCoord[i, j] is Ball)
                                    {
                                        flag = Flag.Escaped;
                                        ballX = ExitX;
                                        ballY = ExitY;
                                        if (!isSimulation)
                                        {
                                            Debug.Log("Stage clear!");
                                            // TODO: (i + 1, k + 1)를 공이 지나 탈출하는 애니메이션 재생
                                            // 탈출하는 순간을 느리게 보여줘도 될 듯?
                                            mutableMovableCoord[i, j].gameObject.SetActive(false);
                                        }
                                        mutableMovableCoord[i, j] = null;
                                        break;
                                    }
                                    else if (mutableMovableCoord[i, j] is Iron)
                                    {
                                        if (!isSimulation)
                                        {
                                            // TODO: (i + 1, k + 1)를 쇠가 지나 탈출하는 애니메이션 재생
                                            mutableMovableCoord[i, j].gameObject.SetActive(false);
                                        }
                                        mutableMovableCoord[i, j] = null;
                                        break;
                                    }
                                }
                                if (mutableMovableCoord[i, j] is Ball && CheckTileFlag(map.mapCoord[i, k], TileFlag.Fire))
                                {
                                    flag = Flag.Burned;
                                    ballX = i + 1;
                                    ballY = k + 1;
                                    Debug.Log("The ball is burned at (" + ballX + ", " + ballY + ")");
                                    if (!isSimulation)
                                    {
                                        GameObject g = Instantiate(flagBurnedPrefab, new Vector3(), Quaternion.identity, movableAndFixedGameObjects.transform);
                                        g.transform.localPosition = new Vector3(i + 1, k + 1, 0f);
                                        if (traceCoord[i, k] != null)
                                        {
                                            traces.Remove(traceCoord[i, k]);
                                            Destroy(traceCoord[i, k]);
                                        }
                                        traceCoord[i, k] = g;
                                        traces.Add(g);
                                        mutableMovableCoord[i, j].gameObject.SetActive(false);
                                    }
                                    mutableMovableCoord[i, j] = null;
                                    break;
                                }
                                if (mutableMovableCoord[i, j] is Ball && CheckTileFlag(map.mapCoord[i, k], TileFlag.QuitGame))
                                {
                                    flag = Flag.QuitGame;
                                    ballX = i + 1;
                                    ballY = k + 1;
                                    if (!isSimulation)
                                    {
                                        mutableMovableCoord[i, j].gameObject.SetActive(false);
                                    }
                                    mutableMovableCoord[i, j] = null;
                                    //GameManager.gm.QuitGame();
                                    break;
                                }
                                if (mutableMovableCoord[i, j] is Ball && CheckTileFlag(map.mapCoord[i, k], TileFlag.MapEditor))
                                {
                                    flag = Flag.MapEditor;
                                    ballX = i + 1;
                                    ballY = k + 1;
                                    if (!isSimulation)
                                    {
                                        mutableMovableCoord[i, j].gameObject.SetActive(false);
                                    }
                                    mutableMovableCoord[i, j] = null;
                                    //GameManager.gm.MapEditor();
                                    break;
                                }
                                if (mutableMovableCoord[i, j] is Iron && mutableMovableCoord[i, k] != null && mutableMovableCoord[i, k] is Ball)
                                {
                                    flag = Flag.Squashed;
                                    ballX = i + 1;
                                    ballY = k + 1;
                                    Debug.Log("The iron at (" + ballX + ", " + (j + 1) + ") squashes the ball at (" + ballX + ", " + ballY + ")");
                                    if (!isSimulation)
                                    {
                                        GameObject g = Instantiate(flagSquashedPrefab, new Vector3(), Quaternion.identity, movableAndFixedGameObjects.transform);
                                        g.transform.localPosition = new Vector3(i + 1, k + 1, 0f);
                                        if (traceCoord[i, k] != null)
                                        {
                                            traces.Remove(traceCoord[i, k]);
                                            Destroy(traceCoord[i, k]);
                                        }
                                        traceCoord[i, k] = g;
                                        traces.Add(g);
                                        mutableMovableCoord[i, k].gameObject.SetActive(false);
                                    }
                                    mutableMovableCoord[i, k] = null;
                                }
                                if (CheckTileFlag(map.mapCoord[i, k], TileFlag.UpWall) ||
                                    (k <= SizeY - 2 && mutableMovableCoord[i, k + 1] != null && mutableMovableCoord[i, k + 1] is Iron))
                                {
                                    if (mutableMovableCoord[i, j] is Ball)
                                    {
                                        ballX = i + 1;
                                        ballY = k + 1;
                                    }
                                    if (!isSimulation)
                                    {
                                        mutableMovableCoord[i, j].transform.localPosition = new Vector3(i + 1, k + 1, 0f);
                                    }
                                    mutableMovableCoord[i, k] = mutableMovableCoord[i, j];
                                    if (k != j)
                                        mutableMovableCoord[i, j] = null;
                                    break;
                                }
                                if (CheckTileFlag(map.mapCoord[i, k], TileFlag.UpShutter) && mutableMovableCoord[i, k + 1] == null && k <= SizeY - 2)
                                {
                                    if (mutableMovableCoord[i, j] is Ball)
                                    {
                                        map.mapCoord[i, k] -= (int)TileFlag.UpShutter / 2;
                                        map.mapCoord[i, k + 1] -= (int)TileFlag.DownShutter / 2;
                                        if (!isSimulation)
                                        {
                                            tilemap.SetTile(new Vector3Int(i, k, 0), tiles[map.mapCoord[i, k] % 81]);
                                            tilemap.SetTile(new Vector3Int(i, k + 1, 0), tiles[map.mapCoord[i, k + 1] % 81]);
                                        }
                                    }
                                }
                                // 현재 k에서 멈추지 않는 경우 잔상 생성
                                if (!isSimulation)
                                {
                                    if (mutableMovableCoord[i, j] is Ball)
                                    {
                                        GameObject g = Instantiate(ballTracePrefabs[0], new Vector3(), Quaternion.identity, movableAndFixedGameObjects.transform);
                                        g.transform.localPosition = new Vector3(i + 1, k + 1, 0f);
                                        if (traceCoord[i, k] != null)
                                        {
                                            traces.Remove(traceCoord[i, k]);
                                            Destroy(traceCoord[i, k]);
                                        }
                                        traceCoord[i, k] = g;
                                        traces.Add(g);
                                    }
                                    else if (mutableMovableCoord[i, j] is Iron)
                                    {
                                        GameObject g = Instantiate(ironTracePrefabs[0], new Vector3(), Quaternion.identity, movableAndFixedGameObjects.transform);
                                        g.transform.localPosition = new Vector3(i + 1, k + 1, 0f);
                                        if (traceCoord[i, k] != null && !traceCoord[i, k].CompareTag("Flag"))
                                        {
                                            traces.Remove(traceCoord[i, k]);
                                            Destroy(traceCoord[i, k]);
                                        }
                                        traceCoord[i, k] = g;
                                        traces.Add(g);
                                    }
                                }
                            }
#endregion
                        }
                    }
                }
                ActionHistory += "w";
                break;
            case GameManager.GravityDirection.Down:
                for (int i = 0; i < SizeX; i++)
                {
                    for (int j = 0; j < SizeY; j++)
                    {
                        if (mutableMovableCoord[i, j] != null)
                        {
                            // j--
#region Down
                            for (int k = j; k >= -1; k--)
                            {
                                if (k == -1)
                                {
                                    if (ExitX != i + 1 || ExitY != k + 1)
                                    {
                                        Debug.LogError("Gravity invalid: wrong exit at (" + (i + 1) + ", " + (k + 1) + ")");
                                        break;
                                    }
                                    if (mutableMovableCoord[i, j] is Ball)
                                    {
                                        flag = Flag.Escaped;
                                        ballX = ExitX;
                                        ballY = ExitY;
                                        if (!isSimulation)
                                        {
                                            Debug.Log("Stage clear!");
                                            // TODO: (i + 1, k + 1)를 공이 지나 탈출하는 애니메이션 재생
                                            // 탈출하는 순간을 느리게 보여줘도 될 듯?
                                            mutableMovableCoord[i, j].gameObject.SetActive(false);
                                        }
                                        mutableMovableCoord[i, j] = null;
                                        break;
                                    }
                                    else if (mutableMovableCoord[i, j] is Iron)
                                    {
                                        if (!isSimulation)
                                        {
                                            // TODO: (i + 1, k + 1)를 쇠가 지나 탈출하는 애니메이션 재생
                                            mutableMovableCoord[i, j].gameObject.SetActive(false);
                                        }
                                        mutableMovableCoord[i, j] = null;
                                        break;
                                    }
                                }
                                if (mutableMovableCoord[i, j] is Ball && CheckTileFlag(map.mapCoord[i, k], TileFlag.Fire))
                                {
                                    flag = Flag.Burned;
                                    ballX = i + 1;
                                    ballY = k + 1;
                                    Debug.Log("The ball is burned at (" + ballX + ", " + ballY + ")");
                                    if (!isSimulation)
                                    {
                                        GameObject g = Instantiate(flagBurnedPrefab, new Vector3(), Quaternion.identity, movableAndFixedGameObjects.transform);
                                        g.transform.localPosition = new Vector3(i + 1, k + 1, 0f);
                                        if (traceCoord[i, k] != null)
                                        {
                                            traces.Remove(traceCoord[i, k]);
                                            Destroy(traceCoord[i, k]);
                                        }
                                        traceCoord[i, k] = g;
                                        traces.Add(g);
                                        mutableMovableCoord[i, j].gameObject.SetActive(false);
                                    }
                                    mutableMovableCoord[i, j] = null;
                                    break;
                                }
                                if (mutableMovableCoord[i, j] is Ball && CheckTileFlag(map.mapCoord[i, k], TileFlag.QuitGame))
                                {
                                    flag = Flag.QuitGame;
                                    ballX = i + 1;
                                    ballY = k + 1;
                                    if (!isSimulation)
                                    {
                                        mutableMovableCoord[i, j].gameObject.SetActive(false);
                                    }
                                    mutableMovableCoord[i, j] = null;
                                    //GameManager.gm.QuitGame();
                                    break;
                                }
                                if (mutableMovableCoord[i, j] is Ball && CheckTileFlag(map.mapCoord[i, k], TileFlag.MapEditor))
                                {
                                    flag = Flag.MapEditor;
                                    ballX = i + 1;
                                    ballY = k + 1;
                                    if (!isSimulation)
                                    {
                                        mutableMovableCoord[i, j].gameObject.SetActive(false);
                                    }
                                    mutableMovableCoord[i, j] = null;
                                    //GameManager.gm.MapEditor();
                                    break;
                                }
                                if (mutableMovableCoord[i, j] is Iron && mutableMovableCoord[i, k] != null && mutableMovableCoord[i, k] is Ball)
                                {
                                    flag = Flag.Squashed;
                                    ballX = i + 1;
                                    ballY = k + 1;
                                    Debug.Log("The iron at (" + ballX + ", " + (j + 1) + ") squashes the ball at (" + ballX + ", " + ballY + ")");
                                    if (!isSimulation)
                                    {
                                        GameObject g = Instantiate(flagSquashedPrefab, new Vector3(), Quaternion.identity, movableAndFixedGameObjects.transform);
                                        g.transform.localPosition = new Vector3(i + 1, k + 1, 0f);
                                        if (traceCoord[i, k] != null)
                                        {
                                            traces.Remove(traceCoord[i, k]);
                                            Destroy(traceCoord[i, k]);
                                        }
                                        traceCoord[i, k] = g;
                                        traces.Add(g);
                                        mutableMovableCoord[i, k].gameObject.SetActive(false);
                                    }
                                    mutableMovableCoord[i, k] = null;
                                }
                                if (CheckTileFlag(map.mapCoord[i, k], TileFlag.DownWall) ||
                                    (k >= 1 && mutableMovableCoord[i, k - 1] != null && mutableMovableCoord[i, k - 1] is Iron))
                                {
                                    if (mutableMovableCoord[i, j] is Ball)
                                    {
                                        ballX = i + 1;
                                        ballY = k + 1;
                                    }
                                    if (!isSimulation)
                                    {
                                        mutableMovableCoord[i, j].transform.localPosition = new Vector3(i + 1, k + 1, 0f);
                                    }
                                    mutableMovableCoord[i, k] = mutableMovableCoord[i, j];
                                    if (k != j)
                                        mutableMovableCoord[i, j] = null;
                                    break;
                                }
                                if (CheckTileFlag(map.mapCoord[i, k], TileFlag.DownShutter) && mutableMovableCoord[i, k - 1] == null && k >= 1)
                                {
                                    if (mutableMovableCoord[i, j] is Ball)
                                    {
                                        map.mapCoord[i, k] -= (int)TileFlag.DownShutter / 2;
                                        map.mapCoord[i, k - 1] -= (int)TileFlag.UpShutter / 2;
                                        if (!isSimulation)
                                        {
                                            tilemap.SetTile(new Vector3Int(i, k, 0), tiles[map.mapCoord[i, k] % 81]);
                                            tilemap.SetTile(new Vector3Int(i, k - 1, 0), tiles[map.mapCoord[i, k - 1] % 81]);
                                        }
                                    }
                                }
                                // 현재 k에서 멈추지 않는 경우 잔상 생성
                                if (!isSimulation)
                                {
                                    if (mutableMovableCoord[i, j] is Ball)
                                    {
                                        GameObject g = Instantiate(ballTracePrefabs[1], new Vector3(), Quaternion.identity, movableAndFixedGameObjects.transform);
                                        g.transform.localPosition = new Vector3(i + 1, k + 1, 0f);
                                        if (traceCoord[i, k] != null)
                                        {
                                            traces.Remove(traceCoord[i, k]);
                                            Destroy(traceCoord[i, k]);
                                        }
                                        traceCoord[i, k] = g;
                                        traces.Add(g);
                                    }
                                    else if (mutableMovableCoord[i, j] is Iron)
                                    {
                                        GameObject g = Instantiate(ironTracePrefabs[1], new Vector3(), Quaternion.identity, movableAndFixedGameObjects.transform);
                                        g.transform.localPosition = new Vector3(i + 1, k + 1, 0f);
                                        if (traceCoord[i, k] != null && !traceCoord[i, k].CompareTag("Flag"))
                                        {
                                            traces.Remove(traceCoord[i, k]);
                                            Destroy(traceCoord[i, k]);
                                        }
                                        traceCoord[i, k] = g;
                                        traces.Add(g);
                                    }
                                }
                            }
#endregion
                        }
                    }
                }
                ActionHistory += "s";
                break;
            case GameManager.GravityDirection.Left:
                for (int i = 0; i < SizeX; i++)
                {
                    for (int j = 0; j < SizeY; j++)
                    {
                        if (mutableMovableCoord[i, j] != null)
                        {
                            // i--
#region Left
                            for (int k = i; k >= -1; k--)
                            {
                                if (k == -1)
                                {
                                    if (ExitX != k + 1 || ExitY != j + 1)
                                    {
                                        Debug.LogError("Gravity invalid: wrong exit at (" + (k + 1) + ", " + (j + 1) + ")");
                                        break;
                                    }
                                    if (mutableMovableCoord[i, j] is Ball)
                                    {
                                        flag = Flag.Escaped;
                                        ballX = ExitX;
                                        ballY = ExitY;
                                        if (!isSimulation)
                                        {
                                            Debug.Log("Stage clear!");
                                            // TODO: (k + 1, j + 1)를 공이 지나 탈출하는 애니메이션 재생
                                            // 탈출하는 순간을 느리게 보여줘도 될 듯?
                                            mutableMovableCoord[i, j].gameObject.SetActive(false);
                                        }
                                        mutableMovableCoord[i, j] = null;
                                        break;
                                    }
                                    else if (mutableMovableCoord[i, j] is Iron)
                                    {
                                        if (!isSimulation)
                                        {
                                            // TODO: (k + 1, j + 1)를 쇠가 지나 탈출하는 애니메이션 재생
                                            mutableMovableCoord[i, j].gameObject.SetActive(false);
                                        }
                                        mutableMovableCoord[i, j] = null;
                                        break;
                                    }
                                }
                                if (mutableMovableCoord[i, j] is Ball && CheckTileFlag(map.mapCoord[k, j], TileFlag.Fire))
                                {
                                    flag = Flag.Burned;
                                    ballX = k + 1;
                                    ballY = j + 1;
                                    Debug.Log("The ball is burned at (" + ballX + ", " + ballY + ")");
                                    if (!isSimulation)
                                    {
                                        GameObject g = Instantiate(flagBurnedPrefab, new Vector3(), Quaternion.identity, movableAndFixedGameObjects.transform);
                                        g.transform.localPosition = new Vector3(k + 1, j + 1, 0f);
                                        if (traceCoord[k, j] != null)
                                        {
                                            traces.Remove(traceCoord[k, j]);
                                            Destroy(traceCoord[k, j]);
                                        }
                                        traceCoord[k, j] = g;
                                        traces.Add(g);
                                        mutableMovableCoord[i, j].gameObject.SetActive(false);
                                    }
                                    mutableMovableCoord[i, j] = null;
                                    break;
                                }
                                if (mutableMovableCoord[i, j] is Ball && CheckTileFlag(map.mapCoord[k, j], TileFlag.QuitGame))
                                {
                                    flag = Flag.QuitGame;
                                    ballX = k + 1;
                                    ballY = j + 1;
                                    if (!isSimulation)
                                    {
                                        mutableMovableCoord[i, j].gameObject.SetActive(false);
                                    }
                                    mutableMovableCoord[i, j] = null;
                                    //GameManager.gm.QuitGame();
                                    break;
                                }
                                if (mutableMovableCoord[i, j] is Ball && CheckTileFlag(map.mapCoord[k, j], TileFlag.MapEditor))
                                {
                                    flag = Flag.MapEditor;
                                    ballX = k + 1;
                                    ballY = j + 1;
                                    if (!isSimulation)
                                    {
                                        mutableMovableCoord[i, j].gameObject.SetActive(false);
                                    }
                                    mutableMovableCoord[i, j] = null;
                                    //GameManager.gm.MapEditor();
                                    break;
                                }
                                if (mutableMovableCoord[i, j] is Iron && mutableMovableCoord[k, j] != null && mutableMovableCoord[k, j] is Ball)
                                {
                                    flag = Flag.Squashed;
                                    ballX = k + 1;
                                    ballY = j + 1;
                                    Debug.Log("The iron at (" + (i + 1) + ", " + ballY + ") squashes the ball at (" + ballX + ", " + ballY + ")");
                                    if (!isSimulation)
                                    {
                                        GameObject g = Instantiate(flagSquashedPrefab, new Vector3(), Quaternion.identity, movableAndFixedGameObjects.transform);
                                        g.transform.localPosition = new Vector3(k + 1, j + 1, 0f);
                                        if (traceCoord[k, j] != null)
                                        {
                                            traces.Remove(traceCoord[k, j]);
                                            Destroy(traceCoord[k, j]);
                                        }
                                        traceCoord[k, j] = g;
                                        traces.Add(g);
                                        mutableMovableCoord[k, j].gameObject.SetActive(false);
                                    }
                                    mutableMovableCoord[k, j] = null;
                                }
                                if (CheckTileFlag(map.mapCoord[k, j], TileFlag.LeftWall) ||
                                    (k >= 1 && mutableMovableCoord[k - 1, j] != null && mutableMovableCoord[k - 1, j] is Iron))
                                {
                                    if (mutableMovableCoord[i, j] is Ball)
                                    {
                                        ballX = k + 1;
                                        ballY = j + 1;
                                    }
                                    if (!isSimulation)
                                    {
                                        mutableMovableCoord[i, j].transform.localPosition = new Vector3(k + 1, j + 1, 0f);
                                    }
                                    mutableMovableCoord[k, j] = mutableMovableCoord[i, j];
                                    if (k != i)
                                        mutableMovableCoord[i, j] = null;
                                    break;
                                }
                                if (CheckTileFlag(map.mapCoord[k, j], TileFlag.LeftShutter) && mutableMovableCoord[k - 1, j] == null && k >= 1)
                                {
                                    if (mutableMovableCoord[i, j] is Ball)
                                    {
                                        map.mapCoord[k, j] -= (int)TileFlag.LeftShutter / 2;
                                        map.mapCoord[k - 1, j] -= (int)TileFlag.RightShutter / 2;
                                        if (!isSimulation)
                                        {
                                            tilemap.SetTile(new Vector3Int(k, j, 0), tiles[map.mapCoord[k, j] % 81]);
                                            tilemap.SetTile(new Vector3Int(k - 1, j, 0), tiles[map.mapCoord[k - 1, j] % 81]);
                                        }
                                    }
                                }
                                // 현재 k에서 멈추지 않는 경우 잔상 생성
                                if (!isSimulation)
                                {
                                    if (mutableMovableCoord[i, j] is Ball)
                                    {
                                        GameObject g = Instantiate(ballTracePrefabs[2], new Vector3(), Quaternion.identity, movableAndFixedGameObjects.transform);
                                        g.transform.localPosition = new Vector3(k + 1, j + 1, 0f);
                                        if (traceCoord[k, j] != null)
                                        {
                                            traces.Remove(traceCoord[k, j]);
                                            Destroy(traceCoord[k, j]);
                                        }
                                        traceCoord[k, j] = g;
                                        traces.Add(g);
                                    }
                                    else if (mutableMovableCoord[i, j] is Iron)
                                    {
                                        GameObject g = Instantiate(ironTracePrefabs[2], new Vector3(), Quaternion.identity, movableAndFixedGameObjects.transform);
                                        g.transform.localPosition = new Vector3(k + 1, j + 1, 0f);
                                        if (traceCoord[k, j] != null && !traceCoord[k, j].CompareTag("Flag"))
                                        {
                                            traces.Remove(traceCoord[k, j]);
                                            Destroy(traceCoord[k, j]);
                                        }
                                        traceCoord[k, j] = g;
                                        traces.Add(g);
                                    }
                                }
                            }
#endregion
                        }
                    }
                }
                ActionHistory += "a";
                break;
            case GameManager.GravityDirection.Right:
                for (int i = SizeX - 1; i >= 0; i--)
                {
                    for (int j = SizeY - 1; j >= 0; j--)
                    {
                        if (mutableMovableCoord[i, j] != null)
                        {
                            // i++
#region Right
                            for (int k = i; k <= SizeX; k++)
                            {
                                if (k == SizeX)
                                {
                                    if (ExitX != k + 1 || ExitY != j + 1)
                                    {
                                        Debug.LogError("Gravity invalid: wrong exit at (" + (k + 1) + ", " + (j + 1) + ")");
                                        break;
                                    }
                                    if (mutableMovableCoord[i, j] is Ball)
                                    {
                                        flag = Flag.Escaped;
                                        ballX = ExitX;
                                        ballY = ExitY;
                                        if (!isSimulation)
                                        {
                                            Debug.Log("Stage clear!");
                                            // TODO: (k + 1, j + 1)를 공이 지나 탈출하는 애니메이션 재생
                                            // 탈출하는 순간을 느리게 보여줘도 될 듯?
                                            mutableMovableCoord[i, j].gameObject.SetActive(false);
                                        }
                                        mutableMovableCoord[i, j] = null;
                                        break;
                                    }
                                    else if (mutableMovableCoord[i, j] is Iron)
                                    {
                                        if (!isSimulation)
                                        {
                                            // TODO: (k + 1, j + 1)를 쇠가 지나 탈출하는 애니메이션 재생
                                            mutableMovableCoord[i, j].gameObject.SetActive(false);
                                        }
                                        mutableMovableCoord[i, j] = null;
                                        break;
                                    }
                                }
                                if (mutableMovableCoord[i, j] is Ball && CheckTileFlag(map.mapCoord[k, j], TileFlag.Fire))
                                {
                                    flag = Flag.Burned;
                                    ballX = k + 1;
                                    ballY = j + 1;
                                    Debug.Log("The ball is burned at (" + ballX + ", " + ballY + ")");
                                    if (!isSimulation)
                                    {
                                        GameObject g = Instantiate(flagBurnedPrefab, new Vector3(), Quaternion.identity, movableAndFixedGameObjects.transform);
                                        g.transform.localPosition = new Vector3(k + 1, j + 1, 0f);
                                        if (traceCoord[k, j] != null)
                                        {
                                            traces.Remove(traceCoord[k, j]);
                                            Destroy(traceCoord[k, j]);
                                        }
                                        traceCoord[k, j] = g;
                                        traces.Add(g);
                                        mutableMovableCoord[i, j].gameObject.SetActive(false);
                                    }
                                    mutableMovableCoord[i, j] = null;
                                    break;
                                }
                                if (mutableMovableCoord[i, j] is Ball && CheckTileFlag(map.mapCoord[k, j], TileFlag.QuitGame))
                                {
                                    flag = Flag.QuitGame;
                                    ballX = k + 1;
                                    ballY = j + 1;
                                    if (!isSimulation)
                                    {
                                        mutableMovableCoord[i, j].gameObject.SetActive(false);
                                    }
                                    mutableMovableCoord[i, j] = null;
                                    //GameManager.gm.QuitGame();
                                    break;
                                }
                                if (mutableMovableCoord[i, j] is Ball && CheckTileFlag(map.mapCoord[k, j], TileFlag.MapEditor))
                                {
                                    flag = Flag.MapEditor;
                                    ballX = k + 1;
                                    ballY = j + 1;
                                    if (!isSimulation)
                                    {
                                        mutableMovableCoord[i, j].gameObject.SetActive(false);
                                    }
                                    mutableMovableCoord[i, j] = null;
                                    //GameManager.gm.MapEditor();
                                    break;
                                }
                                if (mutableMovableCoord[i, j] is Ball && CheckTileFlag(map.mapCoord[k, j], TileFlag.Adventure))
                                {
                                    flag = Flag.Adventure;
                                    ballX = k + 1;
                                    ballY = j + 1;
                                    if (!isSimulation)
                                    {
                                        mutableMovableCoord[i, j].gameObject.SetActive(false);
                                    }
                                    mutableMovableCoord[i, j] = null;
                                    break;
                                }
                                if (mutableMovableCoord[i, j] is Ball && CheckTileFlag(map.mapCoord[k, j], TileFlag.Tutorial))
                                {
                                    flag = Flag.Tutorial;
                                    ballX = k + 1;
                                    ballY = j + 1;
                                    if (!isSimulation)
                                    {
                                        mutableMovableCoord[i, j].gameObject.SetActive(false);
                                    }
                                    mutableMovableCoord[i, j] = null;
                                    break;
                                }
                                if (mutableMovableCoord[i, j] is Ball && CheckTileFlag(map.mapCoord[k, j], TileFlag.Custom))
                                {
                                    flag = Flag.Custom;
                                    ballX = k + 1;
                                    ballY = j + 1;
                                    if (!isSimulation)
                                    {
                                        mutableMovableCoord[i, j].gameObject.SetActive(false);
                                    }
                                    mutableMovableCoord[i, j] = null;
                                    break;
                                }
                                if (mutableMovableCoord[i, j] is Ball && CheckTileFlag(map.mapCoord[k, j], TileFlag.Survival))
                                {
                                    flag = Flag.Survival;
                                    ballX = k + 1;
                                    ballY = j + 1;
                                    if (!isSimulation)
                                    {
                                        mutableMovableCoord[i, j].gameObject.SetActive(false);
                                    }
                                    mutableMovableCoord[i, j] = null;
                                    break;
                                }
                                if (mutableMovableCoord[i, j] is Ball && CheckTileFlag(map.mapCoord[k, j], TileFlag.AdvEasy))
                                {
                                    flag = Flag.AdvEasy;
                                    ballX = k + 1;
                                    ballY = j + 1;
                                    if (!isSimulation)
                                    {
                                        mutableMovableCoord[i, j].gameObject.SetActive(false);
                                    }
                                    mutableMovableCoord[i, j] = null;
                                    break;
                                }
                                if (mutableMovableCoord[i, j] is Ball && CheckTileFlag(map.mapCoord[k, j], TileFlag.AdvNormal))
                                {
                                    flag = Flag.AdvNormal;
                                    ballX = k + 1;
                                    ballY = j + 1;
                                    if (!isSimulation)
                                    {
                                        mutableMovableCoord[i, j].gameObject.SetActive(false);
                                    }
                                    mutableMovableCoord[i, j] = null;
                                    break;
                                }
                                if (mutableMovableCoord[i, j] is Ball && CheckTileFlag(map.mapCoord[k, j], TileFlag.AdvHard))
                                {
                                    flag = Flag.AdvHard;
                                    ballX = k + 1;
                                    ballY = j + 1;
                                    if (!isSimulation)
                                    {
                                        mutableMovableCoord[i, j].gameObject.SetActive(false);
                                    }
                                    mutableMovableCoord[i, j] = null;
                                    break;
                                }
                                if (mutableMovableCoord[i, j] is Ball && CheckTileFlag(map.mapCoord[k, j], TileFlag.AdvInsane))
                                {
                                    flag = Flag.AdvInsane;
                                    ballX = k + 1;
                                    ballY = j + 1;
                                    if (!isSimulation)
                                    {
                                        mutableMovableCoord[i, j].gameObject.SetActive(false);
                                    }
                                    mutableMovableCoord[i, j] = null;
                                    break;
                                }
                                if (mutableMovableCoord[i, j] is Iron && mutableMovableCoord[k, j] != null && mutableMovableCoord[k, j] is Ball)
                                {
                                    flag = Flag.Squashed;
                                    ballX = k + 1;
                                    ballY = j + 1;
                                    Debug.Log("The iron at (" + (i + 1) + ", " + ballY + ") squashes the ball at (" + ballX + ", " + ballY + ")");
                                    if (!isSimulation)
                                    {
                                        GameObject g = Instantiate(flagSquashedPrefab, new Vector3(), Quaternion.identity, movableAndFixedGameObjects.transform);
                                        g.transform.localPosition = new Vector3(k + 1, j + 1, 0f);
                                        if (traceCoord[k, j] != null)
                                        {
                                            traces.Remove(traceCoord[k, j]);
                                            Destroy(traceCoord[k, j]);
                                        }
                                        traceCoord[k, j] = g;
                                        traces.Add(g);
                                        mutableMovableCoord[k, j].gameObject.SetActive(false);
                                    }
                                    mutableMovableCoord[k, j] = null;
                                }
                                if (CheckTileFlag(map.mapCoord[k, j], TileFlag.RightWall) ||
                                    (k <= SizeX - 2 && mutableMovableCoord[k + 1, j] != null && mutableMovableCoord[k + 1, j] is Iron))
                                {
                                    if (mutableMovableCoord[i, j] is Ball)
                                    {
                                        ballX = k + 1;
                                        ballY = j + 1;
                                    }
                                    if (!isSimulation)
                                    {
                                        mutableMovableCoord[i, j].transform.localPosition = new Vector3(k + 1, j + 1, 0f);
                                    }
                                    mutableMovableCoord[k, j] = mutableMovableCoord[i, j];
                                    if (k != i)
                                        mutableMovableCoord[i, j] = null;
                                    break;
                                }
                                if (CheckTileFlag(map.mapCoord[k, j], TileFlag.RightShutter) && mutableMovableCoord[k + 1, j] == null && k <= SizeX - 2)
                                {
                                    if (mutableMovableCoord[i, j] is Ball)
                                    {
                                        map.mapCoord[k, j] -= (int)TileFlag.RightShutter / 2;
                                        map.mapCoord[k + 1, j] -= (int)TileFlag.LeftShutter / 2;
                                        if (!isSimulation)
                                        {
                                            tilemap.SetTile(new Vector3Int(k, j, 0), tiles[map.mapCoord[k, j] % 81]);
                                            tilemap.SetTile(new Vector3Int(k + 1, j, 0), tiles[map.mapCoord[k + 1, j] % 81]);
                                        }
                                    }
                                }
                                // 현재 k에서 멈추지 않는 경우 잔상 생성
                                if (!isSimulation)
                                {
                                    if (mutableMovableCoord[i, j] is Ball)
                                    {
                                        GameObject g = Instantiate(ballTracePrefabs[3], new Vector3(), Quaternion.identity, movableAndFixedGameObjects.transform);
                                        g.transform.localPosition = new Vector3(k + 1, j + 1, 0f);
                                        if (traceCoord[k, j] != null)
                                        {
                                            traces.Remove(traceCoord[k, j]);
                                            Destroy(traceCoord[k, j]);
                                        }
                                        traceCoord[k, j] = g;
                                        traces.Add(g);
                                    }
                                    else if (mutableMovableCoord[i, j] is Iron)
                                    {
                                        GameObject g = Instantiate(ironTracePrefabs[3], new Vector3(), Quaternion.identity, movableAndFixedGameObjects.transform);
                                        g.transform.localPosition = new Vector3(k + 1, j + 1, 0f);
                                        if (traceCoord[k, j] != null && !traceCoord[k, j].CompareTag("Flag"))
                                        {
                                            traces.Remove(traceCoord[k, j]);
                                            Destroy(traceCoord[k, j]);
                                        }
                                        traceCoord[k, j] = g;
                                        traces.Add(g);
                                    }
                                }
                            }
#endregion
                        }
                    }
                }
                ActionHistory += "d";
                break;
        }
        if (ballX == -1 && ballY == -1)
        {
            Debug.LogError("Gravity invalid: ball position");
        }
        return mutableMovableCoord;
    }

    private bool CheckTileFlag(int tile, TileFlag flag)
    {
        if ((int)flag % 2 == 0)
        {
            int div = (int)flag / 2;
            return tile % (3 * div) / div == 2;
        }
        return tile % (3  * (int)flag) / (int)flag == 1;
    }

    private void PrintMapCoord()
    {
        if (!IsReady) return;
        string s = "";
        for (int j = map.sizeY - 1; j >= 0; j--)
        {
            for (int i = 0; i < map.sizeX; i++)
            {
                s += map.mapCoord[i, j] + "\t";
            }
            s += "\n";
        }
        Debug.Log(s);
    }

    private Vector3Int RotatedVector3Int(Vector3Int originalVector, bool isWall = false, bool isOriginalHorizontal = false)
    {
        if (SizeX <= 0 || SizeY <= 0) return new Vector3Int(originalVector.x, originalVector.y, 0);

        int offsetVertical = -1;
        int offsetHorizontal = -1;
        if (isWall)
        {
            if (isOriginalHorizontal)
            {
                offsetHorizontal = 0;
                offsetVertical = -1;
            }
            else
            {
                offsetHorizontal = -1;
                offsetVertical = 0;
            }
        }

        switch (Rotation)
        {
            case RotationStatus.Original:
                return new Vector3Int(originalVector.x, originalVector.y, 0);
            case RotationStatus.Clockwise90:
                return new Vector3Int(originalVector.y, _originalSizeX + offsetVertical - originalVector.x, 0);
            case RotationStatus.Clockwise180:
                return new Vector3Int(_originalSizeX + offsetVertical - originalVector.x, _originalSizeY + offsetHorizontal - originalVector.y, 0);
            case RotationStatus.Clockwise270:
                return new Vector3Int(_originalSizeY + offsetHorizontal - originalVector.y, originalVector.x, 0);
            case RotationStatus.UpsideDown:
                return new Vector3Int(originalVector.x, _originalSizeY + offsetHorizontal - originalVector.y, 0);
            case RotationStatus.UpsideDown90:
                return new Vector3Int(_originalSizeY + offsetHorizontal - originalVector.y, _originalSizeX + offsetVertical - originalVector.x, 0);
            case RotationStatus.UpsideDown180:
                return new Vector3Int(_originalSizeX + offsetVertical - originalVector.x, originalVector.y, 0);
            case RotationStatus.UpsideDown270:
                return new Vector3Int(originalVector.y, originalVector.x, 0);
            default:    // Unused
                return new Vector3Int(originalVector.x, originalVector.y, 0);
        }
    }

    private Vector3 RotatedVector3(Vector3 originalVector, bool isWall = false, bool isOriginalHorizontal = false)
    {
        if (SizeX <= 0 || SizeY <= 0) return new Vector3(originalVector.x, originalVector.y, 0f);

        int offsetVertical = 1;
        int offsetHorizontal = 1;
        if (isWall)
        {
            if (isOriginalHorizontal)
            {
                offsetHorizontal = 0;
                offsetVertical = -1;
            }
            else
            {
                offsetHorizontal = -1;
                offsetVertical = 0;
            }
        }

        switch (Rotation)
        {
            case RotationStatus.Original:
                return new Vector3(originalVector.x, originalVector.y, 0f);
            case RotationStatus.Clockwise90:
                return new Vector3(originalVector.y, _originalSizeX + offsetVertical - originalVector.x, 0f);
            case RotationStatus.Clockwise180:
                return new Vector3(_originalSizeX + offsetVertical - originalVector.x, _originalSizeY + offsetHorizontal - originalVector.y, 0f);
            case RotationStatus.Clockwise270:
                return new Vector3(_originalSizeY + offsetHorizontal - originalVector.y, originalVector.x, 0f);
            case RotationStatus.UpsideDown:
                return new Vector3(originalVector.x, _originalSizeY + offsetHorizontal - originalVector.y, 0f);
            case RotationStatus.UpsideDown90:
                return new Vector3(_originalSizeY + offsetHorizontal - originalVector.y, _originalSizeX + offsetVertical - originalVector.x, 0f);
            case RotationStatus.UpsideDown180:
                return new Vector3(_originalSizeX + offsetVertical - originalVector.x, originalVector.y, 0f);
            case RotationStatus.UpsideDown270:
                return new Vector3(originalVector.y, originalVector.x, 0f);
            default:    // Unused
                return new Vector3(originalVector.x, originalVector.y, 0f);
        }
    }

    private int RotatedX(int originalX, int originalY, bool isWall = false, bool isOriginalHorizontal = false, bool isExit = false)
    {
        

        if (SizeX <= 0 || SizeY <= 0) return originalX;

        int offsetVertical = -1;
        int offsetHorizontal = -1;
        if (isWall)
        {
            if (isOriginalHorizontal)
            {
                if (isExit)
                {
                    offsetHorizontal = 0;
                    offsetVertical = 1;
                }
                else
                {
                    offsetHorizontal = 0;
                    offsetVertical = -1;
                }
            }
            else
            {
                if (isExit)
                {
                    offsetHorizontal = 1;
                    offsetVertical = 0;
                }
                else
                {
                    offsetHorizontal = -1;
                    offsetVertical = 0;
                }
            }
        }

        switch (Rotation)
        {
            case RotationStatus.Original:
                return originalX;
            case RotationStatus.Clockwise90:
                return originalY;
            case RotationStatus.Clockwise180:
                return _originalSizeX + offsetVertical - originalX;
            case RotationStatus.Clockwise270:
                return _originalSizeY + offsetHorizontal - originalY;
            case RotationStatus.UpsideDown:
                return originalX;
            case RotationStatus.UpsideDown90:
                return _originalSizeY + offsetHorizontal - originalY;
            case RotationStatus.UpsideDown180:
                return _originalSizeX + offsetVertical - originalX;
            case RotationStatus.UpsideDown270:
                return originalY;
            default:    // Unused
                return originalX;
        }
    }

    private int RotatedY(int originalX, int originalY, bool isWall = false, bool isOriginalHorizontal = false, bool isExit = false)
    {
        if (SizeX <= 0 || SizeY <= 0) return originalY;

        int offsetVertical = -1;
        int offsetHorizontal = -1;
        if (isWall)
        {
            if (isOriginalHorizontal)
            {
                if (isExit)
                {
                    offsetHorizontal = 0;
                    offsetVertical = 1;
                }
                else
                {
                    offsetHorizontal = 0;
                    offsetVertical = -1;
                }
            }
            else
            {
                if (isExit)
                {
                    offsetHorizontal = 1;
                    offsetVertical = 0;
                }
                else
                {
                    offsetHorizontal = -1;
                    offsetVertical = 0;
                }
            }
        }

        switch (Rotation)
        {
            case RotationStatus.Original:
                return originalY;
            case RotationStatus.Clockwise90:
                return _originalSizeX + offsetVertical - originalX;
            case RotationStatus.Clockwise180:
                return _originalSizeY + offsetHorizontal - originalY;
            case RotationStatus.Clockwise270:
                return originalX;
            case RotationStatus.UpsideDown:
                return _originalSizeY + offsetHorizontal - originalY;
            case RotationStatus.UpsideDown90:
                return _originalSizeX + offsetVertical - originalX;
            case RotationStatus.UpsideDown180:
                return originalY;
            case RotationStatus.UpsideDown270:
                return originalX;
            default:    // Unused
                return originalY;
        }
    }

    private bool RotatedHasTransposed()
    {
        switch (Rotation)
        {
            case RotationStatus.Original:
            case RotationStatus.Clockwise180:
            case RotationStatus.UpsideDown:
            case RotationStatus.UpsideDown180:
                return false;
            case RotationStatus.Clockwise90:
            case RotationStatus.Clockwise270:
            case RotationStatus.UpsideDown90:
            case RotationStatus.UpsideDown270:
                return true;
            default:    // Unused
                return false;
        }
    }

    private int RotatedSizeX()
    {
        if (_originalSizeX <= 0 || _originalSizeY <= 0) return 0;
        switch (Rotation)
        {
            case RotationStatus.Original:
            case RotationStatus.Clockwise180:
            case RotationStatus.UpsideDown:
            case RotationStatus.UpsideDown180:
                return _originalSizeX;
            case RotationStatus.Clockwise90:
            case RotationStatus.Clockwise270:
            case RotationStatus.UpsideDown90:
            case RotationStatus.UpsideDown270:
                return _originalSizeY;
            default:    // Unused
                return _originalSizeX;
        }
    }

    private int RotatedSizeY()
    {
        if (_originalSizeX <= 0 || _originalSizeY <= 0) return 0;
        switch (Rotation)
        {
            case RotationStatus.Original:
            case RotationStatus.Clockwise180:
            case RotationStatus.UpsideDown:
            case RotationStatus.UpsideDown180:
                return _originalSizeY;
            case RotationStatus.Clockwise90:
            case RotationStatus.Clockwise270:
            case RotationStatus.UpsideDown90:
            case RotationStatus.UpsideDown270:
                return _originalSizeX;
            default:    // Unused
                return _originalSizeY;
        }
    }

    private string RotatedSolution(string originalSolution)
    {
        if (originalSolution == null || originalSolution.Equals("")) return "";
        string newSolution = originalSolution.ToUpperInvariant();
        switch (Rotation)
        {
            case RotationStatus.Original:
                newSolution = newSolution.Replace('W', 'w');
                newSolution = newSolution.Replace('A', 'a');
                newSolution = newSolution.Replace('S', 's');
                newSolution = newSolution.Replace('D', 'd');
                return newSolution;
            case RotationStatus.Clockwise90:
                newSolution = newSolution.Replace('W', 'd');
                newSolution = newSolution.Replace('A', 'w');
                newSolution = newSolution.Replace('S', 'a');
                newSolution = newSolution.Replace('D', 's');
                return newSolution;
            case RotationStatus.Clockwise180:
                newSolution = newSolution.Replace('W', 's');
                newSolution = newSolution.Replace('A', 'd');
                newSolution = newSolution.Replace('S', 'w');
                newSolution = newSolution.Replace('D', 'a');
                return newSolution;
            case RotationStatus.Clockwise270:
                newSolution = newSolution.Replace('W', 'a');
                newSolution = newSolution.Replace('A', 's');
                newSolution = newSolution.Replace('S', 'd');
                newSolution = newSolution.Replace('D', 'w');
                return newSolution;
            case RotationStatus.UpsideDown:
                newSolution = newSolution.Replace('W', 's');
                newSolution = newSolution.Replace('A', 'a');
                newSolution = newSolution.Replace('S', 'w');
                newSolution = newSolution.Replace('D', 'd');
                return newSolution;
            case RotationStatus.UpsideDown90:
                newSolution = newSolution.Replace('W', 'a');
                newSolution = newSolution.Replace('A', 'w');
                newSolution = newSolution.Replace('S', 'd');
                newSolution = newSolution.Replace('D', 's');
                return newSolution;
            case RotationStatus.UpsideDown180:
                newSolution = newSolution.Replace('W', 'w');
                newSolution = newSolution.Replace('A', 'd');
                newSolution = newSolution.Replace('S', 's');
                newSolution = newSolution.Replace('D', 'a');
                return newSolution;
            case RotationStatus.UpsideDown270:
                newSolution = newSolution.Replace('W', 'd');
                newSolution = newSolution.Replace('A', 's');
                newSolution = newSolution.Replace('S', 'a');
                newSolution = newSolution.Replace('D', 'w');
                return newSolution;
            default:    // Unused
                return originalSolution;
        }
    }

    public class Map
    {
        public int sizeX;
        public int sizeY;
        public int[,] mapCoord;
        public int exitX;
        public int exitY;

        public Map(int sizeX, int sizeY, int[,] mapCoord, int exitX, int exitY)
        {
            this.sizeX = sizeX;
            this.sizeY = sizeY;
            this.mapCoord = mapCoord;
            this.exitX = exitX;
            this.exitY = exitY;
        }
    }
}
