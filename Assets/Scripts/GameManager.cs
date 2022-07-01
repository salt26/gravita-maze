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

public class GameManager : MonoBehaviour
{

    public static GameManager gm;
    public static MapManager mm = null;

    public enum GravityDirection { Up, Down, Left, Right }

    [HideInInspector]
    public bool canPlay = true;

    private List<string> mapList;
    private int playingMapIndex = 0;
    private int life = 1;

    private void Awake()
    {
        if (gm != null)
        {
            Destroy(gm.gameObject);
        }
        gm = this;
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        // 입력 담당
        if (mm is null || !mm.IsReady) return;

        //MapManager.Flag flag;

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

    IEnumerator SceneLoading(string sceneName)
    {
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

        /*
        objects.Add(new ObjectInfo(ObjectInfo.Type.Iron, 1, 1));
        objects.Add(new ObjectInfo(ObjectInfo.Type.Ball, 5, 2));
        objects.Add(new ObjectInfo(ObjectInfo.Type.MapEditor, 1, 5));
        objects.Add(new ObjectInfo(ObjectInfo.Type.QuitGame, 1, 2));
        */

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
            yield return null;
        }
        canPlay = false;
    }

    IEnumerator InitializeMode()
    {
        while (mm == null)
        {
            mm = GameObject.FindGameObjectWithTag("MapManager").GetComponent<MapManager>();
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
            yield return null;
        }
        List<WallInfo> walls = new List<WallInfo>();

        walls.Add(new WallInfo(WallInfo.Type.Vertical, 1, 6));
        walls.Add(new WallInfo(WallInfo.Type.Vertical, 3, 4));
        walls.Add(new WallInfo(WallInfo.Type.Vertical, 1, 2));
        walls.Add(new WallInfo(WallInfo.Type.Vertical, 2, 5));
        walls.Add(new WallInfo(WallInfo.Type.Vertical, 3, 8));
        walls.Add(new WallInfo(WallInfo.Type.Vertical, 7, 1));  // TODO 나중에 해금
        walls.Add(new WallInfo(WallInfo.Type.Vertical, 7, 3));  // TODO 나중에 해금
        walls.Add(new WallInfo(WallInfo.Type.Vertical, 7, 5));  // TODO 나중에 해금
        walls.Add(new WallInfo(WallInfo.Type.Vertical, 7, 7));  // TODO 나중에 해금
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
            yield return null;
        }

        life = int.MaxValue;
        mm.afterGravity = TutorialAfterGravity;

        mapList = Directory.GetFiles("Assets/PredefinedMaps/Tutorial/", "*.txt").ToList();
        for (int i = 0; i < mapList.Count; i++)
        {
            Debug.Log(mapList[i]);
        }
        for (int i = 0; i < mapList.Count; i++)
        {
            MapManager.OpenFileFlag openFileFlag = mm.InitializeFromFile(mapList[i], out _, out _, out _, out _, out _, out _);
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
                // TODO
                break;
            case MapManager.Flag.AdvNormal:
                // TODO
                break;
            case MapManager.Flag.AdvHard:
                // TODO
                break;
            case MapManager.Flag.AdvInsane:
                // TODO
                break;
        }
    }

    public void TutorialAfterGravity(MapManager.Flag flag)
    {
        if (mapList == null || mapList.Count == 0) return;
        MapManager.OpenFileFlag openFileFlag;
        switch (flag)
        {
            case MapManager.Flag.Continued:
                break;
            case MapManager.Flag.Escaped:
                foreach (Transform obj in GameObject.Find("Objects").GetComponentsInChildren<Transform>())
                {
                    if (obj.gameObject.name.Equals("Objects")) continue;
                    Destroy(obj.gameObject);
                }
                for (int i = playingMapIndex + 1; i <= mapList.Count; i++)
                {
                    if (i >= mapList.Count)
                    {
                        // TODO Victory
                        LoadMain();
                        break;
                    }
                    openFileFlag = mm.InitializeFromFile(mapList[i], out _, out _, out _, out _, out _, out _);
                    if (openFileFlag != MapManager.OpenFileFlag.Success)
                    {
                        continue;
                    }
                    else
                    {
                        playingMapIndex = i;
                        mm.TimeActivate();
                        canPlay = true;
                        break;
                    }
                }
                break;
            case MapManager.Flag.Burned:
                break;
            case MapManager.Flag.Squashed:
                break;
            case MapManager.Flag.TimeOver:
                // Retry the map
                openFileFlag = mm.InitializeFromFile(mapList[playingMapIndex], out _, out _, out _, out _, out _, out _);
                if (openFileFlag != MapManager.OpenFileFlag.Success)
                {
                    Debug.LogError("Tutorial invalid: map has corrupted");
                    LoadMain();
                }
                else
                {
                    mm.TimeActivate();
                    canPlay = true;
                }
                break;
        }
    }

    public void PlayAfterGravity(MapManager.Flag flag)
    {
        if (mapList == null || mapList.Count == 0) return;
        switch (flag)
        {
            case MapManager.Flag.Continued:
                break;
            case MapManager.Flag.Escaped:
                for (int i = playingMapIndex + 1; i <= mapList.Count; i++)
                {
                    if (i >= mapList.Count)
                    {
                        // TODO Victory
                        LoadMain();
                        break;
                    }
                    MapManager.OpenFileFlag openFileFlag = mm.InitializeFromFile(mapList[i], out _, out _, out _, out _, out _, out _);
                    if (openFileFlag != MapManager.OpenFileFlag.Success)
                    {
                        continue;
                    }
                    else
                    {
                        playingMapIndex = i;
                        mm.TimeActivate();
                        canPlay = true;
                        break;
                    }
                }
                break;
            case MapManager.Flag.Burned:
                break;
            case MapManager.Flag.Squashed:
                break;
            case MapManager.Flag.TimeOver:
                life--;
                if (life <= 0)
                {
                    // TODO Defeat
                    LoadMain();
                    break;
                }
                for (int i = playingMapIndex + 1; i <= mapList.Count; i++)
                {
                    if (i >= mapList.Count)
                    {
                        // TODO Victory
                        LoadMain();
                        break;
                    }
                    MapManager.OpenFileFlag openFileFlag = mm.InitializeFromFile(mapList[i], out _, out _, out _, out _, out _, out _);
                    if (openFileFlag != MapManager.OpenFileFlag.Success)
                    {
                        continue;
                    }
                    else
                    {
                        playingMapIndex = i;
                        mm.TimeActivate();
                        canPlay = true;
                        break;
                    }
                }
                break;
        }
    }
}
