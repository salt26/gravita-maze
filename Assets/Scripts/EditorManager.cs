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
                else if (editMode == EditMode.Ball)
                {
                    if (tempObjects.Exists((i) => i.type == ObjectInfo.Type.Ball))
                    {
                        tempObjects.Remove(tempObjects.Find((i) => i.type == ObjectInfo.Type.Ball));
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
                else if (editMode == EditMode.Ball)
                {
                    if (tempObjects.Exists((i) => i.type == ObjectInfo.Type.Ball))
                    {
                        tempObjects.Remove(tempObjects.Find((i) => i.type == ObjectInfo.Type.Ball));
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

    bool TouchMap(float x, float y, EditMode editMode, List<WallInfo> walls, List<ObjectInfo> objects, int touchID, bool verbose = false)
    {
        bool hasChanged = false;
        int a, b;
        switch (editMode)
        {
            case EditMode.Wall:
                #region Wall
                if ((Mathf.FloorToInt(x + y) - Mathf.FloorToInt(y - x)) % 2 == 0)
                {
                    // Horizontal wall
                    a = (Mathf.FloorToInt(x + y) - Mathf.FloorToInt(y - x)) / 2;
                    b = (Mathf.FloorToInt(x + y) + Mathf.FloorToInt(y - x)) / 2;

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
                    a = (Mathf.FloorToInt(x + y) - Mathf.CeilToInt(y - x)) / 2;
                    b = (Mathf.FloorToInt(x + y) + Mathf.CeilToInt(y - x)) / 2;

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
                    a = (Mathf.FloorToInt(x + y) - Mathf.FloorToInt(y - x)) / 2;
                    b = (Mathf.FloorToInt(x + y) + Mathf.FloorToInt(y - x)) / 2;

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
                    a = (Mathf.FloorToInt(x + y) - Mathf.CeilToInt(y - x)) / 2;
                    b = (Mathf.FloorToInt(x + y) + Mathf.CeilToInt(y - x)) / 2;

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
                    a = 0;
                    b = Mathf.FloorToInt(y + 0.5f);

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
                    a = sizeX;
                    b = Mathf.FloorToInt(y + 0.5f);

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
                    a = Mathf.FloorToInt(x + 0.5f);
                    b = 0;

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
                    a = Mathf.FloorToInt(x + 0.5f);
                    b = sizeY;

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
            case EditMode.RemoveWall:
                #region Remove wall
                if (x >= 0.5f && x < sizeX + 0.5f && y >= 0.5f && y < sizeY + 0.5f && (Mathf.FloorToInt(x + y) - Mathf.FloorToInt(y - x)) % 2 == 0)
                {
                    // Remove horizontal wall or exit
                    a = (Mathf.FloorToInt(x + y) - Mathf.FloorToInt(y - x)) / 2;
                    b = (Mathf.FloorToInt(x + y) + Mathf.FloorToInt(y - x)) / 2;

                    if (a < 0 || a > sizeX + 1 || b < 0 || b > sizeY)
                    {
                        if (verbose) Debug.LogWarning("Editor invalid: horizontal wall position at (" + a + ", " + b + ")");
                        break;
                    }
                    if (!walls.Contains(new WallInfo(WallInfo.Type.Horizontal, a, b)) &&
                        !walls.Contains(new WallInfo(WallInfo.Type.ExitHorizontal, a, b)))
                    {
                        if (verbose) Debug.LogWarning("Editor invalid: horizontal wall or exit doesn't exist at (" + a + ", " + b + ")");
                        break;
                    }

                    if (verbose) Debug.Log("Remove horizontal wall or exit at (" + a + ", " + b + ")");
                    walls.Remove(walls.Find((i) => (i.type == WallInfo.Type.Horizontal || i.type == WallInfo.Type.ExitHorizontal) && i.x == a && i.y == b));
                    hasChanged = true;
                }
                else if (x >= 0.5f && x < sizeX + 0.5f && y >= 0.5f && y < sizeY + 0.5f)
                {
                    // Remove vertical wall or exit
                    a = (Mathf.FloorToInt(x + y) - Mathf.CeilToInt(y - x)) / 2;
                    b = (Mathf.FloorToInt(x + y) + Mathf.CeilToInt(y - x)) / 2;

                    if (a < 0 || a > sizeX || b < 0 || b > sizeY + 1)
                    {
                        if (verbose) Debug.LogWarning("Editor invalid: vertical wall position at (" + a + ", " + b + ")");
                        break;
                    }
                    if (!walls.Contains(new WallInfo(WallInfo.Type.Vertical, a, b)) &&
                        !walls.Contains(new WallInfo(WallInfo.Type.ExitVertical, a, b)))
                    {
                        if (verbose) Debug.LogWarning("Editor invalid: vertical wall or exit doesn't exist at (" + a + ", " + b + ")");
                        break;
                    }

                    if (verbose) Debug.Log("Remove vertical wall or exit at (" + a + ", " + b + ")");
                    walls.Remove(walls.Find((i) => (i.type == WallInfo.Type.Vertical || i.type == WallInfo.Type.ExitVertical) && i.x == a && i.y == b));
                    hasChanged = true;
                }
                else if (x < 0.5f)
                {
                    // Remove vertical left exit
                    a = 0;
                    b = Mathf.FloorToInt(y + 0.5f);

                    if (b < 1 || b > sizeY)
                    {
                        if (verbose) Debug.LogWarning("Editor invalid: vertical exit position at (" + a + ", " + b + ")");
                        break;
                    }
                    if (!walls.Contains(new WallInfo(WallInfo.Type.ExitVertical, a, b)))
                    {
                        if (verbose) Debug.LogWarning("Editor invalid: vertical exit doesn't exist at (" + a + ", " + b + ")");
                        break;
                    }

                    walls.Remove(walls.Find((i) => i.type == WallInfo.Type.ExitVertical && i.x == a && i.y == b));
                    if (verbose) Debug.Log("Remove vertical exit at (" + a + ", " + b + ")");
                    hasChanged = true;
                }
                else if (x >= sizeX + 0.5f)
                {
                    // Remove vertical right exit
                    a = sizeX;
                    b = Mathf.FloorToInt(y + 0.5f);

                    if (b < 1 || b > sizeY)
                    {
                        if (verbose) Debug.LogWarning("Editor invalid: vertical exit position at (" + a + ", " + b + ")");
                        break;
                    }
                    if (!walls.Contains(new WallInfo(WallInfo.Type.ExitVertical, a, b)))
                    {
                        if (verbose) Debug.LogWarning("Editor invalid: vertical exit doesn't exist at (" + a + ", " + b + ")");
                        break;
                    }

                    walls.Remove(walls.Find((i) => i.type == WallInfo.Type.ExitVertical && i.x == a && i.y == b));
                    if (verbose) Debug.Log("Remove vertical exit at (" + a + ", " + b + ")");
                    hasChanged = true;
                }
                else if (y < 0.5f)
                {
                    // Remove horizontal down exit
                    a = Mathf.FloorToInt(x + 0.5f);
                    b = 0;

                    if (a < 1 || a > sizeX)
                    {
                        if (verbose) Debug.LogWarning("Editor invalid: horizontal exit position at (" + a + ", " + b + ")");
                        break;
                    }
                    if (!walls.Contains(new WallInfo(WallInfo.Type.ExitHorizontal, a, b)))
                    {
                        if (verbose) Debug.LogWarning("Editor invalid: horizontal exit doesn't exist at (" + a + ", " + b + ")");
                        break;
                    }

                    walls.Remove(walls.Find((i) => i.type == WallInfo.Type.ExitHorizontal && i.x == a && i.y == b));
                    if (verbose) Debug.Log("Remove horizontal exit at (" + a + ", " + b + ")");
                    hasChanged = true;
                }
                else // if (y >= sizeY + 0.5f)
                {
                    // Remove horizontal up exit
                    a = Mathf.FloorToInt(x + 0.5f);
                    b = sizeY;

                    if (a < 1 || a > sizeX)
                    {
                        if (verbose) Debug.LogWarning("Editor invalid: horizontal exit position at (" + a + ", " + b + ")");
                        break;
                    }
                    if (!walls.Contains(new WallInfo(WallInfo.Type.ExitHorizontal, a, b)))
                    {
                        if (verbose) Debug.LogWarning("Editor invalid: horizontal exit doesn't exist at (" + a + ", " + b + ")");
                        break;
                    }

                    walls.Remove(walls.Find((i) => i.type == WallInfo.Type.ExitHorizontal && i.x == a && i.y == b));
                    if (verbose) Debug.Log("Remove horizontal exit at (" + a + ", " + b + ")");
                    hasChanged = true;
                }
                break;
            #endregion
            case EditMode.Ball:
                #region Ball
                a = Mathf.FloorToInt(x + 0.5f);
                b = Mathf.FloorToInt(y + 0.5f);

                if (a < 1 || a > sizeX || b < 1 || b > sizeY)
                {
                    if (verbose) Debug.LogWarning("Editor invalid: ball position at (" + a + ", " + b + ")");
                    break;
                }
                if (objects.Exists(i => i.x == a && i.y == b))
                {
                    if (verbose) Debug.LogWarning("Editor invalid: objects overlapped at (" + a + ", " + b + ")");
                    break;
                }

                if (objects.Exists((i) => i.type == ObjectInfo.Type.Ball))
                {
                    objects.Remove(objects.Find((i) => i.type == ObjectInfo.Type.Ball));
                    if (verbose) Debug.Log("Replace ball at (" + a + ", " + b + ")");
                }
                else
                {
                    if (verbose) Debug.Log("Add ball at (" + a + ", " + b + ")");
                }
                objects.Add(new ObjectInfo(ObjectInfo.Type.Ball, a, b));
                hasChanged = true;
                break;
            #endregion
            case EditMode.Iron:
                #region Iron
                a = Mathf.FloorToInt(x + 0.5f);
                b = Mathf.FloorToInt(y + 0.5f);

                if (a < 1 || a > sizeX || b < 1 || b > sizeY)
                {
                    if (verbose) Debug.LogWarning("Editor invalid: iron position at (" + a + ", " + b + ")");
                    break;
                }
                if (objects.Exists(i => i.x == a && i.y == b))
                {
                    if (verbose) Debug.LogWarning("Editor invalid: objects overlapped at (" + a + ", " + b + ")");
                    break;
                }

                objects.Add(new ObjectInfo(ObjectInfo.Type.Iron, a, b));
                if (verbose) Debug.Log("Add iron at (" + a + ", " + b + ")");
                hasChanged = true;
                break;
            #endregion
            case EditMode.Fire:
                #region Fire
                a = Mathf.FloorToInt(x + 0.5f);
                b = Mathf.FloorToInt(y + 0.5f);

                if (a < 1 || a > sizeX || b < 1 || b > sizeY)
                {
                    if (verbose) Debug.LogWarning("Editor invalid: fire position at (" + a + ", " + b + ")");
                    break;
                }
                if (objects.Exists(i => i.x == a && i.y == b))
                {
                    if (verbose) Debug.LogWarning("Editor invalid: objects overlapped at (" + a + ", " + b + ")");
                    break;
                }

                objects.Add(new ObjectInfo(ObjectInfo.Type.Fire, a, b));
                if (verbose) Debug.Log("Add fire at (" + a + ", " + b + ")");
                hasChanged = true;
                break;
                #endregion
            case EditMode.RemoveObject:
                #region Remove object
                a = Mathf.FloorToInt(x + 0.5f);
                b = Mathf.FloorToInt(y + 0.5f);

                if (a < 1 || a > sizeX || b < 1 || b > sizeY)
                {
                    if (verbose) Debug.LogWarning("Editor invalid: object position at (" + a + ", " + b + ")");
                    break;
                }
                if (!objects.Exists(i => i.x == a && i.y == b))
                {
                    if (verbose) Debug.LogWarning("Editor invalid: object doesn't exists at (" + a + ", " + b + ")");
                    break;
                }

                objects.Remove(objects.Find(i => i.x == a && i.y == b));
                if (verbose) Debug.Log("Remove object at (" + a + ", " + b + ")");
                hasChanged = true;
                break;
                #endregion
        }

        // Map Rendering
        mm.Initialize(sizeX, sizeY, walls, objects, "");

        return hasChanged;
    }
}
