using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EditorManager : MonoBehaviour
{
    public enum EditMode { None, Wall, Exit, RemoveWall, Ball, Iron, Fire, RemoveObject, Simulation }

    public Camera mainCamera;
    public Grid grid;
    public MapManager mm;
    public List<Button> editorButtons;
    public Button editorUndoButton;
    public Button editorRedoButton;
    public Dropdown editorSizeXDropdown;
    public Dropdown editorSizeYDropdown;
    public InputField editorMapNameInput;

    public EditMode editMode = EditMode.None;

    private int sizeX;
    private int sizeY;
    private List<WallInfo> walls = new List<WallInfo>();
    private List<ObjectInfo> objects = new List<ObjectInfo>();
    private string solution = "";
    private string mapName = "";

    private int currentTouchX;
    private int currentTouchY;
    private List<WallInfo> tempWalls;
    private List<ObjectInfo> tempObjects;

    private List<EditActionInfo> undoStack = new List<EditActionInfo>();
    private List<EditActionInfo> redoStack = new List<EditActionInfo>();

    // Start is called before the first frame update
    void Start()
    {
        sizeX = Mathf.Clamp(editorSizeXDropdown.value + MapManager.MIN_SIZE_X, MapManager.MIN_SIZE_X, MapManager.MAX_SIZE_X);
        sizeY = Mathf.Clamp(editorSizeYDropdown.value + MapManager.MIN_SIZE_Y, MapManager.MIN_SIZE_Y, MapManager.MAX_SIZE_Y);
        mm.Initialize(sizeX, sizeY, walls, objects);

        foreach (Button b in editorButtons)
        {
            if (b != null) b.interactable = true;
        }
        editMode = EditMode.None;
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
                    TouchMap(hit.point.x, hit.point.y, editMode, walls, objects, touch.fingerId, true, true);
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
                TouchMap(hit.point.x, hit.point.y, editMode, walls, objects, -1, true, true);
            }
        }
