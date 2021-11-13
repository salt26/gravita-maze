using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EditorManager : MonoBehaviour
{
    public enum EditMode { None, Wall, Exit, RemoveWall, Ball, Iron, Fire, RemoveObject }
    public enum EditPhase { Initialize = 1, Build = 2, Save = 3, Test = 4 }

    public Camera mainCamera;
    public Grid grid;
    public MapManager mm;
    public List<Button> editorModeButtons;
    public Button editorNewButton;
    public Button editorResetButton;
    public Button editorUndoButton;
    public Button editorRedoButton;
    public Button editorNextButton1;
    public Button editorNextButton2;
    public Button editorNextButton4;
    public Button editorBackButton2;
    public Button editorBackButton3;
    public Button editorBackButton4;
    public Button editorBackHighlightedButton4;
    public Button editorRetryButton;
    public Button editorRetryHighlightedButton;
    public Button editorMapTestRequiredButton;
    public Button editorMapTestDoneButton;
    public Button editorSaveButton;
    public Button editorQuitButton3;
    public List<Dropdown> editorSizeXDropdowns;
    public List<Dropdown> editorSizeYDropdowns;
    public List<InputField> editorMapNameInputs;
    public List<GameObject> editorPhases;

    public EditPhase editPhase = EditPhase.Initialize;
    public EditMode editMode = EditMode.None;

    private int sizeX;
    private int sizeY;
    private List<WallInfo> walls = new List<WallInfo>();
    private List<ObjectInfo> objects = new List<ObjectInfo>();
    private string solution = "";
    private string mapName = "";
    private bool hasCreated = false;
    [SerializeField]
    private bool dirtyBit = false;
    private bool hasSavedOnce = false;
    private bool isSaving = false;

    private int currentTouchX;
    private int currentTouchY;
    private List<WallInfo> tempWalls;
    private List<ObjectInfo> tempObjects;

    private List<EditActionInfo> undoStack = new List<EditActionInfo>();
    private List<EditActionInfo> redoStack = new List<EditActionInfo>();

    // Start is called before the first frame update
    void Start()
    {

        sizeX = Mathf.Clamp(editorSizeXDropdowns[0].value + MapManager.MIN_SIZE_X, MapManager.MIN_SIZE_X, MapManager.MAX_SIZE_X);
        sizeY = Mathf.Clamp(editorSizeYDropdowns[0].value + MapManager.MIN_SIZE_Y, MapManager.MIN_SIZE_Y, MapManager.MAX_SIZE_Y);

        editorPhases[0].SetActive(true);
        editorPhases[1].SetActive(false);
        editorPhases[2].SetActive(false);
        editorPhases[3].SetActive(false);
        SetEditModeToNone();
        editPhase = EditPhase.Initialize;
        hasCreated = false;
        hasSavedOnce = false;
        dirtyBit = false;
        GameManager.gm.canPlay = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (editPhase == EditPhase.Build)
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
        }

        // Phase 1 buttons
        if (hasCreated && walls.Count == 0 && objects.Count == 0)
        {
            editorNewButton.interactable = false;
            editorResetButton.interactable = false;
        }
        else if (hasCreated)
        {
            editorNewButton.interactable = true;
            editorResetButton.interactable = true;
        }
        else
        {
            editorNewButton.interactable = true;
        }

        // Phase 2 buttons
        if (walls.Exists(e => e.type == WallInfo.Type.ExitHorizontal || e.type == WallInfo.Type.ExitVertical) &&
            objects.Exists(e => e.type == ObjectInfo.Type.Ball))
        {
            editorNextButton2.interactable = true;
        }
        else
        {
            editorNextButton2.interactable = false;
        }

        editorUndoButton.interactable = undoStack.Count > 0;
        editorRedoButton.interactable = redoStack.Count > 0;

        // Phase 3 buttons
        editorSaveButton.interactable = solution != null && solution != "" && mapName != null && mapName != "" && dirtyBit;
        editorQuitButton3.interactable = solution != null && solution != "" && hasSavedOnce;

        if (solution != null && solution != "")
        {
            editorMapTestRequiredButton.gameObject.SetActive(false);
            editorMapTestDoneButton.gameObject.SetActive(true);
        }
        else
        {
            editorMapTestRequiredButton.gameObject.SetActive(true);
            editorMapTestDoneButton.gameObject.SetActive(false);
        }
    }

    bool TouchMap(float x, float y, EditMode editMode, List<WallInfo> walls, List<ObjectInfo> objects, int touchID,
        bool commitAction = false, bool verbose = false)
    {
        if (touchID != 0 && touchID != -1) return false;
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
                        solution = "";
                        dirtyBit = true;
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
                        solution = "";
                        dirtyBit = true;
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
                            solution = "";
                            dirtyBit = true;
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
                            solution = "";
                            dirtyBit = true;
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
                            solution = "";
                            dirtyBit = true;
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
                            solution = "";
                            dirtyBit = true;
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
                            solution = "";
                            dirtyBit = true;
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
                            solution = "";
                            dirtyBit = true;
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
                            solution = "";
                            dirtyBit = true;
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
                            solution = "";
                            dirtyBit = true;
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
                            solution = "";
                            dirtyBit = true;
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
                            solution = "";
                            dirtyBit = true;
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
                            solution = "";
                            dirtyBit = true;
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
                            solution = "";
                            dirtyBit = true;
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
                        solution = "";
                        dirtyBit = true;
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
                        solution = "";
                        dirtyBit = true;
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
                        solution = "";
                        dirtyBit = true;
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
                        solution = "";
                        dirtyBit = true;
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
                        solution = "";
                        dirtyBit = true;
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
                        solution = "";
                        dirtyBit = true;
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
                        solution = "";
                        dirtyBit = true;
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
                        solution = "";
                        dirtyBit = true;
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
                    solution = "";
                    dirtyBit = true;
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
                    solution = "";
                    dirtyBit = true;
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
                    solution = "";
                    dirtyBit = true;
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
        foreach (Button b in editorModeButtons)
        {
            if (b != null) b.interactable = true;
        }
        editorModeButtons[buttonNum].interactable = false;
        editMode = (EditMode)buttonNum;
    }

    public void EditReset()
    {
        // TODO: 경고 메시지 띄우기
        SetEditModeToNone();

        List<WallInfo> oldWalls = walls;
        List<ObjectInfo> oldObjects = objects;
        walls = new List<WallInfo>();
        objects = new List<ObjectInfo>();
        mm.Initialize(sizeX, sizeY, walls, objects, "");

        undoStack.Add(new EditActionInfo(oldWalls, oldObjects));
        redoStack.Clear();
        solution = "";
        if (hasCreated)
            dirtyBit = true;
    }

    public void EditQuit()
    {
        if (dirtyBit)
        {
            // TODO: 경고 메시지 띄우기
        }
        GameManager.gm.ReturnToMain();
    }

    private void EditSizeX(int newSizeX, Dropdown valueChangedDropdown = null)
    {
        int value;
        if (newSizeX != 0 || valueChangedDropdown == null)
        {
            value = newSizeX;
            foreach (Dropdown editorSizeXDropdown in editorSizeXDropdowns)
                editorSizeXDropdown.SetValueWithoutNotify(value - MapManager.MIN_SIZE_X);
        }
        else
        {
            value = editorSizeXDropdowns.Find(e => e.Equals(valueChangedDropdown)).value + MapManager.MIN_SIZE_X;
            foreach (Dropdown editorSizeXDropdown in editorSizeXDropdowns)
                editorSizeXDropdown.SetValueWithoutNotify(value - MapManager.MIN_SIZE_X);
        }
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
            solution = "";
            dirtyBit = true;
        }

        mm.Initialize(sizeX, sizeY, walls, objects, "");
    }

    public void EditSizeX(Dropdown caller)
    {
        EditSizeX(0, caller);
    }

    private void EditSizeY(int newSizeY, Dropdown valueChangedDropdown = null)
    {
        int value;
        if (newSizeY != 0 || valueChangedDropdown == null)
        {
            value = newSizeY;
            foreach (Dropdown editorSizeYDropdown in editorSizeYDropdowns)
                editorSizeYDropdown.SetValueWithoutNotify(value - MapManager.MIN_SIZE_Y);
        }
        else
        {
            value = editorSizeYDropdowns.Find(e => e.Equals(valueChangedDropdown)).value + MapManager.MIN_SIZE_Y;
            foreach (Dropdown editorSizeYDropdown in editorSizeYDropdowns)
                editorSizeYDropdown.SetValueWithoutNotify(value - MapManager.MIN_SIZE_Y);
        }
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
            solution = "";
            dirtyBit = true;
        }

        mm.Initialize(sizeX, sizeY, walls, objects, "");
    }

    public void EditSizeY(Dropdown caller)
    {
        EditSizeY(0, caller);
    }

    private void EditMapName(string newName, InputField valueChangedInputField = null)
    {
        string oldMapName = mapName;
        if (newName != null || valueChangedInputField == null)
        {
            mapName = newName;
            foreach (InputField editorMapNameInput in editorMapNameInputs)
                editorMapNameInput.text = mapName;
        }
        else
        {
            mapName = editorMapNameInputs.Find(e => e.Equals(valueChangedInputField)).text;
            foreach (InputField editorMapNameInput in editorMapNameInputs)
                editorMapNameInput.text = mapName;
        }

        /*
        // Undoing or redoing map name change is disabled.
        if (newName == null)
        {
            undoStack.Add(new EditActionInfo(oldMapName, mapName));
            redoStack.Clear();
        }
        */

        if (!oldMapName.Equals(mapName))
        {
            Debug.Log("Map name changed: " + mapName);
            dirtyBit = true;
        }
    }

    public void EditMapName(InputField caller)
    {
        EditMapName(null, caller);
    }

    public void EditNew()
    {
        if (editPhase != EditPhase.Initialize) return;

        // TODO 확인 메시지
        editorMapNameInputs[0].interactable = true;
        editorSizeXDropdowns[0].interactable = true;
        editorSizeYDropdowns[0].interactable = true;
        editorNextButton1.interactable = true;
        EditMapName("");
        EditReset();
        hasCreated = true;
    }

    public void EditOpen()
    {
        // TODO
        // ParseMapText(text) 사용하기
    }

    public void EditSave()
    {
        /*
        Debug.Log("isSaving = " + isSaving.ToString() + ", solution = " + solution + ", mapName = " + mapName +
            " dirtyBit = " + dirtyBit.ToString() + ", mm.IsReady = " + mm.IsReady.ToString());
        */
        if (solution == null || solution == "" || mapName == null || mapName == "" || !dirtyBit ||
            mm == null || !mm.IsReady || editPhase != EditPhase.Save || isSaving) return;

        isSaving = true;

        if (!ValidateMapInGame())
        {
            Debug.LogError("Editor invalid: map validation failed");
            isSaving = false;
            return;
        }

        try
        {
            if (!Directory.Exists("Maps"))
            {
                Directory.CreateDirectory("Maps");
            }
        }
        catch (Exception)
        {
            Debug.LogError("Editor invalid: exception while creating a directory");
            isSaving = false;
            throw;
        }

        try
        {
            if (File.Exists(@"Maps\" + mapName + ".txt"))
            {
                // TODO 같은 이름의 파일이 있는데 그래도 저장할 것인지 메시지로 물어보기
                Debug.LogWarning("Map \"" + mapName + "\" already exists. Do you want to overwrite it?");
                isSaving = false; // 이것도 지우기
                return; // return은 지우기
            }
        }
        catch (Exception)
        {
            Debug.LogError("Editor invalid: exception while checking a file");
            isSaving = false;
            throw;
        }

        FileStream fs = new FileStream(@"Maps\" + mapName + ".txt", FileMode.Create);
        StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);

        try
        {
            sw.WriteLine(sizeX + " " + sizeY);
            foreach (ObjectInfo o in objects)
            {
                switch (o.type)
                {
                    case ObjectInfo.Type.Ball:
                        sw.WriteLine("@ " + o.x + " " + o.y);
                        break;
                    case ObjectInfo.Type.Iron:
                        sw.WriteLine("# " + o.x + " " + o.y);
                        break;
                    case ObjectInfo.Type.Fire:
                        sw.WriteLine("* " + o.x + " " + o.y);
                        break;
                }
            }
            foreach (WallInfo w in walls)
            {
                switch (w.type)
                {
                    case WallInfo.Type.Horizontal:
                        sw.WriteLine("- " + w.x + " " + w.y);
                        break;
                    case WallInfo.Type.Vertical:
                        sw.WriteLine("| " + w.x + " " + w.y);
                        break;
                    case WallInfo.Type.ExitHorizontal:
                        sw.WriteLine("$ - " + w.x + " " + w.y);
                        break;
                    case WallInfo.Type.ExitVertical:
                        sw.WriteLine("$ | " + w.x + " " + w.y);
                        break;
                }
            }
            sw.WriteLine(solution);
        }
        catch (Exception)
        {
            Debug.LogError("Editor invalid: exception while saving a map");
            isSaving = false;
            throw;
        }
        finally
        {
            sw.Close();
            fs.Close();
        }

        // TODO 저장 성공 메시지
        Debug.Log("Saved as " + mapName + "!");

        dirtyBit = false;
        hasSavedOnce = true;
        isSaving = false;
    }

    public void EditNext()
    {
        switch (editPhase)
        {
            case EditPhase.Initialize:
                // TODO 전환 애니메이션
                editorPhases[0].SetActive(false);
                editorPhases[1].SetActive(true);
                SetEditModeToNone();
                editPhase = EditPhase.Build;
                GameManager.gm.canPlay = false;
                break;
            case EditPhase.Build:
                editorPhases[1].SetActive(false);
                editorPhases[2].SetActive(true);
                editPhase = EditPhase.Save;
                GameManager.gm.canPlay = false;
                break;
            case EditPhase.Save:
                editorPhases[2].SetActive(false);
                editorPhases[3].SetActive(true);
                editPhase = EditPhase.Test;
                mm.afterGravity = EditorAfterGravity;
                EditorAfterGravity(MapManager.Flag.Continued);
                GameManager.gm.canPlay = true;
                break;
            case EditPhase.Test:
                // Validation finished
                editorPhases[3].SetActive(false);
                editorPhases[2].SetActive(true);
                editPhase = EditPhase.Save;
                solution = mm.ActionHistory;
                print(solution);
                mm.Initialize(sizeX, sizeY, walls, objects, solution);
                GameManager.gm.canPlay = false;
                break;
        }
    }

    public void EditBack()
    {
        switch (editPhase)
        {
            case EditPhase.Build:
                editorPhases[1].SetActive(false);
                editorPhases[0].SetActive(true);
                editPhase = EditPhase.Initialize;
                GameManager.gm.canPlay = false;
                break;
            case EditPhase.Save:
                editorPhases[2].SetActive(false);
                editorPhases[1].SetActive(true);
                editPhase = EditPhase.Build;
                SetEditModeToNone();
                GameManager.gm.canPlay = false;
                break;
            case EditPhase.Test:
                editorPhases[3].SetActive(false);
                editorPhases[1].SetActive(true);
                editPhase = EditPhase.Build;
                SetEditModeToNone();
                mm.Initialize(sizeX, sizeY, walls, objects, solution);
                GameManager.gm.canPlay = false;
                break;
        }
    }

    public void EditorAfterGravity(MapManager.Flag flag)
    {
        switch (flag)
        {
            case MapManager.Flag.Continued:
                editorBackHighlightedButton4.gameObject.SetActive(false);
                editorBackButton4.gameObject.SetActive(true);

                editorRetryHighlightedButton.gameObject.SetActive(false);
                editorRetryButton.gameObject.SetActive(true);

                editorNextButton4.interactable = false;
                editorBackButton4.interactable = true;
                break;
            case MapManager.Flag.Escaped:
                editorBackHighlightedButton4.gameObject.SetActive(false);
                editorBackButton4.gameObject.SetActive(true);

                editorRetryHighlightedButton.gameObject.SetActive(false);
                editorRetryButton.gameObject.SetActive(true);

                editorNextButton4.interactable = true;
                editorBackButton4.interactable = false;
                break;
            case MapManager.Flag.Burned:
            case MapManager.Flag.Squashed:
                editorBackButton4.gameObject.SetActive(false);
                editorBackHighlightedButton4.gameObject.SetActive(true);

                editorRetryButton.gameObject.SetActive(false);
                editorRetryHighlightedButton.gameObject.SetActive(true);

                editorNextButton4.interactable = false;
                editorBackHighlightedButton4.interactable = true;
                break;
        }
    }

#pragma warning disable CS0162 // 접근할 수 없는 코드가 있습니다.
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
        solution = "";
        dirtyBit = true;
    }
#pragma warning restore CS0162 // 접근할 수 없는 코드가 있습니다.

#pragma warning disable CS0162 // 접근할 수 없는 코드가 있습니다.
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
        solution = "";
        dirtyBit = true;
    }
