using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour
{
    public enum Flag { Continued = 0, Escaped = 1, Burned = 2, Squashed = 3, TimeOver = 4, QuitGame = 5, MapEditor = 6,
        Adventure = 7, Tutorial = 8, Custom = 9, Survival = 10, AdvEasy = 11, AdvNormal = 12, AdvHard = 13, AdvInsane = 14 }
    public enum TileFlag { RightWall = 1, LeftWall = 2, DownWall = 4, UpWall = 8, Fire = 16, QuitGame = 32, MapEditor = 64, 
        Adventure = 128, Tutorial = 256, Custom = 512, Survival = 1024, AdvEasy = 2048, AdvNormal = 4096, AdvHard = 8192, AdvInsane = 16384 }

    public const int MIN_SIZE_X = 2;
    public const int MIN_SIZE_Y = 2;
    public const int MAX_SIZE_X = 9;
    public const int MAX_SIZE_Y = 9;

    [HideInInspector]
    public List<Movable> movables;
    [HideInInspector]
    public List<FixedObject> fixedObjects;
    [HideInInspector]
    public List<GameObject> traces;

    public Map map;
    public Movable[,] initialMovableCoord;
    public Movable[,] currentMovableCoord;

    public GameObject movableAndFixedGameObjects;
    public Camera mainCamera;

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

    public delegate void AfterGravity(Flag flag);
    public AfterGravity afterGravity;

    private float _timeLimit;

    public int SizeX
    {
        get;
        private set;
    }
    public int SizeY
    {
        get;
        private set;
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
    public bool IsTimeActivated
    {
        get;
        private set;
    } = false;

    public bool IsReady
    {
        get;
        private set;
    } = false;

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

    void Update()
    {
        if (IsReady && IsTimeActivated && IsTimePassing && RemainingTime > 0f && !HasCleared)
        {
            RemainingTime -= Time.deltaTime;
            if (RemainingTime <= 0f)
            {
                if (afterGravity.GetInvocationList().Length > 0)
                    afterGravity(Flag.TimeOver);
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
        RemainingTime = 0f;
        tilemap.ClearAllTiles();
    }

    public void Initialize(int sizeX, int sizeY, List<WallInfo> walls, List<ObjectInfo> objects, string solution = "",
        float timeLimit = 0f, bool isValidation = false)
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

        this.SizeX = sizeX;
        this.SizeY = sizeY;
        ExitX = 0;
        ExitY = 0;
        int [,] mapCoord = new int[sizeX, sizeY];
        initialMovableCoord = new Movable[sizeX, sizeY];

        tilemap.ClearAllTiles();

        bool[,] horizontalWalls = new bool[sizeX, sizeY + 1];
        bool[,] verticalWalls = new bool[sizeX + 1, sizeY];

        for (int i = 0; i < sizeX; i++)
        {
            horizontalWalls[i, 0] = true;
            horizontalWalls[i, sizeY] = true;
            for (int j = 1; j < sizeY; j++)
            {
                horizontalWalls[i, j] = false;
            }
        }
        for (int j = 0; j < sizeY; j++)
        {
            verticalWalls[0, j] = true;
            verticalWalls[sizeX, j] = true;
            for (int i = 1; i < sizeX; i++)
            {
                verticalWalls[i, j] = false;
            }
        }

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
                    horizontalWalls[wi.x - 1, wi.y] = false;
                    ExitX = wi.x;
                    if (wi.y == 0) ExitY = 0;
                    else ExitY = sizeY + 1;
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
                    verticalWalls[wi.x, wi.y - 1] = false;
                    if (wi.x == 0) ExitX = 0;
                    else ExitX = sizeX + 1;
                    ExitY = wi.y;
                    hasExit = true;
                    break;
                case WallInfo.Type.Horizontal:
                    if (wi.x < 1 || wi.x > sizeX || wi.y < 1 || wi.y > sizeY - 1)
                    {
                        Debug.LogError("Map invalid: wall position at (" + wi.x + ", " + wi.y + ")");
                        return;
                    }
                    horizontalWalls[wi.x - 1, wi.y] = true;
                    break;
                case WallInfo.Type.Vertical:
                    if (wi.x < 1 || wi.x > sizeX - 1 || wi.y < 1 || wi.y > sizeY)
                    {
                        Debug.LogError("Map invalid: wall position at (" + wi.x + ", " + wi.y + ")");
                        return;
                    }
                    verticalWalls[wi.x, wi.y - 1] = true;
                    break;
            }
        }

        if (!hasExit && (SceneManager.GetActiveScene().name != "Editor" || isValidation))
        {
            Debug.LogError("Map invalid: no exit");
            return;
        }

        for (int i = 0; i <= sizeX + 1; i++)
        {
            for (int j = 0; j <= sizeY + 1; j++)
            {
                if (i == 0)
                {
                    if (ExitX == i && ExitY == j) tilemap.SetTile(new Vector3Int(i - 1, j - 1, 0), tiles[24]);
                    else tilemap.SetTile(new Vector3Int(i - 1, j - 1, 0), tiles[16]);
                }
                else if (i == sizeX + 1)
                {
                    if (ExitX == i && ExitY == j) tilemap.SetTile(new Vector3Int(i - 1, j - 1, 0), tiles[25]);
                    else tilemap.SetTile(new Vector3Int(i - 1, j - 1, 0), tiles[17]);
                }
                else if (j == sizeY + 1)
                {
                    if (ExitX == i && ExitY == j) tilemap.SetTile(new Vector3Int(i - 1, j - 1, 0), tiles[26]);
                    else tilemap.SetTile(new Vector3Int(i - 1, j - 1, 0), tiles[18]);
                }
                else if (j == 0)
                {
                    if (ExitX == i && ExitY == j) tilemap.SetTile(new Vector3Int(i - 1, j - 1, 0), tiles[27]);
                    else tilemap.SetTile(new Vector3Int(i - 1, j - 1, 0), tiles[19]);
                }
                else
                {
                    if (horizontalWalls[i - 1, j - 1]) mapCoord[i - 1, j - 1] += (int)TileFlag.DownWall;  // 8
                    if (horizontalWalls[i - 1, j]) mapCoord[i - 1, j - 1] += (int)TileFlag.UpWall;    // 4
                    if (verticalWalls[i - 1, j - 1]) mapCoord[i - 1, j - 1] += (int)TileFlag.LeftWall;  // 2
                    if (verticalWalls[i, j - 1]) mapCoord[i - 1, j - 1] += (int)TileFlag.RightWall;     // 1
                    tilemap.SetTile(new Vector3Int(i - 1, j - 1, 0), tiles[mapCoord[i - 1, j - 1] % 16]);
                }
            }
        }
        tilemap.SetTile(new Vector3Int(-1, -1, 0), tiles[20]);
        tilemap.SetTile(new Vector3Int(-1, sizeY, 0), tiles[21]);
        tilemap.SetTile(new Vector3Int(sizeX, -1, 0), tiles[22]);
        tilemap.SetTile(new Vector3Int(sizeX, sizeY, 0), tiles[23]);

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

                if (x < 1 || x > sizeX || y < 1 || y > sizeY)
                {
                    Debug.LogError("Map invalid: object position at (" + x + ", " + y + ")");
                    return;
                }
                if (initialMovableCoord[x - 1, y - 1] != null || mapCoord[x - 1, y - 1] >= 16)
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

                if (x < 1 || x > sizeX || y < 1 || y > sizeY)
                {
                    Debug.LogError("Map invalid: object position at (" + x + ", " + y + ")");
                    return;
                }
                if (initialMovableCoord[x - 1, y - 1] != null || mapCoord[x - 1, y - 1] >= 16)
                {
                    Debug.LogError("Map invalid: objects overlapped at (" + x + ", " + y + ")");
                    return;
                }

                fixedObjects.Add(f);

                switch (f.type)
                {
                    case FixedObject.Type.Fire:
                        mapCoord[x - 1, y - 1] += (int)TileFlag.Fire;       // 16
                        break;
                    case FixedObject.Type.QuitGame:
                        mapCoord[x - 1, y - 1] += (int)TileFlag.QuitGame;   // 32
                        break;
                    case FixedObject.Type.MapEditor:
                        mapCoord[x - 1, y - 1] += (int)TileFlag.MapEditor;  // 64
                        break;
                    case FixedObject.Type.Adventure:
                        mapCoord[x - 1, y - 1] += (int)TileFlag.Adventure;  // 128
                        break;
                    case FixedObject.Type.Tutorial:
                        mapCoord[x - 1, y - 1] += (int)TileFlag.Tutorial;   // 256
                        break;
                    case FixedObject.Type.Custom:
                        mapCoord[x - 1, y - 1] += (int)TileFlag.Custom;     // 512
                        break;
                    case FixedObject.Type.Survival:
                        mapCoord[x - 1, y - 1] += (int)TileFlag.Survival;   // 1024
                        break;
                    case FixedObject.Type.AdvEasy:
                        mapCoord[x - 1, y - 1] += (int)TileFlag.AdvEasy;    // 2048
                        break;
                    case FixedObject.Type.AdvNormal:
                        mapCoord[x - 1, y - 1] += (int)TileFlag.AdvNormal;  // 4096
                        break;
                    case FixedObject.Type.AdvHard:
                        mapCoord[x - 1, y - 1] += (int)TileFlag.AdvHard;    // 8192
                        break;
                    case FixedObject.Type.AdvInsane:
                        mapCoord[x - 1, y - 1] += (int)TileFlag.AdvInsane;  // 16384
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
            if (initialMovableCoord[oi.x - 1, oi.y - 1] != null || mapCoord[oi.x - 1, oi.y - 1] >= 16)
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
                    g.transform.localPosition = new Vector3(oi.x, oi.y, 0f);
                    movables.Add(g.GetComponent<Movable>());
                    initialMovableCoord[oi.x - 1, oi.y - 1] = g.GetComponent<Movable>();
                    hasBall = true;
                    break;
                case ObjectInfo.Type.Iron:
                    g = Instantiate(ironPrefab, new Vector3(), Quaternion.identity, movableAndFixedGameObjects.transform);
                    g.transform.localPosition = new Vector3(oi.x, oi.y, 0f);
                    movables.Add(g.GetComponent<Movable>());
                    initialMovableCoord[oi.x - 1, oi.y - 1] = g.GetComponent<Movable>();
                    break;
                case ObjectInfo.Type.Fire:
                    g = Instantiate(firePrefab, new Vector3(), Quaternion.identity, movableAndFixedGameObjects.transform);
                    g.transform.localPosition = new Vector3(oi.x, oi.y, 0f);
                    fixedObjects.Add(g.GetComponent<FixedObject>());
                    mapCoord[oi.x - 1, oi.y - 1] += (int)TileFlag.Fire;         // 16
                    break;
                /*
                // 이 친구들은 맵 에디터에서 설치하거나 맵 파일에 기록되거나 자동으로 생성될 수 없음
                case ObjectInfo.Type.QuitGame:
                    mapCoord[oi.x - 1, oi.y - 1] += (int)TileFlag.QuitGame;     // 32
                    break;
                case ObjectInfo.Type.MapEditor:
                    mapCoord[oi.x - 1, oi.y - 1] += (int)TileFlag.MapEditor;    // 64
                    break;
                */
            }
        }

        if (!hasBall && (SceneManager.GetActiveScene().name != "Editor" || isValidation))
        {
            Debug.LogError("Map invalid: no ball");
            return;
        }

        map = new Map(sizeX, sizeY, mapCoord, ExitX, ExitY);
        if ((SceneManager.GetActiveScene().name != "Editor" || isValidation) && !Simulate(map, initialMovableCoord, solution))
        {
            Debug.LogError("Map invalid: impossible to clear");
            return;
        }

        // TODO: 좌표계 회전 및 상하 반전 적용

        currentMovableCoord = (Movable[,])initialMovableCoord.Clone();

        mainCamera.transform.position = new Vector3((sizeX + 1) / 2f, (sizeY + 1) / 2f, -10f);
        mainCamera.orthographicSize = Mathf.Max(sizeX, sizeY) / 2f + 1.5f;

        gravityBall.anchoredPosition = new Vector3(0f, 0f);

        TimeLimit = Mathf.Max(3f, timeLimit);
        ActionHistory = "";
        IsReady = true;
        HasCleared = false;
        HasDied = false;
        IsTimeActivated = false;
        IsTimePassing = false;
        RemainingTime = 0f;
        //PrintMapCoord();
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
        Debug.Log("Remaining time: " + RemainingTime);
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

        if (traces != null)
        {
            foreach (GameObject g in traces)
            {
                Destroy(g);
            }
        }
        traces = new List<GameObject>();

        currentMovableCoord = (Movable[,])initialMovableCoord.Clone();

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
        return tile % (2 * (int)flag) / (int)flag == 1;
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
