using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorManager : MonoBehaviour
{
    public enum EditMode { None, Wall, Exit, RemoveWall, Ball, Iron, Fire, RemoveObject }

    public Camera mainCamera;
    public Grid grid;
    public MapManager mm;

    public EditMode editMode = EditMode.Wall;//EditMode.None;

    private int sizeX = 8;
    private int sizeY = 8;
    private List<WallInfo> walls = new List<WallInfo>();
    private List<ObjectInfo> objects = new List<ObjectInfo>();
    private string solution = "";

    private int currentTouchX;
    private int currentTouchY;
    private List<WallInfo> tempWalls;
    private List<ObjectInfo> tempObjects;

    // Start is called before the first frame update
    void Start()
    {
        mm.Initialize(sizeX, sizeY, walls, objects);
        editMode = EditMode.Wall;
    }

    // Update is called once per frame
    void Update()
    {
#if (UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_WP8_1) && !UNITY_EDITOR
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            if (touch.phase == TouchPhase.Began)
            {
                RaycastHit hit;
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit))
                {
                    tempWalls = walls.ConvertAll(i => new WallInfo(i.type, i.x, i.y));
                    tempObjects = objects.ConvertAll(i => new ObjectInfo(i.type, i.x, i.y));
                    if (editMode == EditMode.Exit)
                    {
                        if (tempWalls.Exists((i) => i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.ExitVertical))
                        {
                            tempWalls.Remove(tempWalls.Find((i) => i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.ExitVertical));
                        }
                    }
                    TouchMap(hit.point.x, hit.point.y, editMode, tempWalls, tempObjects, touch.fingerId);
                }
            }

            if (touch.phase == TouchPhase.Moved)
            {
                RaycastHit hit;
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit))
                {
                    tempWalls = walls.ConvertAll(i => new WallInfo(i.type, i.x, i.y));
                    tempObjects = objects.ConvertAll(i => new ObjectInfo(i.type, i.x, i.y));
                    if (editMode == EditMode.Exit)
                    {
                        if (tempWalls.Exists((i) => i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.ExitVertical))
                        {
                            tempWalls.Remove(tempWalls.Find((i) => i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.ExitVertical));
                        }
                    }
                    TouchMap(hit.point.x, hit.point.y, editMode, tempWalls, tempObjects, touch.fingerId);
                }
            }

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                RaycastHit hit;
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit))
                {
                    TouchMap(hit.point.x, hit.point.y, editMode, walls, objects, touch.fingerId, true);
                }
            }
        }
#else
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                tempWalls = walls.ConvertAll(i => new WallInfo(i.type, i.x, i.y));
                tempObjects = objects.ConvertAll(i => new ObjectInfo(i.type, i.x, i.y));
                if (editMode == EditMode.Exit)
                {
                    if (tempWalls.Exists((i) => i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.ExitVertical))
                    {
                        tempWalls.Remove(tempWalls.Find((i) => i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.ExitVertical));
                    }
                }
                TouchMap(hit.point.x, hit.point.y, editMode, tempWalls, tempObjects, -1);
            }
        }
        if (Input.GetMouseButton(0))
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                tempWalls = walls.ConvertAll(i => new WallInfo(i.type, i.x, i.y));
                tempObjects = objects.ConvertAll(i => new ObjectInfo(i.type, i.x, i.y));
                if (editMode == EditMode.Exit)
                {
                    if (tempWalls.Exists((i) => i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.ExitVertical))
                    {
                        tempWalls.Remove(tempWalls.Find((i) => i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.ExitVertical));
                    }
                }
                TouchMap(hit.point.x, hit.point.y, editMode, tempWalls, tempObjects, -1);
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                TouchMap(hit.point.x, hit.point.y, editMode, walls, objects, -1, true);
            }
        }
