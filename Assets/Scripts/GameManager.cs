using System;
using System.Collections;
using System.Collections.Generic;
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

    private void Awake()
    {
        if (gm != null)
        {
            Destroy(gameObject);
        }
        gm = this;
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        // TODO: 씬 바뀔 때마다 적절한 레벨 선택하고 MapManager 찾아서 맵 로드해야 함
        if (SceneManager.GetActiveScene().name.Equals("Main"))
        {
            mm = GameObject.FindGameObjectWithTag("MapManager").GetComponent<MapManager>();
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

            mm.Initialize(7, 7, walls, objects, "d");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: 입력 담당
        if (mm is null || !mm.IsReady) return;

        //MapManager.Flag flag;

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            mm.Gravity(GravityDirection.Down, out _);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            mm.Gravity(GravityDirection.Up, out _);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            mm.Gravity(GravityDirection.Left, out _);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            mm.Gravity(GravityDirection.Right, out _);
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            mm.Restart();
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

    public void MapEditor()
    {
        SceneManager.LoadScene("Editor");
    }
}