#pragma warning restore CS0162 // 접근할 수 없는 코드가 있습니다.


    private void SetEditModeToNone()
    {
        foreach (Button b in editorModeButtons)
        {
            if (b != null) b.interactable = true;
        }
        editMode = EditMode.None;
    }

    private bool ValidateMapInGame()
    {
        /*
        if (sizeX < MapManager.MIN_SIZE_X || sizeX > MapManager.MAX_SIZE_X ||
            sizeY < MapManager.MIN_SIZE_Y || sizeY > MapManager.MAX_SIZE_Y) return false;
        if (objects.FindAll(e => e.type == ObjectInfo.Type.Ball).Count != 1) return false;
        foreach (ObjectInfo o in objects)
        {
            if (o.x < 1 || o.x > sizeX || o.y < 1 || o.y > sizeY) return false;
        }
        */
        mm.Initialize(sizeX, sizeY, walls, objects, solution, true);
        return mm.IsReady;
    }

    private bool ParseMapText(string text)
    {
        // TODO
        // 파일 유효성 검사는 파일 텍스트의 기호나 포맷만 본다.
        // 숫자(좌표 등), 중복 좌표, Ball과 Exit의 유일성 등은 VailidationMapInGame()으로 확인하자.

        // 근데 생각해 보면 유효성을 보기만 할 게 아니라 파싱까지 하면 되잖아?
        return false;
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