#endif
    }

    void TouchMap(float x, float y, EditMode editMode, List<WallInfo> walls, List<ObjectInfo> objects, int touchID, bool verbose = false)
    {
        bool hasChanged = false;
        switch (editMode)
        {
            case EditMode.Wall:
                #region Wall
                if ((Mathf.FloorToInt(x + y) - Mathf.FloorToInt(y - x)) % 2 == 0)
                {
                    // Horizontal wall
                    int a = (Mathf.FloorToInt(x + y) - Mathf.FloorToInt(y - x)) / 2;
                    int b = (Mathf.FloorToInt(x + y) + Mathf.FloorToInt(y - x)) / 2;

                    if (a < 1 || a > sizeX || b < 1 || b > sizeY - 1)
                    {
                        if (verbose) Debug.LogWarning("Editor invalid: horizontal wall position at (" + a + ", " + b + ")");
                        break;
                    }
                    if (walls.Contains(new WallInfo(WallInfo.Type.Horizontal, a, b)))
                    {
                        if (verbose) Debug.LogWarning("Editor invalid: horizontal wall overlapped at (" + a + ", " + b + ")");
                        break;
                    }

                    if (verbose) Debug.Log("Add horizontal wall at (" + a + ", " + b + ")");
                    walls.Add(new WallInfo(WallInfo.Type.Horizontal, a, b));
                    hasChanged = true;
                }
                else
                {
                    // Vertical wall
                    int a = (Mathf.FloorToInt(x + y) - Mathf.CeilToInt(y - x)) / 2;
                    int b = (Mathf.FloorToInt(x + y) + Mathf.CeilToInt(y - x)) / 2;

                    if (a < 1 || a > sizeX - 1 || b < 1 || b > sizeY)
                    {
                        if (verbose) Debug.LogWarning("Editor invalid: vertical wall position at (" + a + ", " + b + ")");
                        break;
                    }
                    if (walls.Contains(new WallInfo(WallInfo.Type.Vertical, a, b)))
                    {
                        if (verbose) Debug.LogWarning("Editor invalid: vertical wall overlapped at (" + a + ", " + b + ")");
                        break;
                    }

                    if (verbose) Debug.Log("Add vertical wall at (" + a + ", " + b + ")");
                    walls.Add(new WallInfo(WallInfo.Type.Vertical, a, b));
                    hasChanged = true;
                }
                break;
            #endregion
            case EditMode.Exit:
                #region Exit
                if (x >= 0.5f && x < sizeX + 0.5f && y >= 0.5f && y < sizeY + 0.5f && (Mathf.FloorToInt(x + y) - Mathf.FloorToInt(y - x)) % 2 == 0)
                {
                    // Horizontal exit
                    int a = (Mathf.FloorToInt(x + y) - Mathf.FloorToInt(y - x)) / 2;
                    int b = (Mathf.FloorToInt(x + y) + Mathf.FloorToInt(y - x)) / 2;

                    if (a < 1 || a > sizeX || !(b == 0 || b == sizeY))
                    {
                        if (verbose) Debug.LogWarning("Editor invalid: horizontal exit position at (" + a + ", " + b + ")");
                        break;
                    }
                    if (walls.Contains(new WallInfo(WallInfo.Type.ExitHorizontal, a, b)))
                    {
                        if (verbose) Debug.LogWarning("Editor invalid: horizontal exit overlapped at (" + a + ", " + b + ")");
                        break;
                    }

                    if (walls.Exists((i) => i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.ExitVertical))
                    {
                        walls.Remove(walls.Find((i) => i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.ExitVertical));
                        if (verbose) Debug.Log("Replace horizontal exit at (" + a + ", " + b + ")");
                    }
                    else
                    {
                        if (verbose) Debug.Log("Add horizontal exit at (" + a + ", " + b + ")");
                    }
                    walls.Add(new WallInfo(WallInfo.Type.ExitHorizontal, a, b));
                    hasChanged = true;
                }
                else if (x >= 0.5f && x < sizeX + 0.5f && y >= 0.5f && y < sizeY + 0.5f)
                {
                    // Vertical exit
                    int a = (Mathf.FloorToInt(x + y) - Mathf.CeilToInt(y - x)) / 2;
                    int b = (Mathf.FloorToInt(x + y) + Mathf.CeilToInt(y - x)) / 2;

                    if (!(a == 0 || a == sizeX) || b < 1 || b > sizeY)
                    {
                        if (verbose) Debug.LogWarning("Editor invalid: vertical exit position at (" + a + ", " + b + ")");
                        break;
                    }
                    if (walls.Contains(new WallInfo(WallInfo.Type.ExitVertical, a, b)))
                    {
                        if (verbose) Debug.LogWarning("Editor invalid: vertical exit overlapped at (" + a + ", " + b + ")");
                        break;
                    }

                    if (walls.Exists((i) => i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.ExitVertical))
                    {
                        walls.Remove(walls.Find((i) => i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.ExitVertical));
                        if (verbose) Debug.Log("Replace vertical exit at (" + a + ", " + b + ")");
                    }
                    else
                    {
                        if (verbose) Debug.Log("Add vertical exit at (" + a + ", " + b + ")");
                    }
                    walls.Add(new WallInfo(WallInfo.Type.ExitVertical, a, b));
                    hasChanged = true;
                }
                else if (x < 0.5f)
                {
                    // Vertical left exit
                    int a = 0;
                    int b = Mathf.FloorToInt(y + 0.5f);

                    if (b < 1 || b > sizeY)
                    {
                        if (verbose) Debug.LogWarning("Editor invalid: vertical exit position at (" + a + ", " + b + ")");
                        break;
                    }
                    if (walls.Contains(new WallInfo(WallInfo.Type.ExitVertical, a, b)))
                    {
                        if (verbose) Debug.LogWarning("Editor invalid: vertical exit overlapped at (" + a + ", " + b + ")");
                        break;
                    }

                    if (walls.Exists((i) => i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.ExitVertical))
                    {
                        walls.Remove(walls.Find((i) => i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.ExitVertical));
                        if (verbose) Debug.Log("Replace vertical exit at (" + a + ", " + b + ")");
                    }
                    else
                    {
                        if (verbose) Debug.Log("Add vertical exit at (" + a + ", " + b + ")");
                    }
                    walls.Add(new WallInfo(WallInfo.Type.ExitVertical, a, b));
                    hasChanged = true;
                }
                else if (x >= sizeX + 0.5f)
                {
                    // Vertical right exit
                    int a = sizeX;
                    int b = Mathf.FloorToInt(y + 0.5f);

                    if (b < 1 || b > sizeY)
                    {
                        if (verbose) Debug.LogWarning("Editor invalid: vertical exit position at (" + a + ", " + b + ")");
                        break;
                    }
                    if (walls.Contains(new WallInfo(WallInfo.Type.ExitVertical, a, b)))
                    {
                        if (verbose) Debug.LogWarning("Editor invalid: vertical exit overlapped at (" + a + ", " + b + ")");
                        break;
                    }

                    if (walls.Exists((i) => i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.ExitVertical))
                    {
                        walls.Remove(walls.Find((i) => i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.ExitVertical));
                        if (verbose) Debug.Log("Replace vertical exit at (" + a + ", " + b + ")");
                    }
                    else
                    {
                        if (verbose) Debug.Log("Add vertical exit at (" + a + ", " + b + ")");
                    }
                    walls.Add(new WallInfo(WallInfo.Type.ExitVertical, a, b));
                    hasChanged = true;
                }
                else if (y < 0.5f)
                {
                    // Horizontal down exit
                    int a = Mathf.FloorToInt(x + 0.5f);
                    int b = 0;

                    if (a < 1 || a > sizeX)
                    {
                        if (verbose) Debug.LogWarning("Editor invalid: horizontal exit position at (" + a + ", " + b + ")");
                        break;
                    }
                    if (walls.Contains(new WallInfo(WallInfo.Type.ExitHorizontal, a, b)))
                    {
                        if (verbose) Debug.LogWarning("Editor invalid: horizontal exit overlapped at (" + a + ", " + b + ")");
                        break;
                    }

                    if (walls.Exists((i) => i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.ExitVertical))
                    {
                        walls.Remove(walls.Find((i) => i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.ExitVertical));
                        if (verbose) Debug.Log("Replace horizontal exit at (" + a + ", " + b + ")");
                    }
                    else
                    {
                        if (verbose) Debug.Log("Add horizontal exit at (" + a + ", " + b + ")");
                    }
                    walls.Add(new WallInfo(WallInfo.Type.ExitHorizontal, a, b));
                    hasChanged = true;
                }
                else // if (y >= sizeY + 0.5f)
                {
                    // Horizontal up exit
                    int a = Mathf.FloorToInt(x + 0.5f);
                    int b = sizeY;

                    if (a < 1 || a > sizeX)
                    {
                        if (verbose) Debug.LogWarning("Editor invalid: horizontal exit position at (" + a + ", " + b + ")");
                        break;
                    }
                    if (walls.Contains(new WallInfo(WallInfo.Type.ExitHorizontal, a, b)))
                    {
                        if (verbose) Debug.LogWarning("Editor invalid: horizontal exit overlapped at (" + a + ", " + b + ")");
                        break;
                    }

                    if (walls.Exists((i) => i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.ExitVertical))
                    {
                        walls.Remove(walls.Find((i) => i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.ExitVertical));
                        if (verbose) Debug.Log("Replace horizontal exit at (" + a + ", " + b + ")");
                    }
                    else
                    {
                        if (verbose) Debug.Log("Add horizontal exit at (" + a + ", " + b + ")");
                    }
                    walls.Add(new WallInfo(WallInfo.Type.ExitHorizontal, a, b));
                    hasChanged = true;
                }
                break;
                #endregion
        }

        // Map Rendering
        mm.Initialize(sizeX, sizeY, walls, objects, "");

        if (hasChanged)
        {
            
        }
    }
}
