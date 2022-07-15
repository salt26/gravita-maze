using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

public class GameManager : MonoBehaviour
{

    public static GameManager gm;
    public static MapManager mm = null;
    public static PlayManager pm = null;

    public enum GravityDirection { Up, Down, Left, Right }

    [HideInInspector]
    public bool canPlay = true;

    private enum AdventureLevel { NULL = 0, Easy = 1, Normal = 2, Hard = 3, Insane = 4 }

    [SerializeField]
    private AdventureLevel adventureLevel;
    private List<string> mapList;
    private int playingMapIndex = 0;

    public bool HasClearedAll
    {
        get;
        private set;
    } = false;

    private void Awake()
    {
        if (gm != null && gm != this)
        {
            Destroy(gameObject);
            return;
        }
        gm = this;
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        Initialize();

#if UNITY_ANDROID && !UNITY_EDITOR
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        }
#endif
    }

    // Update is called once per frame
    void Update()
    {
        // 입력 담당
        if (mm is null || !mm.IsReady) return;

        if (canPlay)
        {
            if (Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.S))
            {
                mm.ManipulateGravityDown();
            }
            else if (Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.W))
            {
                mm.ManipulateGravityUp();
            }
            else if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.A))
            {
                mm.ManipulateGravityLeft();
            }
            else if (Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.D))
            {
                mm.ManipulateGravityRight();
            }
            else if (Input.GetKeyUp(KeyCode.Space))
            {
                mm.Restart();
            }
        }
    }

    void Initialize()
    {
        mapList = null;
        playingMapIndex = -1;

        // TODO: 씬 바뀔 때마다 적절한 레벨 선택하고 MapManager 찾아서 맵 로드해야 함
        if (SceneManager.GetActiveScene().name.Equals("Main"))
        {
            StartCoroutine(InitializeMain());
        }
        else if (SceneManager.GetActiveScene().name.Equals("Editor"))
        {
            StartCoroutine(InitializeEditor());
        }
        else if (SceneManager.GetActiveScene().name.Equals("Mode"))
        {
            StartCoroutine(InitializeMode());
        }
        else if (SceneManager.GetActiveScene().name.Equals("AdventureLevel"))
        {
            StartCoroutine(InitializeAdventureLevel());
        }
        else if (SceneManager.GetActiveScene().name.Equals("Tutorial"))
        {
            StartCoroutine(InitializeTutorial());
        }
        else if (SceneManager.GetActiveScene().name.Equals("Adventure"))
        {
            StartCoroutine(InitializeAdventure());
        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void LoadEditor()
    {
        StartCoroutine(SceneLoading("Editor"));
    }

    public void LoadMain()
    {
        StartCoroutine(SceneLoading("Main"));
    }

    public void LoadMode()
    {
        StartCoroutine(SceneLoading("Mode"));
    }

    public void LoadAdventureLevel()
    {
        StartCoroutine(SceneLoading("AdventureLevel"));
    }

    public void LoadTutorial()
    {
        StartCoroutine(SceneLoading("Tutorial"));
    }

    public void LoadAdventureEasy()
    {
        adventureLevel = AdventureLevel.Easy;
        StartCoroutine(SceneLoading("Adventure"));
    }

    public void LoadAdventureNormal()
    {
        adventureLevel = AdventureLevel.Normal;
        StartCoroutine(SceneLoading("Adventure"));
    }

    public void LoadAdventureHard()
    {
        adventureLevel = AdventureLevel.Hard;
        StartCoroutine(SceneLoading("Adventure"));
    }

    public void LoadAdventureInsane()
    {
        adventureLevel = AdventureLevel.Insane;
        StartCoroutine(SceneLoading("Adventure"));
    }

    IEnumerator SceneLoading(string sceneName)
    {
        if (mm != null)
        {
            mm.loadingPanel.SetActive(true);
        }
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        while (!operation.isDone)
        {
            yield return null;
        }
        Initialize();
    }

    IEnumerator InitializeMain()
    {
        while (mm == null)
        {
            mm = GameObject.FindGameObjectWithTag("MapManager").GetComponent<MapManager>();
            if (mm == null)
            {
                mm = GameObject.Find("MapManager").GetComponent<MapManager>();
            }
            yield return null;
        }
        List<WallInfo> walls = new List<WallInfo>();

        walls.Add(new WallInfo(WallInfo.Type.Vertical, 5, 3));
        walls.Add(new WallInfo(WallInfo.Type.Vertical, 6, 3));
        walls.Add(new WallInfo(WallInfo.Type.Vertical, 3, 2));
        walls.Add(new WallInfo(WallInfo.Type.Vertical, 4, 2));
        walls.Add(new WallInfo(WallInfo.Type.Vertical, 6, 2));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 5, 6));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 6, 6));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 7, 6));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 1, 5));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 2, 5));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 3, 5));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 4, 5));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 5, 5));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 6, 5));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 7, 5));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 1, 4));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 2, 4));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 3, 4));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 4, 4));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 5, 4));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 6, 4));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 7, 4));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 1, 3));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 2, 3));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 3, 3));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 4, 3));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 5, 3));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 7, 3));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 1, 2));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 2, 2));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 3, 2));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 4, 2));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 5, 2));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 5, 1));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 6, 1));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 7, 1));
        walls.Add(new WallInfo(WallInfo.Type.ExitVertical, 7, 4));

        List<ObjectInfo> objects = new List<ObjectInfo>();

        mm.afterGravity = MainAfterGravity;

        mm.Initialize(7, 7, walls, objects, "d", float.PositiveInfinity);
        mm.TimeActivate();
        canPlay = true;
    }

    IEnumerator InitializeEditor()
    {
        while (mm == null)
        {
            mm = GameObject.FindGameObjectWithTag("MapManager").GetComponent<MapManager>();
            if (mm == null)
            {
                mm = GameObject.Find("MapManager").GetComponent<MapManager>();
            }
            yield return null;
        }
        canPlay = false;
    }

    IEnumerator InitializeMode()
    {
        while (mm == null)
        {
            mm = GameObject.FindGameObjectWithTag("MapManager").GetComponent<MapManager>();
            if (mm == null)
            {
                mm = GameObject.Find("MapManager").GetComponent<MapManager>();
            }
            yield return null;
        }
        List<WallInfo> walls = new List<WallInfo>();

        walls.Add(new WallInfo(WallInfo.Type.Vertical, 1, 6));
        walls.Add(new WallInfo(WallInfo.Type.Vertical, 3, 4));
        walls.Add(new WallInfo(WallInfo.Type.Vertical, 1, 2));
        walls.Add(new WallInfo(WallInfo.Type.Vertical, 2, 5));
        walls.Add(new WallInfo(WallInfo.Type.Vertical, 9, 1));  // TODO 나중에 해금
        walls.Add(new WallInfo(WallInfo.Type.Vertical, 9, 3));  // TODO 나중에 해금
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 1, 7));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 2, 7));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 3, 7));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 4, 7));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 5, 7));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 6, 7));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 7, 7));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 8, 7));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 9, 7));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 10, 7));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 2, 5));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 2, 6));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 3, 6));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 4, 6));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 5, 6));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 6, 6));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 7, 6));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 8, 6));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 9, 6));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 10, 6));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 3, 5));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 4, 5));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 5, 5));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 6, 5));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 7, 5));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 8, 5));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 9, 5));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 10, 5));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 1, 4));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 2, 3));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 2, 2));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 3, 2));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 4, 2));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 4, 4));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 4, 3));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 5, 3));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 6, 3));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 7, 3));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 8, 3));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 9, 3));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 10, 3));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 5, 4));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 6, 4));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 7, 4));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 8, 4));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 9, 4));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 10, 4));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 5, 2));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 6, 2));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 7, 2));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 8, 2));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 9, 2));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 10, 2));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 2, 1));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 3, 1));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 4, 1));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 5, 1));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 6, 1));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 7, 1));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 8, 1));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 9, 1));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 10, 1));
        walls.Add(new WallInfo(WallInfo.Type.ExitVertical, 0, 4));

        List<ObjectInfo> objects = new List<ObjectInfo>();

        mm.afterGravity = ModeAfterGravity;

        mm.Initialize(10, 8, walls, objects, "a", float.PositiveInfinity);
        mm.TimeActivate();
        canPlay = true;
    }

    IEnumerator InitializeAdventureLevel()
    {
        while (mm == null)
        {
            mm = GameObject.FindGameObjectWithTag("MapManager").GetComponent<MapManager>();
            if (mm == null)
            {
                mm = GameObject.Find("MapManager").GetComponent<MapManager>();
            }
            yield return null;
        }
        List<WallInfo> walls = new List<WallInfo>();

        walls.Add(new WallInfo(WallInfo.Type.Vertical, 1, 6));
        walls.Add(new WallInfo(WallInfo.Type.Vertical, 3, 4));
        walls.Add(new WallInfo(WallInfo.Type.Vertical, 1, 2));
        walls.Add(new WallInfo(WallInfo.Type.Vertical, 2, 5));
        walls.Add(new WallInfo(WallInfo.Type.Vertical, 3, 8));
        //walls.Add(new WallInfo(WallInfo.Type.Vertical, 7, 1));  // Insane
        //walls.Add(new WallInfo(WallInfo.Type.Vertical, 7, 3));  // Hard
        //walls.Add(new WallInfo(WallInfo.Type.Vertical, 7, 5));  // Normal
        //walls.Add(new WallInfo(WallInfo.Type.Vertical, 7, 7));  // Easy
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 1, 7));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 2, 7));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 3, 7));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 4, 7));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 5, 7));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 6, 7));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 7, 7));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 8, 7));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 2, 5));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 2, 6));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 3, 6));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 4, 6));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 5, 6));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 6, 6));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 7, 6));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 8, 6));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 3, 5));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 4, 5));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 5, 5));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 6, 5));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 7, 5));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 8, 5));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 1, 4));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 2, 3));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 2, 2));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 3, 2));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 4, 2));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 4, 4));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 4, 3));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 5, 3));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 6, 3));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 7, 3));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 8, 3));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 5, 4));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 6, 4));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 7, 4));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 8, 4));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 5, 2));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 6, 2));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 7, 2));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 8, 2));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 2, 1));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 3, 1));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 4, 1));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 5, 1));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 6, 1));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 7, 1));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 8, 1));
        walls.Add(new WallInfo(WallInfo.Type.ExitVertical, 0, 4));

        List<ObjectInfo> objects = new List<ObjectInfo>();

        mm.afterGravity = AdventureLevelAfterGravity;

        mm.Initialize(8, 8, walls, objects, "a", float.PositiveInfinity);
        mm.TimeActivate();
        canPlay = true;
    }

    IEnumerator InitializeTutorial()
    {
        while (mm == null)
        {
            mm = GameObject.FindGameObjectWithTag("MapManager").GetComponent<MapManager>();
            if (mm == null)
            {
                mm = GameObject.Find("MapManager").GetComponent<MapManager>();
            }
            yield return null;
        }

        while (pm == null)
        {
            pm = GameObject.FindGameObjectWithTag("PlayManager").GetComponent<PlayManager>();
            if (pm != null) break;
            else
            {
                pm = GameObject.Find("PlayManager").GetComponent<PlayManager>();
            }
            yield return null;
        }

        pm.Initialize(PlayManager.Mode.Tutorial);
        HasClearedAll = false;
        mm.afterGravity = pm.TutorialAfterGravity;

        //mapList = Directory.GetFiles("Assets/PredefinedMaps/Tutorial/", "*.txt").ToList();

        for (int i = 0; i < pm.MapFiles.Count; i++)
        {
            MapManager.OpenFileFlag openFileFlag = mm.InitializeFromText(pm.MapFiles[i].text, out _, out _, out _, out _, out _, out _);
            if (openFileFlag != MapManager.OpenFileFlag.Success)
            {
                continue;
            }
            else
            {
                playingMapIndex = i;
                mm.TimeActivate();
                canPlay = true;
                yield break;
            }
        }
    }
    
    IEnumerator InitializeAdventure()
    {
        while (mm == null)
        {
            mm = GameObject.FindGameObjectWithTag("MapManager").GetComponent<MapManager>();
            if (mm == null)
            {
                mm = GameObject.Find("MapManager").GetComponent<MapManager>();
            }
            yield return null;
        }

        while (pm == null)
        {
            pm = GameObject.FindGameObjectWithTag("PlayManager").GetComponent<PlayManager>();
            if (pm != null) break;
            else
            {
                pm = GameObject.Find("PlayManager").GetComponent<PlayManager>();
            }
            yield return null;
        }
        Debug.Log(adventureLevel);

        switch (adventureLevel)
        {
            case AdventureLevel.Easy:
                pm.Initialize(PlayManager.Mode.AdvEasy, true);
                break;
            case AdventureLevel.Normal:
                pm.Initialize(PlayManager.Mode.AdvNormal, true);
                break;
            case AdventureLevel.Hard:
                pm.Initialize(PlayManager.Mode.AdvHard, true);
                break;
            case AdventureLevel.Insane:
                pm.Initialize(PlayManager.Mode.AdvInsane, true);
                break;
            default:
                Debug.LogError("Play invalid: unknown adventure level");
                yield break;
        }

        HasClearedAll = false;
        mm.afterGravity = pm.PlayAfterGravity;

        for (int i = 0; i < pm.MapFiles.Count; i++)
        {
            MapManager.OpenFileFlag openFileFlag = mm.InitializeFromText(pm.MapFiles[i].text, out _, out _, out _, out _, out _, out _);
            if (openFileFlag != MapManager.OpenFileFlag.Success)
            {
                continue;
            }
            else
            {
                playingMapIndex = i;
                mm.TimeActivate();
                canPlay = true;
                yield break;
            }
        }
    }

    public void MainAfterGravity(MapManager.Flag flag)
    {
        switch (flag)
        {
            case MapManager.Flag.Escaped:
                LoadMode();
                break;
            case MapManager.Flag.MapEditor:
                LoadEditor();
                break;
            case MapManager.Flag.QuitGame:
                QuitGame();
                break;
        }
    }

    public void ModeAfterGravity(MapManager.Flag flag)
    {
        switch (flag)
        {
            case MapManager.Flag.Escaped:
                LoadMain();
                break;
            case MapManager.Flag.Adventure:
                LoadAdventureLevel();
                break;
            case MapManager.Flag.Tutorial:
                LoadTutorial();
                break;
            case MapManager.Flag.Custom:
                // TODO
                break;
            case MapManager.Flag.Survival:
                // TODO
                break;
        }
    }

    public void AdventureLevelAfterGravity(MapManager.Flag flag)
    {
        switch (flag)
        {
            case MapManager.Flag.Escaped:
                LoadMode();
                break;
            case MapManager.Flag.AdvEasy:
                LoadAdventureEasy();
                break;
            case MapManager.Flag.AdvNormal:
                LoadAdventureNormal();
                break;
            case MapManager.Flag.AdvHard:
                LoadAdventureHard();
                break;
            case MapManager.Flag.AdvInsane:
                LoadAdventureInsane();
                break;
        }
    }

    public void TutorialNext()
    {
        if (pm == null || pm.MapFiles == null || pm.MapFiles.Count == 0) return;

        foreach (Transform obj in GameObject.Find("Objects").GetComponentsInChildren<Transform>())
        {
            if (obj.gameObject.name.Equals("Objects")) continue;
            Destroy(obj.gameObject);
        }
        for (int i = playingMapIndex + 1; i <= pm.MapFiles.Count; i++)
        {
            if (i >= pm.PlayLength)
            {
                // TODO Victory
                HasClearedAll = true;
                break;
            }
            MapManager.OpenFileFlag openFileFlag = mm.InitializeFromText(pm.MapFiles[i].text, out _, out _, out _, out _, out _, out _);
            if (openFileFlag != MapManager.OpenFileFlag.Success)
            {
                continue;
            }
            else
            {
                playingMapIndex = i;
                pm.TutorialAfterGravity(MapManager.Flag.Continued);
                mm.TimeActivate();
                canPlay = true;
                break;
            }
        }
    }

    public void PlayNext()
    {
        // TODO mapList? or pm.mapFiles?
        if (pm == null || pm.MapFiles == null || pm.MapFiles.Count == 0) return;

        for (int i = playingMapIndex + 1; i <= pm.MapFiles.Count; i++)
        {
            if (i >= pm.PlayLength)
            {
                // TODO Victory
                HasClearedAll = true;
                break;
            }
            MapManager.OpenFileFlag openFileFlag = mm.InitializeFromText(pm.MapFiles[i].text, out _, out _, out _, out _, out _, out _);
            if (openFileFlag != MapManager.OpenFileFlag.Success)
            {
                continue;
            }
            else
            {
                playingMapIndex = i;
                pm.PlayAfterGravity(MapManager.Flag.Continued);
                mm.TimeActivate();
                canPlay = true;
                break;
            }
        }
    }
}