#endif
        editorUndoButton.interactable = undoStack.Count > 0;
        editorRedoButton.interactable = redoStack.Count > 0;
    }

    bool TouchMap(float x, float y, EditMode editMode, List<WallInfo> walls, List<ObjectInfo> objects, int touchID,
        bool commitAction = false, bool verbose = false)
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
                    if (commitAction)
                    {
                        undoStack.Add(new EditActionInfo(null, new WallInfo(WallInfo.Type.Horizontal, a, b)));
                        redoStack.Clear();
                    }
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
                    if (commitAction)
                    {
                        undoStack.Add(new EditActionInfo(null, new WallInfo(WallInfo.Type.Vertical, a, b)));
                        redoStack.Clear();
                    }
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
                        if (verbose) Debug.Log("Replace horizontal exit at (" + a + ", " + b + ")");
                        if (commitAction)
                        {
                            undoStack.Add(new EditActionInfo(walls.Find((i) => i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.ExitVertical),
                                new WallInfo(WallInfo.Type.ExitHorizontal, a, b)));
                            redoStack.Clear();
                        }
                        walls.Remove(walls.Find((i) => i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.ExitVertical));
                    }
                    else
                    {
                        if (verbose) Debug.Log("Add horizontal exit at (" + a + ", " + b + ")");
                        if (commitAction)
                        {
                            undoStack.Add(new EditActionInfo(null, new WallInfo(WallInfo.Type.ExitHorizontal, a, b)));
                            redoStack.Clear();
                        }
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
                        if (verbose) Debug.Log("Replace vertical exit at (" + a + ", " + b + ")");
                        if (commitAction)
                        {
                            undoStack.Add(new EditActionInfo(walls.Find((i) => i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.ExitVertical),
                                new WallInfo(WallInfo.Type.ExitVertical, a, b)));
                            redoStack.Clear();
                        }
                        walls.Remove(walls.Find((i) => i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.ExitVertical));
                    }
                    else
                    {
                        if (verbose) Debug.Log("Add vertical exit at (" + a + ", " + b + ")");
                        if (commitAction)
                        {
                            undoStack.Add(new EditActionInfo(null, new WallInfo(WallInfo.Type.ExitVertical, a, b)));
                            redoStack.Clear();
                        }
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
                        if (verbose) Debug.Log("Replace vertical exit at (" + a + ", " + b + ")");
                        if (commitAction)
                        {
                            undoStack.Add(new EditActionInfo(walls.Find((i) => i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.ExitVertical),
                                new WallInfo(WallInfo.Type.ExitVertical, a, b)));
                            redoStack.Clear();
                        }
                        walls.Remove(walls.Find((i) => i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.ExitVertical));
                    }
                    else
                    {
                        if (verbose) Debug.Log("Add vertical exit at (" + a + ", " + b + ")");
                        if (commitAction)
                        {
                            undoStack.Add(new EditActionInfo(null, new WallInfo(WallInfo.Type.ExitVertical, a, b)));
                            redoStack.Clear();
                        }
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
                        if (verbose) Debug.Log("Replace vertical exit at (" + a + ", " + b + ")");
                        if (commitAction)
                        {
                            undoStack.Add(new EditActionInfo(walls.Find((i) => i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.ExitVertical),
                                new WallInfo(WallInfo.Type.ExitVertical, a, b)));
                            redoStack.Clear();
                        }
                        walls.Remove(walls.Find((i) => i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.ExitVertical));
                    }
                    else
                    {
                        if (verbose) Debug.Log("Add vertical exit at (" + a + ", " + b + ")");
                        if (commitAction)
                        {
                            undoStack.Add(new EditActionInfo(null, new WallInfo(WallInfo.Type.ExitVertical, a, b)));
                            redoStack.Clear();
                        }
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
                        if (verbose) Debug.Log("Replace horizontal exit at (" + a + ", " + b + ")");
                        if (commitAction)
                        {
                            undoStack.Add(new EditActionInfo(walls.Find((i) => i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.ExitVertical),
                                new WallInfo(WallInfo.Type.ExitHorizontal, a, b)));
                            redoStack.Clear();
                        }
                        walls.Remove(walls.Find((i) => i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.ExitVertical));
                    }
                    else
                    {
                        if (verbose) Debug.Log("Add horizontal exit at (" + a + ", " + b + ")");
                        if (commitAction)
                        {
                            undoStack.Add(new EditActionInfo(null, new WallInfo(WallInfo.Type.ExitHorizontal, a, b)));
                            redoStack.Clear();
                        }
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
                        if (verbose) Debug.Log("Replace horizontal exit at (" + a + ", " + b + ")");
                        if (commitAction)
                        {
                            undoStack.Add(new EditActionInfo(walls.Find((i) => i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.ExitVertical),
                                new WallInfo(WallInfo.Type.ExitHorizontal, a, b)));
                            redoStack.Clear();
                        }
                        walls.Remove(walls.Find((i) => i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.ExitVertical));
                    }
                    else
                    {
                        if (verbose) Debug.Log("Add horizontal exit at (" + a + ", " + b + ")");
                        if (commitAction)
                        {
                            undoStack.Add(new EditActionInfo(null, new WallInfo(WallInfo.Type.ExitHorizontal, a, b)));
                            redoStack.Clear();
                        }
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
                    //if (commitAction &&
                    //    !walls.Exists((i) => (i.type == WallInfo.Type.Horizontal || i.type == WallInfo.Type.ExitHorizontal) && i.x == a && i.y == b))
                    //    Debug.LogError("Editor invalid: null in Removing horizontal wall or exit");
                    if (commitAction)
                    {
                        undoStack.Add(new EditActionInfo(walls.Find((i) =>
                            (i.type == WallInfo.Type.Horizontal || i.type == WallInfo.Type.ExitHorizontal) && i.x == a && i.y == b), null));
                        redoStack.Clear();
                    }
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
                    //if (commitAction &&
                    //    !walls.Exists((i) => (i.type == WallInfo.Type.Vertical || i.type == WallInfo.Type.ExitVertical) && i.x == a && i.y == b))
                    //    Debug.LogError("Editor invalid: null in Removing vertical wall or exit");
                    if (commitAction)
                    {
                        undoStack.Add(new EditActionInfo(walls.Find((i) =>
                            (i.type == WallInfo.Type.Vertical || i.type == WallInfo.Type.ExitVertical) && i.x == a && i.y == b), null));
                        redoStack.Clear();
                    }
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

                    if (verbose) Debug.Log("Remove vertical exit at (" + a + ", " + b + ")");
                    //if (commitAction && !walls.Exists((i) => i.type == WallInfo.Type.ExitVertical && i.x == a && i.y == b))
                    //    Debug.LogError("Editor invalid: null in Removing vertical left exit");
                    if (commitAction)
                    {
                        undoStack.Add(new EditActionInfo(walls.Find((i) => i.type == WallInfo.Type.ExitVertical && i.x == a && i.y == b), null));
                        redoStack.Clear();
                    }
                    walls.Remove(walls.Find((i) => i.type == WallInfo.Type.ExitVertical && i.x == a && i.y == b));
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

                    if (verbose) Debug.Log("Remove vertical exit at (" + a + ", " + b + ")");
                    //if (commitAction && !walls.Exists((i) => i.type == WallInfo.Type.ExitVertical && i.x == a && i.y == b))
                    //    Debug.LogError("Editor invalid: null in Removing vertical right exit");
                    if (commitAction)
                    {
                        undoStack.Add(new EditActionInfo(walls.Find((i) => i.type == WallInfo.Type.ExitVertical && i.x == a && i.y == b), null));
                        redoStack.Clear();
                    }
                    walls.Remove(walls.Find((i) => i.type == WallInfo.Type.ExitVertical && i.x == a && i.y == b));
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

                    if (verbose) Debug.Log("Remove horizontal exit at (" + a + ", " + b + ")");
                    //if (commitAction && !walls.Exists((i) => i.type == WallInfo.Type.ExitHorizontal && i.x == a && i.y == b))
                    //    Debug.LogError("Editor invalid: null in Removing horizontal down exit");
                    if (commitAction)
                    {
                        undoStack.Add(new EditActionInfo(walls.Find((i) => i.type == WallInfo.Type.ExitHorizontal && i.x == a && i.y == b), null));
                        redoStack.Clear();
                    }
                    walls.Remove(walls.Find((i) => i.type == WallInfo.Type.ExitHorizontal && i.x == a && i.y == b));
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

                    if (verbose) Debug.Log("Remove horizontal exit at (" + a + ", " + b + ")");
                    //if (commitAction && !walls.Exists((i) => i.type == WallInfo.Type.ExitHorizontal && i.x == a && i.y == b))
                    //    Debug.LogError("Editor invalid: null in Removing horizontal up exit");
                    if (commitAction)
                    {
                        undoStack.Add(new EditActionInfo(walls.Find((i) => i.type == WallInfo.Type.ExitHorizontal && i.x == a && i.y == b), null));
                        redoStack.Clear();
                    }
                    walls.Remove(walls.Find((i) => i.type == WallInfo.Type.ExitHorizontal && i.x == a && i.y == b));
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
                    if (verbose) Debug.Log("Replace ball at (" + a + ", " + b + ")");
                    if (commitAction)
                    {
                        undoStack.Add(new EditActionInfo(objects.Find((i) => i.type == ObjectInfo.Type.Ball), new ObjectInfo(ObjectInfo.Type.Ball, a, b)));
                        redoStack.Clear();
                    }
                    objects.Remove(objects.Find((i) => i.type == ObjectInfo.Type.Ball));
                }
                else
                {
                    if (verbose) Debug.Log("Add ball at (" + a + ", " + b + ")");
                    if (commitAction)
                    {
                        undoStack.Add(new EditActionInfo(null, new ObjectInfo(ObjectInfo.Type.Ball, a, b)));
                        redoStack.Clear();
                    }
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

                if (verbose) Debug.Log("Add iron at (" + a + ", " + b + ")");
                if (commitAction)
                {
                    undoStack.Add(new EditActionInfo(null, new ObjectInfo(ObjectInfo.Type.Iron, a, b)));
                    redoStack.Clear();
                }
                objects.Add(new ObjectInfo(ObjectInfo.Type.Iron, a, b));
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

                if (verbose) Debug.Log("Add fire at (" + a + ", " + b + ")");
                if (commitAction)
                {
                    undoStack.Add(new EditActionInfo(null, new ObjectInfo(ObjectInfo.Type.Fire, a, b)));
                    redoStack.Clear();
                }
                objects.Add(new ObjectInfo(ObjectInfo.Type.Fire, a, b));
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

                if (verbose) Debug.Log("Remove object at (" + a + ", " + b + ")");
                //if (commitAction && !objects.Exists(i => i.x == a && i.y == b))
                //    Debug.LogError("Editor invalid: null in Removing object");
                if (commitAction)
                {
                    undoStack.Add(new EditActionInfo(objects.Find(i => i.x == a && i.y == b), null));
                    redoStack.Clear();
                }
                objects.Remove(objects.Find(i => i.x == a && i.y == b));
                hasChanged = true;
                break;
                #endregion
        }

        // Map Rendering
        mm.Initialize(sizeX, sizeY, walls, objects, "");

        return hasChanged;
    }

    public void EditModeChange(int buttonNum)
    {
        if (editMode == EditMode.Simulation) return;
        foreach (Button b in editorButtons)
        {
            if (b != null) b.interactable = true;
        }
        editorButtons[buttonNum].interactable = false;
        editMode = (EditMode)buttonNum;
    }

    public void EditReset()
    {
        // TODO: 경고 메시지 띄우기
        foreach (Button b in editorButtons)
        {
            if (b != null) b.interactable = true;
        }
        editMode = EditMode.None;

        List<WallInfo> oldWalls = walls;
        List<ObjectInfo> oldObjects = objects;
        walls = new List<WallInfo>();
        objects = new List<ObjectInfo>();
        mm.Initialize(sizeX, sizeY, walls, objects, "");

        undoStack.Add(new EditActionInfo(oldWalls, oldObjects));
        redoStack.Clear();
    }

    public void EditQuit()
    {
        // TODO: 경고 메시지 띄우기
        GameManager.gm.ReturnToMain();
    }

    private void EditSizeX(int newSizeX)
    {
        if (newSizeX != 0)
        {
            editorSizeXDropdown.SetValueWithoutNotify(newSizeX - MapManager.MIN_SIZE_X);
        }

        int value = editorSizeXDropdown.value + MapManager.MIN_SIZE_X;
        if (value < MapManager.MIN_SIZE_X || value > MapManager.MAX_SIZE_X) return;
        int oldValue = sizeX;

        sizeX = value;

        List<WallInfo> removedWalls = new List<WallInfo>();
        List<ObjectInfo> removedObjects = new List<ObjectInfo>();
        List<WallInfo> tempWalls = null;
        List<ObjectInfo> tempObjects = null;

        if (oldValue > value)
        {
            tempWalls = walls.FindAll(w => w.x > value);
            removedWalls.AddRange(tempWalls);
            walls.RemoveAll(w => w.x > value);

            tempWalls = walls.FindAll(w => w.type == WallInfo.Type.Vertical && w.x == value);
            removedWalls.AddRange(tempWalls);
            walls.RemoveAll(w => w.type == WallInfo.Type.Vertical && w.x == value);

            tempObjects = objects.FindAll(o => o.x > value);
            removedObjects.AddRange(tempObjects);
            objects.RemoveAll(o => o.x > value);
        }
        tempWalls = walls.FindAll(w => w.type == WallInfo.Type.ExitVertical && !(w.x == 0 || w.x == value));
        removedWalls.AddRange(tempWalls);
        walls.RemoveAll(w => w.type == WallInfo.Type.ExitVertical && !(w.x == 0 || w.x == value));

        if (newSizeX == 0)
        {
            undoStack.Add(new EditActionInfo(true, oldValue, value, removedWalls, removedObjects));
            redoStack.Clear();
        }

        mm.Initialize(sizeX, sizeY, walls, objects, "");
    }

    public void EditSizeX()
    {
        EditSizeX(0);
    }

    private void EditSizeY(int newSizeY)
    {
        if (newSizeY != 0)
        {
            editorSizeYDropdown.SetValueWithoutNotify(newSizeY - MapManager.MIN_SIZE_Y);
        }

        int value = editorSizeYDropdown.value + MapManager.MIN_SIZE_Y;
        if (value < MapManager.MIN_SIZE_Y || value > MapManager.MAX_SIZE_Y) return;
        int oldValue = sizeY;

        sizeY = value;

        List<WallInfo> removedWalls = new List<WallInfo>();
        List<ObjectInfo> removedObjects = new List<ObjectInfo>();
        List<WallInfo> tempWalls = null;
        List<ObjectInfo> tempObjects = null;

        if (oldValue > value)
        {
            tempWalls = walls.FindAll(w => w.y > value);
            removedWalls.AddRange(tempWalls);
            walls.RemoveAll(w => w.y > value);

            tempWalls = walls.FindAll(w => w.type == WallInfo.Type.Horizontal && w.y == value);
            removedWalls.AddRange(tempWalls);
            walls.RemoveAll(w => w.type == WallInfo.Type.Horizontal && w.y == value);

            tempObjects = objects.FindAll(o => o.y > value);
            removedObjects.AddRange(tempObjects);
            objects.RemoveAll(o => o.y > value);
        }
        tempWalls = walls.FindAll(w => w.type == WallInfo.Type.ExitHorizontal && !(w.y == 0 || w.y == value));
        removedWalls.AddRange(tempWalls);
        walls.RemoveAll(w => w.type == WallInfo.Type.ExitHorizontal && !(w.y == 0 || w.y == value));

        if (newSizeY == 0)
        {
            undoStack.Add(new EditActionInfo(false, oldValue, value, removedWalls, removedObjects));
            redoStack.Clear();
        }

        mm.Initialize(sizeX, sizeY, walls, objects, "");
    }

    public void EditSizeY()
    {
        EditSizeY(0);
    }

    private void EditMapName(string newName)
    {
        string oldMapName = mapName;
        if (newName != null)
        {
            editorMapNameInput.text = newName;
        }
        mapName = editorMapNameInput.text;
        Debug.Log("Map name changed: " + mapName);
        /*
        // Undoing or redoing map name change is disabled.
        if (newName == null)
        {
            undoStack.Add(new EditActionInfo(oldMapName, mapName));
            redoStack.Clear();
        }
        */
    }

    public void EditMapName()
    {
        EditMapName(null);
    }

    public void EditUndo()
    {
        if (undoStack.Count == 0) return;
        EditActionInfo eai = undoStack[undoStack.Count - 1];
        const bool verbose = false;
        string verboseMessage = "";

        switch (eai.type)
        {
            case EditActionInfo.Type.MapName:
                EditMapName(eai.oldName);
                break;
            case EditActionInfo.Type.Wall:
                #region Undo Wall
                if (eai.oldWall != null && !walls.Contains(eai.oldWall))
                {
                    walls.Add(eai.oldWall);
                    if (verbose)
                        verboseMessage += eai.oldWall.type.ToString() + " at (" + eai.oldWall.x + ", " + eai.oldWall.y + ")";
                }
                else if (verbose)
                    verboseMessage += "null";

                if (verbose)
                    verboseMessage += " <- ";

                if (eai.newWall != null && walls.Contains(eai.newWall))
                {
                    walls.Remove(eai.newWall);
                    if (verbose)
                        verboseMessage += eai.newWall.type.ToString() + " at (" + eai.newWall.x + ", " + eai.newWall.y + ")";
                }
                else if (verbose)
                    verboseMessage += "null";

                if (verbose)
                    Debug.Log(verboseMessage);

                mm.Initialize(sizeX, sizeY, walls, objects, "");
                #endregion
                break;
            case EditActionInfo.Type.Object:
                #region Undo Object
                if (eai.oldObject != null && !objects.Contains(eai.oldObject))
                {
                    objects.Add(eai.oldObject);
                    if (verbose)
                        verboseMessage += eai.oldObject.type.ToString() + " at (" + eai.oldObject.x + ", " + eai.oldObject.y + ")";
                }
                else if (verbose)
                    verboseMessage += "null";

                if (verbose)
                    verboseMessage += " <- ";

                if (eai.newObject != null && objects.Contains(eai.newObject))
                {
                    objects.Remove(eai.newObject);
                    if (verbose)
                        verboseMessage += eai.newObject.type.ToString() + " at (" + eai.newObject.x + ", " + eai.newObject.y + ")";
                }
                else if (verbose)
                    verboseMessage += "null";

                if (verbose)
                    Debug.Log(verboseMessage);

                mm.Initialize(sizeX, sizeY, walls, objects, "");
                #endregion
                break;
            case EditActionInfo.Type.SizeX:
                EditSizeX(eai.oldSize);
                walls.AddRange(eai.oldWalls);
                objects.AddRange(eai.oldObjects);
                mm.Initialize(sizeX, sizeY, walls, objects, "");
                break;
            case EditActionInfo.Type.SizeY:
                EditSizeY(eai.oldSize);
                walls.AddRange(eai.oldWalls);
                objects.AddRange(eai.oldObjects);
                mm.Initialize(sizeX, sizeY, walls, objects, "");
                break;
            case EditActionInfo.Type.MassRemoval:
                walls.AddRange(eai.oldWalls);
                objects.AddRange(eai.oldObjects);
                mm.Initialize(sizeX, sizeY, walls, objects, "");
                break;
        }
        undoStack.RemoveAt(undoStack.Count - 1);
        redoStack.Add(eai);
    }

    public void EditRedo()
    {
        if (redoStack.Count == 0) return;
        EditActionInfo eai = redoStack[redoStack.Count - 1];
        const bool verbose = false;
        string verboseMessage = "";

        switch (eai.type)
        {
            case EditActionInfo.Type.MapName:
                EditMapName(eai.newName);
                break;
            case EditActionInfo.Type.Wall:
                #region Redo Wall
                if (eai.oldWall != null && walls.Contains(eai.oldWall))
                {
                    walls.Remove(eai.oldWall);
                    if (verbose)
                        verboseMessage += eai.oldWall.type.ToString() + " at (" + eai.oldWall.x + ", " + eai.oldWall.y + ")";
                }
                else if (verbose)
                    verboseMessage += "null";

                if (verbose)
                    verboseMessage += " -> ";

                if (eai.newWall != null && !walls.Contains(eai.newWall))
                {
                    walls.Add(eai.newWall);
                    if (verbose)
                        verboseMessage += eai.newWall.type.ToString() + " at (" + eai.newWall.x + ", " + eai.newWall.y + ")";
                }
                else if (verbose)
                    verboseMessage += "null";

                if (verbose)
                    Debug.Log(verboseMessage);

                mm.Initialize(sizeX, sizeY, walls, objects, "");
                #endregion
                break;
            case EditActionInfo.Type.Object:
                #region Redo Object
                if (eai.oldObject != null && objects.Contains(eai.oldObject))
                {
                    objects.Remove(eai.oldObject);
                    if (verbose)
                        verboseMessage += eai.oldObject.type.ToString() + " at (" + eai.oldObject.x + ", " + eai.oldObject.y + ")";
                }
                else if (verbose)
                    verboseMessage += "null";

                if (verbose)
                    verboseMessage += " -> ";

                if (eai.newObject != null && !objects.Contains(eai.newObject))
                {
                    objects.Add(eai.newObject);
                    if (verbose)
                        verboseMessage += eai.newObject.type.ToString() + " at (" + eai.newObject.x + ", " + eai.newObject.y + ")";
                }
                else if (verbose)
                    verboseMessage += "null";

                if (verbose)
                    Debug.Log(verboseMessage);

                mm.Initialize(sizeX, sizeY, walls, objects, "");
                #endregion
                break;
            case EditActionInfo.Type.SizeX:
                EditSizeX(eai.newSize);
                mm.Initialize(sizeX, sizeY, walls, objects, "");
                break;
            case EditActionInfo.Type.SizeY:
                EditSizeY(eai.newSize);
                mm.Initialize(sizeX, sizeY, walls, objects, "");
                break;
            case EditActionInfo.Type.MassRemoval:
                // TODO
                foreach (WallInfo wi in eai.oldWalls)
                    walls.Remove(wi);
                foreach (ObjectInfo oi in eai.oldObjects)
                    objects.Remove(oi);
                mm.Initialize(sizeX, sizeY, walls, objects, "");
                break;
        }
        redoStack.RemoveAt(redoStack.Count - 1);
        undoStack.Add(eai);
    }

    private class EditActionInfo
    {
        public enum Type { MapName, SizeX, SizeY, Wall, Object, MassRemoval }

        public Type type;

        // MapName
        public string oldName;
        public string newName;

        // SizeX, SizeY
        public int oldSize;
        public int newSize;

        // Wall
        public WallInfo oldWall;
        public WallInfo newWall;

        // Object
        public ObjectInfo oldObject;
        public ObjectInfo newObject;

        // SizeX, SizeY, MassRemoval
        public List<WallInfo> oldWalls;
        public List<ObjectInfo> oldObjects;

        /// <summary>
        /// Type: MapName
        /// </summary>
        /// <param name="oldMapName">없으면 ""</param>
        /// <param name="newMapName">없으면 ""</param>
        public EditActionInfo(string oldMapName, string newMapName)
        {
            type = Type.MapName;
            oldName = oldMapName;
            newName = newMapName;
        }

        /// <summary>
        /// Type: SizeX, SizeY
        /// </summary>
        /// <param name="isX">sizeX 변경 시 true, sizeY 변경 시 false</param>
        /// <param name="oldSize"></param>
        /// <param name="newSize"></param>
        public EditActionInfo(bool isX, int oldSize, int newSize,
            List<WallInfo> oldRemovedWalls = null, List <ObjectInfo> oldRemovedObjects = null)
        {
            if (isX)
                type = Type.SizeX;
            else
                type = Type.SizeY;
            this.oldSize = oldSize;
            this.newSize = newSize;
            if (oldRemovedWalls is null)
                oldWalls = new List<WallInfo>();
            else
                oldWalls = oldRemovedWalls;
            if (oldRemovedObjects is null)
                oldObjects = new List<ObjectInfo>();
            else
                oldObjects = oldRemovedObjects;
        }

        /// <summary>
        /// Type: Wall
        /// </summary>
        /// <param name="oldWallInfo">없으면 null</param>
        /// <param name="newWallInfo">없으면 null</param>
        public EditActionInfo(WallInfo oldWallInfo, WallInfo newWallInfo)
        {
            type = Type.Wall;
            oldWall = oldWallInfo;
            newWall = newWallInfo;
        }

        /// <summary>
        /// Type: Object
        /// </summary>
        /// <param name="oldObjectInfo">없으면 null</param>
        /// <param name="newObjectInfo">없으면 null</param>
        public EditActionInfo(ObjectInfo oldObjectInfo, ObjectInfo newObjectInfo)
        {
            type = Type.Object;
            oldObject = oldObjectInfo;
            newObject = newObjectInfo;
        }

        /// <summary>
        /// Type: MassRemoval (Reset, ...)
        /// </summary>
        /// <param name="oldRemovedWalls">없으면 null</param>
        /// <param name="oldRemovedObjects">없으면 null</param>
        public EditActionInfo(List<WallInfo> oldRemovedWalls, List<ObjectInfo> oldRemovedObjects)
        {
            type = Type.MassRemoval;
            if (oldRemovedWalls is null)
                oldWalls = new List<WallInfo>();
            else
                oldWalls = oldRemovedWalls;
            if (oldRemovedObjects is null)
                oldObjects = new List<ObjectInfo>();
            else
                oldObjects = oldRemovedObjects;
        }
    }
}