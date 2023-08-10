using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

public class EditorManager : MonoBehaviour
{
    public string tableName = "StringTable";

    public enum EditMode { None, Wall, Exit, RemoveWall, Ball, Iron, Fire, RemoveObject, Shutter, Hole }
    public enum EditPhase { Initialize = 1, Build = 2, Request = 3, Test = 4, Open = 5, Save = 6 }

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
    public Button editorRetryTimeButton;
    public Button editorRetryTimeHighlightedButton;
    public Button editorMapTestRequiredButton;
    public Button editorMapTestDoneButton;
    public Button editorSaveButton3;
    public Button editorQuitButton3;
    public Button editorQuitHighlightedButton3;
    public Button editorCancelButton5;
    public Button editorOpenButton5;
    public Button editorOpenHighlightedButton5;
    public Text editorOpenPathText;
    public Button editorCancelButton6;
    public Button editorSaveButton6;
    public Button editorOpenButton6;
    public Button editorNewFolderButton6;
    public Text editorSavePathText;
    public ButtonWithText editorSavePathButton;
    public List<Dropdown> editorSizeXDropdowns;
    public List<Dropdown> editorSizeYDropdowns;
    public List<InputField> editorMapNameInputs;
    public GameObject openScrollItemPrefab;
    public GameObject editorOpenScrollContent;
    public Scrollbar editorOpenScrollbar;
    public GameObject editorOpenScrollEmptyText;
    public GameObject saveScrollItemPrefab;
    public GameObject editorSaveScrollContent;
    public Scrollbar editorSaveScrollbar;
    public GameObject editorSaveScrollEmptyText;
    public TimerUI timerUI;
    public Slider editorTimerSlider;
    public Image editorTimerLabel10;
    public Image editorTimerLabel1;
    public StatusUI statusUI;
    public MessageUI messageUI;
    public InputMessageUI inputMessageUI;
    public GameObject tooltipUI;
    public List<GameObject> editorPhases;

    public EditPhase editPhase = EditPhase.Initialize;
    public EditMode editMode = EditMode.None;

    private int sizeX;
    private int sizeY;
    private List<WallInfo> walls = new List<WallInfo>();
    private List<ObjectInfo> objects = new List<ObjectInfo>();
    private string solution = "";
    private string mapName = "";
    private float timeLimit = MapManager.DEFAULT_TIME_LIMIT;
    private bool hasCreated = false;
    [SerializeField]
    private bool dirtyBit = false;
    private bool hasSavedOnce = false;
    private bool isSaving = false;
    private bool hasPassedInitPhaseOnce = false;
    private OpenSaveScrollItem selectedOpenScrollItem;
    private string currentOpenPath = MapManager.MAP_ROOT_PATH;
    private float openItemSelectTime = 0f;
    private OpenSaveScrollItem selectedSaveScrollItem;
    private string currentSavePath = MapManager.MAP_ROOT_PATH;
    private float saveItemSelectTime = 0f;
    private string folderName = "";
    private TooltipHover editorSaveButton3Hover;
    private TooltipHover editorQuitButton3Hover;
    

    private int currentTouchX;
    private int currentTouchY;
    private List<WallInfo> tempWalls;
    private List<ObjectInfo> tempObjects;

    private List<EditActionInfo> undoStack = new List<EditActionInfo>();
    private List<EditActionInfo> redoStack = new List<EditActionInfo>();

    // Start is called before the first frame update
    void Start()
    {
        editorSaveButton3Hover = editorSaveButton3.GetComponent<TooltipHover>();
        editorQuitButton3Hover = editorQuitButton3.GetComponent<TooltipHover>();
        sizeX = Mathf.Clamp(editorSizeXDropdowns[0].value + MapManager.MIN_SIZE_X, MapManager.MIN_SIZE_X, MapManager.MAX_SIZE_X);
        sizeY = Mathf.Clamp(editorSizeYDropdowns[0].value + MapManager.MIN_SIZE_Y, MapManager.MIN_SIZE_Y, MapManager.MAX_SIZE_Y);

        statusUI.gameObject.SetActive(true);
        timerUI.gameObject.SetActive(false);
        messageUI.gameObject.SetActive(false);
        editorPhases[0].SetActive(true);
        editorPhases[1].SetActive(false);
        editorPhases[2].SetActive(false);
        editorPhases[3].SetActive(false);
        editorPhases[4].SetActive(false);
        editorPhases[5].SetActive(false);
        SetEditModeToNone();
        mm.Initialize();
        editPhase = EditPhase.Initialize;
        statusUI.SetStatusMessage(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_start_welcome"));
        hasCreated = false;
        hasSavedOnce = false;
        dirtyBit = false;
        hasPassedInitPhaseOnce = false;
        GameManager.gm.canPlay = false;
        foreach (var t in tooltipUI.GetComponentsInChildren<TooltipBox>())
        {
            Destroy(t.gameObject);
        }
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
                        tempWalls = walls.ConvertAll(j => new WallInfo(j.type, j.x, j.y));
                        tempObjects = objects.ConvertAll(j => new ObjectInfo(j.type, j.x, j.y));
                        if (editMode == EditMode.Exit)
                        {
                            if (tempWalls.Exists((j) => j.type == WallInfo.Type.ExitHorizontal || j.type == WallInfo.Type.ExitVertical))
                            {
                                tempWalls.Remove(tempWalls.Find((j) => j.type == WallInfo.Type.ExitHorizontal || j.type == WallInfo.Type.ExitVertical));
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
                        tempWalls = walls.ConvertAll(j => new WallInfo(j.type, j.x, j.y));
                        tempObjects = objects.ConvertAll(j => new ObjectInfo(j.type, j.x, j.y));
                        if (editMode == EditMode.Exit)
                        {
                            if (tempWalls.Exists((j) => j.type == WallInfo.Type.ExitHorizontal || j.type == WallInfo.Type.ExitVertical))
                            {
                                tempWalls.Remove(tempWalls.Find((j) => j.type == WallInfo.Type.ExitHorizontal || j.type == WallInfo.Type.ExitVertical));
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
            if (editPhase == EditPhase.Initialize && !hasPassedInitPhaseOnce)
            {
                statusUI.SetStatusMessage(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_update_begin"));
            }
        }
        else if (hasCreated)
        {
            editorNewButton.interactable = true;
            editorResetButton.interactable = true;
            if (editPhase == EditPhase.Initialize && !hasPassedInitPhaseOnce)
            {
                statusUI.SetStatusMessage(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_update_begin"));
            }
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
        editorSaveButton3.interactable = solution != null && solution != "" && dirtyBit;
        if (solution == null || solution == "")
        {
            editorSaveButton3Hover.tooltipMessage = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_update_test_prompt");
            editorSaveButton3Hover.tooltipWidth = 672f;
            editorSaveButton3Hover.tooltipHeight = 216f;
        }
        else if (!dirtyBit)
        {
            editorSaveButton3Hover.tooltipMessage = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_update_no_change");
            editorSaveButton3Hover.tooltipWidth = 480f;
            editorSaveButton3Hover.tooltipHeight = 264f;
        }

        if (solution != null && solution != "" && hasSavedOnce)
        {
            editorQuitButton3.gameObject.SetActive(false);
            editorQuitHighlightedButton3.gameObject.SetActive(true);
        }
        else
        {
            editorQuitButton3.gameObject.SetActive(true);
            editorQuitButton3.interactable = false;
            editorQuitHighlightedButton3.gameObject.SetActive(false);

            if (solution != null && solution != "")
            {
                editorQuitButton3Hover .tooltipMessage = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_update_save_prompt");
                editorQuitButton3Hover.tooltipWidth = 696f;
            }
            else
            {
                editorQuitButton3Hover .tooltipMessage = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_update_test_prompt");
                editorQuitButton3Hover.tooltipWidth = 672f;
            }
            //revise
        }

        if (solution != null && solution != "")
        {
            editorMapTestRequiredButton.gameObject.SetActive(false);
            editorMapTestDoneButton.gameObject.SetActive(true);

            if (editPhase == EditPhase.Request)
            {
                if (dirtyBit)
                {
                    statusUI.SetStatusMessage(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_update_save_ready"));
                }
            }
        }
        else
        {
            editorMapTestRequiredButton.gameObject.SetActive(true);
            editorMapTestDoneButton.gameObject.SetActive(false);

            if (editPhase == EditPhase.Request)
            {
                statusUI.SetStatusMessage(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_update_set_time"));
            }
        }

        // Phase 6 buttons
        editorSaveButton6.interactable = (mapName != null && mapName != "");
        editorSavePathButton.interactable = (selectedSaveScrollItem != null && selectedSaveScrollItem.isFolder);
        if (editPhase == EditPhase.Save)
        {
            if (mapName == null || mapName == "")
            {
                statusUI.SetStatusMessage(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_update_set_map_name"));
            }
            else
            {
                statusUI.SetStatusMessage(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_update_set_save_path"));
            }
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
                        if (verbose) Debug.LogWarning("Editor warning: horizontal wall position at (" + a + ", " + b + ")");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_add_wall"), 1f);
                        break;
                    }
                    if (walls.Contains(new WallInfo(WallInfo.Type.Horizontal, a, b)))
                    {
                        if (verbose) Debug.LogWarning("Editor warning: horizontal wall overlapped at (" + a + ", " + b + ")");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_add_wall"), 1f);
                        break;
                    }
                    if (objects.Exists(i => (i.type == ObjectInfo.Type.Hole && i.x == a && (i.y == b || i.y == b + 1))))
                    {
                        if (verbose) Debug.LogWarning("Editor warning: horizontal wall adjacent to hole");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_add_wall"), 1f);
                        break;
                    }

                    if (verbose) Debug.Log("Add horizontal wall at (" + a + ", " + b + ")");
                    if (commitAction)
                    {
                        if (walls.Contains(new WallInfo(WallInfo.Type.HorizontalShutter, a, b)))
                        {
                            walls.Remove(walls.Find((i) => (i.type == WallInfo.Type.HorizontalShutter) && i.x == a && i.y == b));
                            undoStack.Add(new EditActionInfo(new WallInfo(WallInfo.Type.HorizontalShutter, a, b), 
                                new WallInfo(WallInfo.Type.Horizontal, a, b)));
                        }
                        else
                        {
                            undoStack.Add(new EditActionInfo(null, new WallInfo(WallInfo.Type.Horizontal, a, b)));
                        }
                        redoStack.Clear();
                        solution = "";
                        dirtyBit = true;
                        GameManager.gm.PlayWallSFX();
                        walls.Add(new WallInfo(WallInfo.Type.Horizontal, a, b));
                        hasChanged = true;
                    }
                }
                else
                {
                    // Vertical wall
                    a = (Mathf.FloorToInt(x + y) - Mathf.CeilToInt(y - x)) / 2;
                    b = (Mathf.FloorToInt(x + y) + Mathf.CeilToInt(y - x)) / 2;

                    if (a < 1 || a > sizeX - 1 || b < 1 || b > sizeY)
                    {
                        if (verbose) Debug.LogWarning("Editor warning: vertical wall position at (" + a + ", " + b + ")");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_add_wall"), 1f);
                        break;
                    }
                    if (walls.Contains(new WallInfo(WallInfo.Type.Vertical, a, b)))
                    {
                        if (verbose) Debug.LogWarning("Editor warning: vertical wall overlapped at (" + a + ", " + b + ")");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_add_wall"), 1f);
                        break;
                    }
                    if (objects.Exists(i => (i.type == ObjectInfo.Type.Hole && (i.x == a || i.x == a + 1) && i.y == b)))
                    {
                        if (verbose) Debug.LogWarning("Editor warning: vertical wall adjacent to hole");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_add_wall"), 1f);
                        break;
                    }

                    if (verbose) Debug.Log("Add vertical wall at (" + a + ", " + b + ")");
                    if (commitAction)
                    {
                        if (walls.Contains(new WallInfo(WallInfo.Type.VerticalShutter, a, b)))
                        {
                            walls.Remove(walls.Find((i) => (i.type == WallInfo.Type.VerticalShutter) && i.x == a && i.y == b));
                            undoStack.Add(new EditActionInfo(new WallInfo(WallInfo.Type.VerticalShutter, a, b), 
                                new WallInfo(WallInfo.Type.Vertical, a, b)));
                        }
                        else
                        {
                            undoStack.Add(new EditActionInfo(null, new WallInfo(WallInfo.Type.Vertical, a, b)));
                        }
                        redoStack.Clear();
                        solution = "";
                        dirtyBit = true;
                        GameManager.gm.PlayWallSFX();
                        walls.Add(new WallInfo(WallInfo.Type.Vertical, a, b));
                        hasChanged = true;
                    }
                }
                break;
#endregion
            case EditMode.Shutter:
#region Shutter
                if ((Mathf.FloorToInt(x + y) - Mathf.FloorToInt(y - x)) % 2 == 0)
                {
                    // Horizontal shutter
                    a = (Mathf.FloorToInt(x + y) - Mathf.FloorToInt(y - x)) / 2;
                    b = (Mathf.FloorToInt(x + y) + Mathf.FloorToInt(y - x)) / 2;

                    if (a < 1 || a > sizeX || b < 1 || b > sizeY - 1)
                    {
                        if (verbose) Debug.LogWarning("Editor warning: horizontal shutter position at (" + a + ", " + b + ")");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_add_shutter"), 1f);
                        break;
                    }
                    if (walls.Contains(new WallInfo(WallInfo.Type.HorizontalShutter, a, b)))
                    {
                        if (verbose) Debug.LogWarning("Editor warning: horizontal shutter overlapped at (" + a + ", " + b + ")");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_add_shutter"), 1f);
                        break;
                    }
                    if (objects.Exists(i => (i.type == ObjectInfo.Type.Hole && i.x == a && (i.y == b || i.y == b + 1))))
                    {
                        if (verbose) Debug.LogWarning("Editor warning: horizontal shutter adjacent to hole");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_add_shutter"), 1f);
                        break;
                    }

                    if (verbose) Debug.Log("Add horizontal shutter at (" + a + ", " + b + ")");
                    if (commitAction)
                    {
                        if (walls.Contains(new WallInfo(WallInfo.Type.Horizontal, a, b)))
                        {
                            walls.Remove(walls.Find((i) => (i.type == WallInfo.Type.Horizontal) && i.x == a && i.y == b));
                            undoStack.Add(new EditActionInfo(new WallInfo(WallInfo.Type.Horizontal, a, b), 
                                new WallInfo(WallInfo.Type.HorizontalShutter, a, b)));
                        }
                        else
                        {
                            undoStack.Add(new EditActionInfo(null, new WallInfo(WallInfo.Type.HorizontalShutter, a, b)));
                        }
                        redoStack.Clear();
                        solution = "";
                        dirtyBit = true;
                        GameManager.gm.PlayShutterSFX();
                        walls.Add(new WallInfo(WallInfo.Type.HorizontalShutter, a, b));
                        hasChanged = true;
                    }
                }
                else
                {
                    // Vertical shutter
                    a = (Mathf.FloorToInt(x + y) - Mathf.CeilToInt(y - x)) / 2;
                    b = (Mathf.FloorToInt(x + y) + Mathf.CeilToInt(y - x)) / 2;

                    if (a < 1 || a > sizeX - 1 || b < 1 || b > sizeY)
                    {
                        if (verbose) Debug.LogWarning("Editor warning: vertical shutter position at (" + a + ", " + b + ")");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_add_shutter"), 1f);
                        break;
                    }
                    if (walls.Contains(new WallInfo(WallInfo.Type.VerticalShutter, a, b)))
                    {
                        if (verbose) Debug.LogWarning("Editor warning: vertical shutter overlapped at (" + a + ", " + b + ")");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_add_shutter"), 1f);
                        break;
                    }
                    if (objects.Exists(i => (i.type == ObjectInfo.Type.Hole && (i.x == a || i.x == a + 1) && i.y == b)))
                    {
                        if (verbose) Debug.LogWarning("Editor warning: vertical shutter adjacent to hole");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_add_shutter"), 1f);
                        break;
                    }

                    if (verbose) Debug.Log("Add vertical shutter at (" + a + ", " + b + ")");
                    if (commitAction)
                    {
                        if (walls.Contains(new WallInfo(WallInfo.Type.Vertical, a, b)))
                        {
                            walls.Remove(walls.Find((i) => (i.type == WallInfo.Type.Vertical) && i.x == a && i.y == b));
                            undoStack.Add(new EditActionInfo(new WallInfo(WallInfo.Type.Vertical, a, b), 
                                new WallInfo(WallInfo.Type.VerticalShutter, a, b)));
                        }
                        else
                        {
                            undoStack.Add(new EditActionInfo(null, new WallInfo(WallInfo.Type.VerticalShutter, a, b)));
                        }
                        redoStack.Clear();
                        solution = "";
                        dirtyBit = true;
                        GameManager.gm.PlayShutterSFX();
                        walls.Add(new WallInfo(WallInfo.Type.VerticalShutter, a, b));
                        hasChanged = true;
                    }
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
                        if (verbose) Debug.LogWarning("Editor warning: horizontal exit position at (" + a + ", " + b + ")");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_add_exit"), 1f);
                        break;
                    }
                    if (walls.Contains(new WallInfo(WallInfo.Type.ExitHorizontal, a, b)))
                    {
                        if (verbose) Debug.LogWarning("Editor warning: horizontal exit overlapped at (" + a + ", " + b + ")");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_add_exit"), 1f);
                        break;
                    }
                    if ((b == sizeY && objects.Contains(new ObjectInfo(ObjectInfo.Type.Hole, a, b))) ||
                        (b == 0 && objects.Contains(new ObjectInfo(ObjectInfo.Type.Hole, a, b + 1))))
                    {
                        if (verbose) Debug.LogWarning("Editor warning: horizontal exit adjacent to hole");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_add_exit"), 1f);
                        break;
                    }

                    if (walls.Exists((i) => i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.ExitVertical))
                    {
                        if (verbose) Debug.Log("Replace horizontal exit at (" + a + ", " + b + ")");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_duplicate_exits"), 1f);
                        if (commitAction)
                        {
                            undoStack.Add(new EditActionInfo(walls.Find((i) => i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.ExitVertical),
                                new WallInfo(WallInfo.Type.ExitHorizontal, a, b)));
                            redoStack.Clear();
                            solution = "";
                            dirtyBit = true;
                            GameManager.gm.PlayEscapedSFX();

#if UNITY_ANDROID && !UNITY_EDITOR
                            GameManager.gm.OnTriggerHaptic(6); //11 HapticError 
#endif
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
                            GameManager.gm.PlayEscapedSFX();

#if UNITY_ANDROID && !UNITY_EDITOR
                            GameManager.gm.OnTriggerHaptic(6); //11 HapticError
#endif
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
                        if (verbose) Debug.LogWarning("Editor warning: vertical exit position at (" + a + ", " + b + ")");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_add_exit"), 1f);
                        break;
                    }
                    if (walls.Contains(new WallInfo(WallInfo.Type.ExitVertical, a, b)))
                    {
                        if (verbose) Debug.LogWarning("Editor warning: vertical exit overlapped at (" + a + ", " + b + ")");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_add_exit"), 1f);
                        break;
                    }
                    if ((a == 0 && objects.Contains(new ObjectInfo(ObjectInfo.Type.Hole, a + 1, b))) ||
                        (a == sizeX && objects.Contains(new ObjectInfo(ObjectInfo.Type.Hole, a, b))))
                    {
                        if (verbose) Debug.LogWarning("Editor warning: vertical exit adjacent to hole");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_add_exit"), 1f);
                        break;
                    }

                    if (walls.Exists((i) => i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.ExitVertical))
                    {
                        if (verbose) Debug.Log("Replace vertical exit at (" + a + ", " + b + ")");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_duplicate_exits"), 1f);
                        if (commitAction)
                        {
                            undoStack.Add(new EditActionInfo(walls.Find((i) => i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.ExitVertical),
                                new WallInfo(WallInfo.Type.ExitVertical, a, b)));
                            redoStack.Clear();
                            solution = "";
                            dirtyBit = true;
                            GameManager.gm.PlayEscapedSFX();

#if UNITY_ANDROID && !UNITY_EDITOR
                            GameManager.gm.OnTriggerHaptic(6); //11 HapticError
#endif
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
                            GameManager.gm.PlayEscapedSFX();

#if UNITY_ANDROID && !UNITY_EDITOR
                            GameManager.gm.OnTriggerHaptic(6); //11 HapticError
#endif
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
                        if (verbose) Debug.LogWarning("Editor warning: vertical exit position at (" + a + ", " + b + ")");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_add_exit"), 1f);
                        break;
                    }
                    if (walls.Contains(new WallInfo(WallInfo.Type.ExitVertical, a, b)))
                    {
                        if (verbose) Debug.LogWarning("Editor warning: vertical exit overlapped at (" + a + ", " + b + ")");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_add_exit"), 1f);
                        break;
                    }
                    if (objects.Contains(new ObjectInfo(ObjectInfo.Type.Hole, a + 1, b)))
                    {
                        if (verbose) Debug.LogWarning("Editor warning: vertical exit adjacent to hole");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_add_exit"), 1f);
                        break;
                    }

                    if (walls.Exists((i) => i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.ExitVertical))
                    {
                        if (verbose) Debug.Log("Replace vertical exit at (" + a + ", " + b + ")");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_duplicate_exits"), 1f);
                        if (commitAction)
                        {
                            undoStack.Add(new EditActionInfo(walls.Find((i) => i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.ExitVertical),
                                new WallInfo(WallInfo.Type.ExitVertical, a, b)));
                            redoStack.Clear();
                            solution = "";
                            dirtyBit = true;
                            GameManager.gm.PlayEscapedSFX();

#if UNITY_ANDROID && !UNITY_EDITOR
                            GameManager.gm.OnTriggerHaptic(6); //11 HapticError
#endif
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
                            GameManager.gm.PlayEscapedSFX();

#if UNITY_ANDROID && !UNITY_EDITOR
                            GameManager.gm.OnTriggerHaptic(6); //11 HapticError
#endif
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
                        if (verbose) Debug.LogWarning("Editor warning: vertical exit position at (" + a + ", " + b + ")");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_add_exit"), 1f);
                        break;
                    }
                    if (walls.Contains(new WallInfo(WallInfo.Type.ExitVertical, a, b)))
                    {
                        if (verbose) Debug.LogWarning("Editor warning: vertical exit overlapped at (" + a + ", " + b + ")");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_add_exit"), 1f);
                        break;
                    }
                    if (objects.Contains(new ObjectInfo(ObjectInfo.Type.Hole, a, b)))
                    {
                        if (verbose) Debug.LogWarning("Editor warning: vertical exit adjacent to hole");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_add_exit"), 1f);
                        break;
                    }

                    if (walls.Exists((i) => i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.ExitVertical))
                    {
                        if (verbose) Debug.Log("Replace vertical exit at (" + a + ", " + b + ")");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_duplicate_exits"), 1f);
                        if (commitAction)
                        {
                            undoStack.Add(new EditActionInfo(walls.Find((i) => i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.ExitVertical),
                                new WallInfo(WallInfo.Type.ExitVertical, a, b)));
                            redoStack.Clear();
                            solution = "";
                            dirtyBit = true;
                            GameManager.gm.PlayEscapedSFX();

#if UNITY_ANDROID && !UNITY_EDITOR
                            GameManager.gm.OnTriggerHaptic(6); //11 HapticError
#endif
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
                            GameManager.gm.PlayEscapedSFX();

#if UNITY_ANDROID && !UNITY_EDITOR
                            GameManager.gm.OnTriggerHaptic(6); //11 HapticError
#endif
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
                        if (verbose) Debug.LogWarning("Editor warning: horizontal exit position at (" + a + ", " + b + ")");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_add_exit"), 1f);
                        break;
                    }
                    if (walls.Contains(new WallInfo(WallInfo.Type.ExitHorizontal, a, b)))
                    {
                        if (verbose) Debug.LogWarning("Editor warning: horizontal exit overlapped at (" + a + ", " + b + ")");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_add_exit"), 1f);
                        break;
                    }
                    if (objects.Contains(new ObjectInfo(ObjectInfo.Type.Hole, a, b + 1)))
                    {
                        if (verbose) Debug.LogWarning("Editor warning: horizontal exit adjacent to hole");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_add_exit"), 1f);
                        break;
                    }

                    if (walls.Exists((i) => i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.ExitVertical))
                    {
                        if (verbose) Debug.Log("Replace horizontal exit at (" + a + ", " + b + ")");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_duplicate_exits"), 1f);
                        if (commitAction)
                        {
                            undoStack.Add(new EditActionInfo(walls.Find((i) => i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.ExitVertical),
                                new WallInfo(WallInfo.Type.ExitHorizontal, a, b)));
                            redoStack.Clear();
                            solution = "";
                            dirtyBit = true;
                            GameManager.gm.PlayEscapedSFX();

#if UNITY_ANDROID && !UNITY_EDITOR
                            GameManager.gm.OnTriggerHaptic(6); //11 HapticError
#endif
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
                            GameManager.gm.PlayEscapedSFX();

#if UNITY_ANDROID && !UNITY_EDITOR
                            GameManager.gm.OnTriggerHaptic(6); //11 HapticError
#endif
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
                        if (verbose) Debug.LogWarning("Editor warning: horizontal exit position at (" + a + ", " + b + ")");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_add_exit"), 1f);
                        break;
                    }
                    if (walls.Contains(new WallInfo(WallInfo.Type.ExitHorizontal, a, b)))
                    {
                        if (verbose) Debug.LogWarning("Editor warning: horizontal exit overlapped at (" + a + ", " + b + ")");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_add_exit"), 1f);
                        break;
                    }
                    if (objects.Contains(new ObjectInfo(ObjectInfo.Type.Hole, a, b)))
                    {
                        if (verbose) Debug.LogWarning("Editor warning: horizontal exit adjacent to hole");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_add_exit"), 1f);
                        break;
                    }

                    if (walls.Exists((i) => i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.ExitVertical))
                    {
                        if (verbose) Debug.Log("Replace horizontal exit at (" + a + ", " + b + ")");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_duplicate_exits"), 1f);
                        if (commitAction)
                        {
                            undoStack.Add(new EditActionInfo(walls.Find((i) => i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.ExitVertical),
                                new WallInfo(WallInfo.Type.ExitHorizontal, a, b)));
                            redoStack.Clear();
                            solution = "";
                            dirtyBit = true;
                            GameManager.gm.PlayEscapedSFX();

#if UNITY_ANDROID && !UNITY_EDITOR
                            GameManager.gm.OnTriggerHaptic(6); //11 HapticError
#endif
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
                            GameManager.gm.PlayEscapedSFX();

#if UNITY_ANDROID && !UNITY_EDITOR
                            GameManager.gm.OnTriggerHaptic(6); //11 HapticError
#endif
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
                    // Remove horizontal wall or exit or shutter
                    a = (Mathf.FloorToInt(x + y) - Mathf.FloorToInt(y - x)) / 2;
                    b = (Mathf.FloorToInt(x + y) + Mathf.FloorToInt(y - x)) / 2;

                    if (a < 0 || a > sizeX + 1 || b < 0 || b > sizeY)
                    {
                        if (verbose) Debug.LogWarning("Editor warning: horizontal wall position at (" + a + ", " + b + ")");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_remove_wall"), 1f);
                        break;
                    }
                    if (!walls.Contains(new WallInfo(WallInfo.Type.Horizontal, a, b)) &&
                        !walls.Contains(new WallInfo(WallInfo.Type.ExitHorizontal, a, b)) &&
                        !walls.Contains(new WallInfo(WallInfo.Type.HorizontalShutter, a, b)))
                    {
                        if (verbose) Debug.LogWarning("Editor warning: horizontal wall or shutter or exit doesn't exist at (" + a + ", " + b + ")");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_remove_multi"), 1f);
                        break;
                    }

                    if (verbose) Debug.Log("Remove horizontal wall or shutter or exit at (" + a + ", " + b + ")");
                    //if (commitAction &&
                    //    !walls.Exists((i) => (i.type == WallInfo.Type.Horizontal || i.type == WallInfo.Type.ExitHorizontal) && i.x == a && i.y == b))
                    //    Debug.LogError("Editor invalid: null in Removing horizontal wall or exit");
                    if (commitAction)
                    {
                        undoStack.Add(new EditActionInfo(walls.Find((i) =>
                            (i.type == WallInfo.Type.Horizontal || i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.HorizontalShutter) && i.x == a && i.y == b), null));
                        redoStack.Clear();
                        solution = "";
                        dirtyBit = true;
                        GameManager.gm.PlayRemoveSFX();
                    }
                    walls.Remove(walls.Find((i) => (i.type == WallInfo.Type.Horizontal || i.type == WallInfo.Type.ExitHorizontal || i.type == WallInfo.Type.HorizontalShutter) && i.x == a && i.y == b));
                    hasChanged = true;
                }
                else if (x >= 0.5f && x < sizeX + 0.5f && y >= 0.5f && y < sizeY + 0.5f)
                {
                    // Remove vertical wall or exit
                    a = (Mathf.FloorToInt(x + y) - Mathf.CeilToInt(y - x)) / 2;
                    b = (Mathf.FloorToInt(x + y) + Mathf.CeilToInt(y - x)) / 2;

                    if (a < 0 || a > sizeX || b < 0 || b > sizeY + 1)
                    {
                        if (verbose) Debug.LogWarning("Editor warning: vertical wall position at (" + a + ", " + b + ")");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_remove_wall"), 1f);
                        break;
                    }
                    if (!walls.Contains(new WallInfo(WallInfo.Type.Vertical, a, b)) &&
                        !walls.Contains(new WallInfo(WallInfo.Type.ExitVertical, a, b)) &&
                        !walls.Contains(new WallInfo(WallInfo.Type.VerticalShutter, a, b)))
                    {
                        if (verbose) Debug.LogWarning("Editor warning: vertical wall or shutter or exit doesn't exist at (" + a + ", " + b + ")");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_remove_multi"), 1f);
                        break;
                    }

                    if (verbose) Debug.Log("Remove vertical wall or shutter or exit at (" + a + ", " + b + ")");
                    //if (commitAction &&
                    //    !walls.Exists((i) => (i.type == WallInfo.Type.Vertical || i.type == WallInfo.Type.ExitVertical) && i.x == a && i.y == b))
                    //    Debug.LogError("Editor invalid: null in Removing vertical wall or exit");
                    if (commitAction)
                    {
                        undoStack.Add(new EditActionInfo(walls.Find((i) =>
                            (i.type == WallInfo.Type.Vertical || i.type == WallInfo.Type.ExitVertical || i.type == WallInfo.Type.VerticalShutter) && i.x == a && i.y == b), null));
                        redoStack.Clear();
                        solution = "";
                        dirtyBit = true;
                        GameManager.gm.PlayRemoveSFX();
                    }
                    walls.Remove(walls.Find((i) => (i.type == WallInfo.Type.Vertical || i.type == WallInfo.Type.ExitVertical || i.type == WallInfo.Type.VerticalShutter) && i.x == a && i.y == b));
                    hasChanged = true;
                }
                else if (x < 0.5f)
                {
                    // Remove vertical left exit
                    a = 0;
                    b = Mathf.FloorToInt(y + 0.5f);

                    if (b < 1 || b > sizeY)
                    {
                        if (verbose) Debug.LogWarning("Editor warning: vertical exit position at (" + a + ", " + b + ")");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_remove_exit"), 1f);
                        break;
                    }
                    if (!walls.Contains(new WallInfo(WallInfo.Type.ExitVertical, a, b)))
                    {
                        if (verbose) Debug.LogWarning("Editor warning: vertical exit doesn't exist at (" + a + ", " + b + ")");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_remove_exit"), 1f);
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
                        GameManager.gm.PlayRemoveSFX();
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
                        if (verbose) Debug.LogWarning("Editor warning: vertical exit position at (" + a + ", " + b + ")");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_remove_exit"), 1f);
                        break;
                    }
                    if (!walls.Contains(new WallInfo(WallInfo.Type.ExitVertical, a, b)))
                    {
                        if (verbose) Debug.LogWarning("Editor warning: vertical exit doesn't exist at (" + a + ", " + b + ")");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_remove_exit"), 1f);
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
                        GameManager.gm.PlayRemoveSFX();
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
                        if (verbose) Debug.LogWarning("Editor warning: horizontal exit position at (" + a + ", " + b + ")");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_remove_exit"), 1f);
                        break;
                    }
                    if (!walls.Contains(new WallInfo(WallInfo.Type.ExitHorizontal, a, b)))
                    {
                        if (verbose) Debug.LogWarning("Editor warning: horizontal exit doesn't exist at (" + a + ", " + b + ")");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_remove_exit"), 1f);
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
                        GameManager.gm.PlayRemoveSFX();
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
                        if (verbose) Debug.LogWarning("Editor warning: horizontal exit position at (" + a + ", " + b + ")");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_remove_exit"), 1f);
                        break;
                    }
                    if (!walls.Contains(new WallInfo(WallInfo.Type.ExitHorizontal, a, b)))
                    {
                        if (verbose) Debug.LogWarning("Editor warning: horizontal exit doesn't exist at (" + a + ", " + b + ")");
                        statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_remove_exit"), 1f);
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
                        GameManager.gm.PlayRemoveSFX();
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
                    if (verbose) Debug.LogWarning("Editor warning: ball position at (" + a + ", " + b + ")");
                    statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_add_ball"), 1f);
                    break;
                }
                if (objects.Exists(i => i.x == a && i.y == b))
                {
                    if (verbose) Debug.LogWarning("Editor warning: objects overlapped at (" + a + ", " + b + ")");
                    statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_add_ball"), 1f);
                    break;
                }
                if ((objects.Contains(new ObjectInfo(ObjectInfo.Type.Hole, a, b + 1)) && !walls.Contains(new WallInfo(WallInfo.Type.Horizontal, a, b))) ||
                    (objects.Contains(new ObjectInfo(ObjectInfo.Type.Hole, a, b - 1)) && !walls.Contains(new WallInfo(WallInfo.Type.Horizontal, a, b - 1))) ||
                    (objects.Contains(new ObjectInfo(ObjectInfo.Type.Hole, a - 1, b)) && !walls.Contains(new WallInfo(WallInfo.Type.Vertical, a - 1, b))) ||
                    (objects.Contains(new ObjectInfo(ObjectInfo.Type.Hole, a + 1, b)) && !walls.Contains(new WallInfo(WallInfo.Type.Vertical, a, b))))
                {
                    if (verbose) Debug.LogWarning("Editor warning: ball adjacent to hole");
                    statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_add_ball"), 1f);
                    break;
                }

                if (objects.Exists((i) => i.type == ObjectInfo.Type.Ball))
                {
                    if (verbose) Debug.Log("Replace ball at (" + a + ", " + b + ")");
                    statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_duplicate_balls"), 1f);
                    if (commitAction)
                    {
                        undoStack.Add(new EditActionInfo(objects.Find((i) => i.type == ObjectInfo.Type.Ball), new ObjectInfo(ObjectInfo.Type.Ball, a, b)));
                        redoStack.Clear();
                        solution = "";
                        dirtyBit = true;
                        GameManager.gm.PlayBallSFX();
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
                        GameManager.gm.PlayBallSFX();
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
                    if (verbose) Debug.LogWarning("Editor warning: iron position at (" + a + ", " + b + ")");
                    statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_add_iron"), 1f);
                    break;
                }
                if (objects.Exists(i => i.x == a && i.y == b))
                {
                    if (verbose) Debug.LogWarning("Editor warning: objects overlapped at (" + a + ", " + b + ")");
                    statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_add_iron"), 1f);
                    break;
                }
                if ((objects.Contains(new ObjectInfo(ObjectInfo.Type.Hole, a, b + 1)) && !walls.Contains(new WallInfo(WallInfo.Type.Horizontal, a, b))) ||
                    (objects.Contains(new ObjectInfo(ObjectInfo.Type.Hole, a, b - 1)) && !walls.Contains(new WallInfo(WallInfo.Type.Horizontal, a, b - 1))) ||
                    (objects.Contains(new ObjectInfo(ObjectInfo.Type.Hole, a - 1, b)) && !walls.Contains(new WallInfo(WallInfo.Type.Vertical, a - 1, b))) ||
                    (objects.Contains(new ObjectInfo(ObjectInfo.Type.Hole, a + 1, b)) && !walls.Contains(new WallInfo(WallInfo.Type.Vertical, a, b))))
                {
                    if (verbose) Debug.LogWarning("Editor warning: iron adjacent to hole");
                    statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_add_iron"), 1f);
                    break;
                }

                if (verbose) Debug.Log("Add iron at (" + a + ", " + b + ")");
                if (commitAction)
                {
                    undoStack.Add(new EditActionInfo(null, new ObjectInfo(ObjectInfo.Type.Iron, a, b)));
                    redoStack.Clear();
                    solution = "";
                    dirtyBit = true;

#if UNITY_ANDROID && !UNITY_EDITOR
                    GameManager.gm.OnTriggerHaptic(7);
#endif
                    GameManager.gm.PlayIronSFX(UnityEngine.Random.Range(1, 9));
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
                    if (verbose) Debug.LogWarning("Editor warning: fire position at (" + a + ", " + b + ")");
                    statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_add_fire"), 1f);
                    break;
                }
                if (objects.Exists(i => i.x == a && i.y == b))
                {
                    if (verbose) Debug.LogWarning("Editor warning: objects overlapped at (" + a + ", " + b + ")");
                    statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_add_fire"), 1f);
                    break;
                }
                if ((objects.Contains(new ObjectInfo(ObjectInfo.Type.Hole, a, b + 1)) && !walls.Contains(new WallInfo(WallInfo.Type.Horizontal, a, b))) ||
                    (objects.Contains(new ObjectInfo(ObjectInfo.Type.Hole, a, b - 1)) && !walls.Contains(new WallInfo(WallInfo.Type.Horizontal, a, b - 1))) ||
                    (objects.Contains(new ObjectInfo(ObjectInfo.Type.Hole, a - 1, b)) && !walls.Contains(new WallInfo(WallInfo.Type.Vertical, a - 1, b))) ||
                    (objects.Contains(new ObjectInfo(ObjectInfo.Type.Hole, a + 1, b)) && !walls.Contains(new WallInfo(WallInfo.Type.Vertical, a, b))))
                {
                    if (verbose) Debug.LogWarning("Editor warning: fire adjacent to hole");
                    statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_add_fire"), 1f);
                    break;
                }

                if (verbose) Debug.Log("Add fire at (" + a + ", " + b + ")");
                if (commitAction)
                {
                    undoStack.Add(new EditActionInfo(null, new ObjectInfo(ObjectInfo.Type.Fire, a, b)));
                    redoStack.Clear();
                    solution = "";
                    dirtyBit = true;
                    GameManager.gm.PlayBurnedSFX();
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
                    if (verbose) Debug.LogWarning("Editor warning: object position at (" + a + ", " + b + ")");
                    statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_remove_object"), 1f);
                    break;
                }
                if (!objects.Exists(i => i.x == a && i.y == b))
                {
                    if (verbose) Debug.LogWarning("Editor warning: object doesn't exists at (" + a + ", " + b + ")");
                    statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_remove_object"), 1f);
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
                    GameManager.gm.PlayRemoveSFX();
                }
                objects.Remove(objects.Find(i => i.x == a && i.y == b));
                hasChanged = true;
                break;
#endregion
            case EditMode.Hole:
#region Hole
                a = Mathf.FloorToInt(x + 0.5f);
                b = Mathf.FloorToInt(y + 0.5f);

                if (a < 1 || a > sizeX || b < 1 || b > sizeY)
                {
                    if (verbose) Debug.LogWarning("Editor warning: hole position at (" + a + ", " + b + ")");
                    statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_add_hole"), 1f);
                    break;
                }
                if (objects.Exists(i => i.x == a && i.y == b))
                {
                    if (verbose) Debug.LogWarning("Editor warning: objects overlapped at (" + a + ", " + b + ")");
                    statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_add_hole"), 1f);
                    break;
                }

                // Cannot be placed adjacent to Exit
                if ((b == sizeY && walls.Contains(new WallInfo(WallInfo.Type.ExitHorizontal, a, b))) ||
                    (b == 1     && walls.Contains(new WallInfo(WallInfo.Type.ExitHorizontal, a, b))) ||
                    (a == 1     && walls.Contains(new WallInfo(WallInfo.Type.ExitVertical, a, b))) ||
                    (a == sizeX && walls.Contains(new WallInfo(WallInfo.Type.ExitVertical, a, b))))
                {
                    if (verbose) Debug.LogWarning("Editor warning: hole adjacent to exit");
                    statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_add_hole"), 1f);
                    break;
                }

                if (verbose) Debug.Log("Add hole at (" + a + ", " + b + ")");

                // Undo/Redo stack
                List<WallInfo> oldWalls = new();
                List<ObjectInfo> oldObjects = new();
                List<WallInfo> newWalls = new();
                List<ObjectInfo> newObjects = new();

                // Wall up
                if (b == sizeY)     // Uppermost
                {
                    if (walls.Contains(new WallInfo(WallInfo.Type.Horizontal, a, b)))
                    {
                        // WIP
                    }
                }
                else if (b < sizeY) // Not uppermost
                {
                    // Shutter was there
                    if (walls.Contains(new WallInfo(WallInfo.Type.HorizontalShutter, a, b)))
                    {
                        walls.Remove(walls.Find((i) => (i.type == WallInfo.Type.HorizontalShutter) && i.x == a && i.y == b));
                        if (commitAction) oldWalls.Add(new WallInfo(WallInfo.Type.HorizontalShutter, a, b));
                    }

                    // No wall was there (including when shutter was there)
                    if (!walls.Contains(new WallInfo(WallInfo.Type.Horizontal, a, b)))
                    {
                        if (commitAction) newWalls.Add(new WallInfo(WallInfo.Type.Horizontal, a, b));
                        walls.Add(new WallInfo(WallInfo.Type.Horizontal, a, b));
                    }

                    // Wall was there and Hole was over the wall
                    else if (objects.Contains(new ObjectInfo(ObjectInfo.Type.Hole, a, b + 1)))
                    {
                        walls.Remove(walls.Find((i) => (i.type == WallInfo.Type.Horizontal) && i.x == a && i.y == b));
                        if (commitAction) oldWalls.Add(new WallInfo(WallInfo.Type.Horizontal, a, b));
                    }
                }

                // Wall down
                if (b == 1)     // Lowermost
                {

                }
                else if (b > 1) // Not lowermost
                {
                    // Shutter was there
                    if (walls.Contains(new WallInfo(WallInfo.Type.HorizontalShutter, a, b - 1)))
                    {
                        walls.Remove(walls.Find((i) => (i.type == WallInfo.Type.HorizontalShutter) && i.x == a && i.y == b - 1));
                        if (commitAction) oldWalls.Add(new WallInfo(WallInfo.Type.HorizontalShutter, a, b - 1));
                    }

                    // No wall was there (including when shutter was there)
                    if (!walls.Contains(new WallInfo(WallInfo.Type.Horizontal, a, b - 1)))
                    {
                        if (commitAction) newWalls.Add(new WallInfo(WallInfo.Type.Horizontal, a, b - 1));
                        walls.Add(new WallInfo(WallInfo.Type.Horizontal, a, b - 1));
                    }

                    // Wall was there and Hole was over the wall
                    else if (objects.Contains(new ObjectInfo(ObjectInfo.Type.Hole, a, b - 1)))
                    {
                        walls.Remove(walls.Find((i) => (i.type == WallInfo.Type.Horizontal) && i.x == a && i.y == b - 1));
                        if (commitAction) oldWalls.Add(new WallInfo(WallInfo.Type.Horizontal, a, b - 1));
                    }
                }

                // Wall left
                if (a == 1)     // Leftmost
                {

                }
                else if (a > 1) // Not leftmost
                {
                    // Shutter was there
                    if (walls.Contains(new WallInfo(WallInfo.Type.VerticalShutter, a - 1, b)))
                    {
                        walls.Remove(walls.Find((i) => (i.type == WallInfo.Type.VerticalShutter) && i.x == a - 1 && i.y == b));
                        if (commitAction) oldWalls.Add(new WallInfo(WallInfo.Type.VerticalShutter, a - 1, b));
                    }

                    // No wall was there (including when shutter was there)
                    if (!walls.Contains(new WallInfo(WallInfo.Type.Vertical, a - 1, b)))
                    {
                        if (commitAction) newWalls.Add(new WallInfo(WallInfo.Type.Vertical, a - 1, b));
                        walls.Add(new WallInfo(WallInfo.Type.Vertical, a - 1, b));
                    }

                    // Wall was there and Hole was over the wall
                    else if (objects.Contains(new ObjectInfo(ObjectInfo.Type.Hole, a - 1, b)))
                    {
                        walls.Remove(walls.Find((i) => (i.type == WallInfo.Type.Vertical) && i.x == a - 1 && i.y == b));
                        if (commitAction) oldWalls.Add(new WallInfo(WallInfo.Type.Vertical, a - 1, b));
                    }
                }

                // Wall right
                if (a == sizeX)     // Rightmost
                {

                }
                else if (a < sizeX) // Not rightmost
                {
                    // Shutter was there
                    if (walls.Contains(new WallInfo(WallInfo.Type.VerticalShutter, a, b)))
                    {
                        walls.Remove(walls.Find((i) => (i.type == WallInfo.Type.VerticalShutter) && i.x == a && i.y == b));
                        if (commitAction) oldWalls.Add(new WallInfo(WallInfo.Type.VerticalShutter, a, b));
                    }

                    // No wall was there (including when shutter was there)
                    if (!walls.Contains(new WallInfo(WallInfo.Type.Vertical, a, b)))
                    {
                        if (commitAction) newWalls.Add(new WallInfo(WallInfo.Type.Vertical, a, b));
                        walls.Add(new WallInfo(WallInfo.Type.Vertical, a, b));
                    }

                    // Wall was there and Hole was over the wall
                    else if (objects.Contains(new ObjectInfo(ObjectInfo.Type.Hole, a + 1, b)))
                    {
                        walls.Remove(walls.Find((i) => (i.type == WallInfo.Type.Vertical) && i.x == a && i.y == b));
                        if (commitAction) oldWalls.Add(new WallInfo(WallInfo.Type.Vertical, a, b));
                    }
                }

                if (commitAction)
                {
                    newObjects.Add(new ObjectInfo(ObjectInfo.Type.Hole, a, b));
                    undoStack.Add(new EditActionInfo(oldWalls, oldObjects, newWalls, newObjects));
                    redoStack.Clear();
                    solution = "";
                    dirtyBit = true;
                    GameManager.gm.PlayWallSFX();
                }
                objects.Add(new ObjectInfo(ObjectInfo.Type.Hole, a, b));
                hasChanged = true;
                break;
#endregion
        }

        // Map Rendering
        mm.Initialize(sizeX, sizeY, walls, objects, "", timeLimit, false, false, true);

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
        switch (editMode) {
            case EditMode.None:
                statusUI.SetStatusMessage("");
                break;
            case EditMode.Ball:
                statusUI.SetStatusMessage(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_guide_add_ball"));
                break;
            case EditMode.Exit:
                statusUI.SetStatusMessage(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_guide_add_exit"));
                break;
            case EditMode.Fire:
                statusUI.SetStatusMessage(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_guide_add_fire"));
                break;
            case EditMode.Iron:
                statusUI.SetStatusMessage(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_guide_add_iron"));
                break;
            case EditMode.Hole:
                statusUI.SetStatusMessage(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_guide_add_hole"));
                break;
            case EditMode.Wall:
                statusUI.SetStatusMessage(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_guide_add_wall"));
                break;
            case EditMode.Shutter:
                statusUI.SetStatusMessage(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_guide_add_shutter"));
                break;
            case EditMode.RemoveWall:
                statusUI.SetStatusMessage(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_guide_remove_multi"));
                break;
            case EditMode.RemoveObject:
                statusUI.SetStatusMessage(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_guide_remove_object"));
                break;
        }
    }

    public void EditReset(bool setStatusMessage = true)
    {
        SetEditModeToNone();

        List<WallInfo> oldWalls = walls;
        List<ObjectInfo> oldObjects = objects;
        walls = new List<WallInfo>();
        objects = new List<ObjectInfo>();
        mm.Initialize(sizeX, sizeY, walls, objects, "", timeLimit);
        solution = "";

        if (hasCreated)
        {
            undoStack.Add(new EditActionInfo(oldWalls, oldObjects));
            redoStack.Clear();
            if (setStatusMessage)
            {
                statusUI.SetStatusMessage(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_reset_initial"));
                statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_reset_message"), 2f);
            }
            dirtyBit = true;
            GameManager.gm.PlayRemoveSFX();
        }
    }

    public void EditQuit(int phase)
    {
        if (phase != (int)EditPhase.Initialize && phase != (int)EditPhase.Request) return;
        if (dirtyBit)
        {
            // 경고 메시지 띄우기
            if (phase == (int)EditPhase.Initialize)
            {
                // 맵 변경 후 1페이즈의 나가기 버튼을 누르는 경우
                messageUI.Initialize(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_quit_changed"), () => GameManager.gm.LoadMain(), null);
            }
            else if (solution != null && solution != "")
            {
                // 맵 변경 후 검증은 완료했지만 저장되지 않은 상태에서 3페이즈의 나가기 버튼을 누르는 경우
                messageUI.Initialize(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_quit_unsaved"), () => GameManager.gm.LoadMain(), null);
            }
            else
            {
                // 맵 변경 후 검증을 하지 않고 3페이즈의 나가기 버튼을 누르는 경우
                messageUI.Initialize(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_quit_untested"), () => GameManager.gm.LoadMain(), null);
            }
        }
        else
        {
            GameManager.gm.LoadMain();
        }
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

            tempWalls = walls.FindAll(w => (w.type == WallInfo.Type.Vertical || w.type == WallInfo.Type.VerticalShutter) && w.x == value);
            removedWalls.AddRange(tempWalls);
            walls.RemoveAll(w => (w.type == WallInfo.Type.Vertical || w.type == WallInfo.Type.VerticalShutter) && w.x == value);

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

        mm.Initialize(sizeX, sizeY, walls, objects, "", timeLimit);
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

            tempWalls = walls.FindAll(w => (w.type == WallInfo.Type.Horizontal || w.type == WallInfo.Type.HorizontalShutter) && w.y == value);
            removedWalls.AddRange(tempWalls);
            walls.RemoveAll(w => (w.type == WallInfo.Type.Horizontal || w.type == WallInfo.Type.HorizontalShutter) && w.y == value);

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

        mm.Initialize(sizeX, sizeY, walls, objects, "", timeLimit);
    }

    public void EditSizeY(Dropdown caller)
    {
        EditSizeY(0, caller);
    }

    private bool IsBadFileName(string newName)
    {
        if (newName is null)
        {
            return true;
        }
        else if (newName.Contains("/") || newName.Contains("\\") ||
            newName.Contains(":") || newName.Contains("*") || newName.Contains("?") ||
            newName.Contains("\"") || newName.Contains("<") || newName.Contains(">") ||
            newName.Contains("|") || newName.Equals("con") || newName.Equals("aux") ||
            newName.Equals("nul") || newName.Equals("prn"))
        {
            return true;
        }
        else
        {
            for (int i = 0; i <= 9; i++)
            {
                if (newName.Equals("com" + i) || newName.Equals("lpt" + i))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void EditMapName(string newName, InputField valueChangedInputField = null)
    {
        string oldMapName = mapName;

        if (newName is null && !(valueChangedInputField is null))
        {
            newName = editorMapNameInputs.Find(e => e.Equals(valueChangedInputField)).text;
        }

        if (IsBadFileName(newName))
        {
            Debug.LogWarning("Editor warning: illegal file name");
            statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_illegal_file_name"), 1f);
            mapName = oldMapName;
            foreach (InputField editorMapNameInput in editorMapNameInputs)
                editorMapNameInput.text = mapName;
            return;
        }

        if (newName != null)
        {
            mapName = newName;
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
            //statusUI.SetStatusMessageWithFlashing("Map name changed.", 1.5f);
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

        editorMapNameInputs[0].interactable = true;
        editorSizeXDropdowns[0].interactable = true;
        editorSizeYDropdowns[0].interactable = true;
        editorNextButton1.interactable = true;
        timeLimit = MapManager.DEFAULT_TIME_LIMIT;
        EditMapName("");
        EditReset(false);
        if (hasPassedInitPhaseOnce)
        {
            statusUI.SetStatusMessage(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_reset_message"));
        }
        hasCreated = true;
    }

    public void EditOpenPhase()
    {
        if (editPhase != EditPhase.Initialize) return;

        // UI 만들기
        // Maps 폴더의 모든 맵을 불러와서 목록에 띄워주기

#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
            {
                Permission.RequestUserPermission(Permission.ExternalStorageRead);
            }
            if (!Directory.Exists(Path.GetDirectoryName(MapManager.ROOT_PATH)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(MapManager.ROOT_PATH));
            }
        }
        catch (Exception e)
        {
            statusUI.SetStatusMessageWithFlashing(e.ToString(), 2f);
            return;
        }
#endif

        try
        {
            if (!Directory.Exists(MapManager.MAP_ROOT_PATH))
            {
                Debug.LogWarning("File warning: there is no directory \"" + MapManager.MAP_ROOT_PATH + "\"");
                MetaUtil.CreateDirectory(MapManager.MAP_ROOT_PATH);
            }
        }
        catch (Exception e)
        {
            statusUI.SetStatusMessageWithFlashing(e.ToString(), 2f);
            return;
        }

        RenderOpenScrollView(MapManager.MAP_ROOT_PATH);

        editorPhases[0].SetActive(false);
        editorPhases[4].SetActive(true);
        mm.Initialize();
        editPhase = EditPhase.Open;
        statusUI.SetStatusMessage(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_open_map"));
        GameManager.gm.canPlay = false;
        foreach (var t in tooltipUI.GetComponentsInChildren<TooltipBox>())
        {
            Destroy(t.gameObject);
        }
    }

    private void RenderOpenScrollView(string openPath)
    {
        ClearOpenScrollItems();

        const float SCROLL_ITEM_HEIGHT = 84f;

        openPath = openPath.Replace('\\', '/');
        string[] files = null;
        string[] dirs = null;
        int index = 0;
        int length = 0;
        bool isRoot = true;
        try
        {
            files = Directory.GetFiles(openPath, "*.txt");
            dirs = Directory.GetDirectories(openPath);
            length = dirs.Length + files.Length;
        }
        catch (IOException)
        {
            Debug.LogError("File invalid: cannot open the path \"" + openPath + "\"");
            statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "warning_invalid_path"), 2f);
        }

        if (!openPath.TrimEnd('/').Equals(MapManager.MAP_ROOT_PATH.TrimEnd('/')))
        {
            isRoot = false;
            length++;
        }

        currentOpenPath = openPath.TrimEnd('/');
        //Debug.Log(currentOpenPath);

        string currentPath = currentOpenPath.Substring(currentOpenPath.LastIndexOf('/') + 1);
        if (currentOpenPath.Length <= 21)
        {
            editorOpenPathText.text = currentOpenPath;
        }
        else if (currentPath.Length <= 17)
        {
            string tempPath = currentOpenPath.Substring(currentOpenPath.Length - 17);
            tempPath = tempPath.Substring(tempPath.IndexOf('/') + 1);
            editorOpenPathText.text = ".../" + tempPath;
        }
        else
        {
            editorOpenPathText.text = ".../" + currentPath.Remove(14) + "...";
        }

        editorOpenScrollContent.GetComponent<RectTransform>().sizeDelta =
            new Vector2(editorOpenScrollContent.GetComponent<RectTransform>().sizeDelta.x, SCROLL_ITEM_HEIGHT * length);

        if (!openPath.TrimEnd('/').Equals(MapManager.MAP_ROOT_PATH.TrimEnd('/')))
        {
            GameObject g = Instantiate(openScrollItemPrefab, editorOpenScrollContent.transform);
            g.GetComponent<RectTransform>().offsetMin = new Vector2(0, -SCROLL_ITEM_HEIGHT / 2);
            g.GetComponent<RectTransform>().offsetMax = new Vector2(0, SCROLL_ITEM_HEIGHT / 2);
            g.GetComponent<RectTransform>().anchoredPosition =
                new Vector3(g.GetComponent<RectTransform>().anchoredPosition.x, (SCROLL_ITEM_HEIGHT / 2) * (length - 1 - 2 * index), 0f);

            g.GetComponent<OpenSaveScrollItem>().Initialize(OpenSaveScrollItem.Type.Open, currentOpenPath.Remove(currentOpenPath.LastIndexOf('/')), true, this, true);
            index++;
        }

        if (dirs != null)
        {
            foreach (string s in dirs)
            {
                GameObject g = Instantiate(openScrollItemPrefab, editorOpenScrollContent.transform);
                g.GetComponent<RectTransform>().offsetMin = new Vector2(0, -SCROLL_ITEM_HEIGHT / 2);
                g.GetComponent<RectTransform>().offsetMax = new Vector2(0, SCROLL_ITEM_HEIGHT / 2);
                g.GetComponent<RectTransform>().anchoredPosition =
                    new Vector3(g.GetComponent<RectTransform>().anchoredPosition.x, (SCROLL_ITEM_HEIGHT / 2) * (length - 1 - 2 * index), 0f);

                g.GetComponent<OpenSaveScrollItem>().Initialize(OpenSaveScrollItem.Type.Open, s, true, this, false);
                index++;
            }
        }

        if (files != null)
        {
            foreach (string s in files)
            {
                GameObject g = Instantiate(openScrollItemPrefab, editorOpenScrollContent.transform);
                g.GetComponent<RectTransform>().offsetMin = new Vector2(0, -SCROLL_ITEM_HEIGHT / 2);
                g.GetComponent<RectTransform>().offsetMax = new Vector2(0, SCROLL_ITEM_HEIGHT / 2);
                g.GetComponent<RectTransform>().anchoredPosition =
                    new Vector3(g.GetComponent<RectTransform>().anchoredPosition.x, (SCROLL_ITEM_HEIGHT / 2) * (length - 1 - 2 * index), 0f);
                g.GetComponent<OpenSaveScrollItem>().Initialize(OpenSaveScrollItem.Type.Open, s, false, this);
                index++;
            }
        }

        editorOpenScrollbar.numberOfSteps = Mathf.Max(1, length - 4);

        if (length == 0)
        {
            editorOpenScrollEmptyText.GetComponent<RectTransform>().offsetMax = new Vector3(0f, 0f, 0f);
            editorOpenScrollEmptyText.SetActive(true);
        }
        else if (!isRoot && length == 1)
        {
            editorOpenScrollEmptyText.GetComponent<RectTransform>().offsetMax = new Vector3(0f, -42f, 0f);
            editorOpenScrollEmptyText.SetActive(true);
        }
        else
        {
            editorOpenScrollEmptyText.SetActive(false);
        }
    }

    public void EditOpenItemSelect(OpenSaveScrollItem caller)
    {
        float selectTime = Time.time;
        if (caller != null && caller.Equals(selectedOpenScrollItem) && 
            openItemSelectTime > 0f && selectTime - openItemSelectTime < 0.5f)
        {
            // Double click
            EditOpen();
            return;
        }
        openItemSelectTime = selectTime;

        foreach (OpenSaveScrollItem i in editorOpenScrollContent.GetComponentsInChildren<OpenSaveScrollItem>())
        {
            i.isSelected = false;
        }
        caller.isSelected = true;
        selectedOpenScrollItem = caller;

        if (caller.isFolder)
        {
            editorOpenButton5.gameObject.SetActive(true);
            editorOpenHighlightedButton5.gameObject.SetActive(false);
            editorOpenButton5.interactable = true;
            mm.Initialize();
        }
        else
        {
            bool b = EditOpenFile(selectedOpenScrollItem.path, true);
            editorOpenHighlightedButton5.gameObject.SetActive(b);
            editorOpenButton5.gameObject.SetActive(!b);
            editorOpenButton5.interactable = b;
            editorOpenHighlightedButton5.interactable = b;
        }
    }

    public void EditOpen()
    {
        if (editPhase != EditPhase.Open || selectedOpenScrollItem is null) return;

        if (selectedOpenScrollItem.isFolder)
        {
            RenderOpenScrollView(selectedOpenScrollItem.path);
            mm.Initialize();
        }
        else
        {
            EditOpenFile(selectedOpenScrollItem.path, false);
        }
    }

    public bool EditOpenFile(string path, bool isPreview)
    {
        MapManager.OpenFileFlag openFileFlag = mm.InitializeFromFile(path, out int tempSizeX, out int tempSizeY, 
            out List<ObjectInfo> tempObjects, out List<WallInfo> tempWalls, 
            out string tempSolution, out float tempTimeLimit, statusUI);

        switch (openFileFlag)
        {
            case MapManager.OpenFileFlag.Restore:
                if (hasCreated && !isPreview)
                    mm.Initialize(sizeX, sizeY, walls, objects, solution, timeLimit);
                else
                    mm.Initialize();

                return false;
            case MapManager.OpenFileFlag.Success:
                if (isPreview)
                {
                    return true;
                }
                else
                {
                    if (hasCreated)
                    {
                        undoStack.Add(new EditActionInfo(mapName, Path.GetFileNameWithoutExtension(path),
                            sizeX, sizeY, tempSizeX, tempSizeY, walls, objects, tempWalls, tempObjects));
                        redoStack.Clear();
                    }

                    EditSizeX(tempSizeX);
                    EditSizeY(tempSizeY);
                    EditMapName(Path.GetFileNameWithoutExtension(path));
                    objects = tempObjects;
                    walls = tempWalls;
                    solution = tempSolution;
                    timeLimit = Mathf.Max(3f, tempTimeLimit);
                    mm.Initialize(sizeX, sizeY, walls, objects, solution, timeLimit);

                    editorMapNameInputs[0].interactable = false;
                    editorSizeXDropdowns[0].interactable = false;
                    editorSizeYDropdowns[0].interactable = false;
                    editorNextButton1.interactable = true;

                    editorPhases[4].SetActive(false);
                    editorPhases[0].SetActive(true);
                    editPhase = EditPhase.Initialize;
                    if (hasPassedInitPhaseOnce)
                    {
                        statusUI.SetStatusMessage(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_reinitialize_message"));
                    }

                    ClearOpenScrollItems();

                    hasCreated = true;

                    foreach (var t in tooltipUI.GetComponentsInChildren<TooltipBox>())
                    {
                        Destroy(t.gameObject);
                    }

                    return true;
                }
            case MapManager.OpenFileFlag.Failed:
            default:
                return false;
        }
    }

    public void EditSavePhase()
    {
        if (solution == null || solution == "" || !dirtyBit ||
            mm == null || !mm.IsReady || editPhase != EditPhase.Request) return;

        // UI 만들기
        // Maps 폴더의 모든 맵을 불러와서 목록에 띄워주기

#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
            {
                Permission.RequestUserPermission(Permission.ExternalStorageWrite);
            }
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
            {
                Permission.RequestUserPermission(Permission.ExternalStorageRead);
            }
            if (!Directory.Exists(MapManager.ROOT_PATH))
            {
                Directory.CreateDirectory(MapManager.ROOT_PATH);
            }
        }
        catch (Exception e)
        {
            statusUI.SetStatusMessageWithFlashing(e.ToString(), 2f);
            return;
        }
#endif

        if (!Directory.Exists(MapManager.MAP_ROOT_PATH))
        {
            Debug.LogWarning("File warning: there is no directory \"" + MapManager.MAP_ROOT_PATH + "\"");
            MetaUtil.CreateDirectory(MapManager.MAP_ROOT_PATH);
        }

        RenderSaveScrollView(MapManager.MAP_ROOT_PATH);

        editorPhases[2].SetActive(false);
        editorPhases[5].SetActive(true);
        editPhase = EditPhase.Save;
        GameManager.gm.canPlay = false;

        foreach (var t in tooltipUI.GetComponentsInChildren<TooltipBox>())
        {
            Destroy(t.gameObject);
        }
    }

    private void RenderSaveScrollView(string savePath)
    {
        ClearSaveScrollItems();

        const float SCROLL_ITEM_HEIGHT = 84f;

        savePath = savePath.Replace('\\', '/');
        string[] files = null;
        string[] dirs = null;
        int index = 0;
        int length = 0;
        bool isRoot = true;
        try
        {
            files = Directory.GetFiles(savePath, "*.txt");
            dirs = Directory.GetDirectories(savePath);
            length = dirs.Length + files.Length;
        }
        catch (IOException)
        {
            Debug.LogError("File invalid: cannot open the path \"" + savePath + "\"");
            statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "warning_invalid_path"), 2f);
        }

        if (!savePath.TrimEnd('/').Equals(MapManager.MAP_ROOT_PATH.TrimEnd('/')))
        {
            isRoot = false;
            length++;
        }

        currentSavePath = savePath.TrimEnd('/');
        //Debug.Log(currentSavePath);

        string currentPath = currentSavePath.Substring(currentSavePath.LastIndexOf('/') + 1);
        if (currentSavePath.Length <= 21)
        {
            editorSavePathText.text = currentSavePath;
        }
        else if (currentPath.Length <= 17)
        {
            string tempPath = currentSavePath.Substring(currentSavePath.Length - 17);
            tempPath = tempPath.Substring(tempPath.IndexOf('/') + 1);
            editorSavePathText.text = ".../" + tempPath;
        }
        else
        {
            editorSavePathText.text = ".../" + currentPath.Remove(14) + "...";
        }
        editorSavePathButton.IsTextHighlighted = true;

        editorSaveScrollContent.GetComponent<RectTransform>().sizeDelta =
            new Vector2(editorSaveScrollContent.GetComponent<RectTransform>().sizeDelta.x, SCROLL_ITEM_HEIGHT * length);

        if (!savePath.TrimEnd('/').Equals(MapManager.MAP_ROOT_PATH.TrimEnd('/')))
        {
            GameObject g = Instantiate(saveScrollItemPrefab, editorSaveScrollContent.transform);
            g.GetComponent<RectTransform>().offsetMin = new Vector2(0, -SCROLL_ITEM_HEIGHT / 2);
            g.GetComponent<RectTransform>().offsetMax = new Vector2(0, SCROLL_ITEM_HEIGHT / 2);
            g.GetComponent<RectTransform>().anchoredPosition =
                new Vector3(g.GetComponent<RectTransform>().anchoredPosition.x, (SCROLL_ITEM_HEIGHT / 2) * (length - 1 - 2 * index), 0f);

            g.GetComponent<OpenSaveScrollItem>().Initialize(OpenSaveScrollItem.Type.Save, currentSavePath.Remove(currentSavePath.LastIndexOf('/')), true, this, true);
            index++;
        }

        if (dirs != null)
        {
            foreach (string s in dirs)
            {
                GameObject g = Instantiate(saveScrollItemPrefab, editorSaveScrollContent.transform);
                g.GetComponent<RectTransform>().offsetMin = new Vector2(0, -SCROLL_ITEM_HEIGHT / 2);
                g.GetComponent<RectTransform>().offsetMax = new Vector2(0, SCROLL_ITEM_HEIGHT / 2);
                g.GetComponent<RectTransform>().anchoredPosition =
                    new Vector3(g.GetComponent<RectTransform>().anchoredPosition.x, (SCROLL_ITEM_HEIGHT / 2) * (length - 1 - 2 * index), 0f);

                g.GetComponent<OpenSaveScrollItem>().Initialize(OpenSaveScrollItem.Type.Save, s, true, this, false);
                index++;
            }
        }

        if (files != null)
        {
            foreach (string s in files)
            {
                GameObject g = Instantiate(saveScrollItemPrefab, editorSaveScrollContent.transform);
                g.GetComponent<RectTransform>().offsetMin = new Vector2(0, -SCROLL_ITEM_HEIGHT / 2);
                g.GetComponent<RectTransform>().offsetMax = new Vector2(0, SCROLL_ITEM_HEIGHT / 2);
                g.GetComponent<RectTransform>().anchoredPosition =
                    new Vector3(g.GetComponent<RectTransform>().anchoredPosition.x, (SCROLL_ITEM_HEIGHT / 2) * (length - 1 - 2 * index), 0f);
                g.GetComponent<OpenSaveScrollItem>().Initialize(OpenSaveScrollItem.Type.Save, s, false, this);
                index++;
            }
        }

        editorSaveScrollbar.numberOfSteps = Mathf.Max(1, length - 2);

        if (length == 0)
        {
            editorSaveScrollEmptyText.GetComponent<RectTransform>().offsetMax = new Vector3(0f, 0f, 0f);
            editorSaveScrollEmptyText.SetActive(true);
        }
        else if (!isRoot && length == 1)
        {
            editorSaveScrollEmptyText.GetComponent<RectTransform>().offsetMax = new Vector3(0f, -42f, 0f);
            editorSaveScrollEmptyText.SetActive(true);
        }
        else
        {
            editorSaveScrollEmptyText.SetActive(false);
        }
    }

    public void EditSaveItemSelect(OpenSaveScrollItem caller)
    {
        float selectTime = Time.time;
        if (caller != null && caller.Equals(selectedSaveScrollItem) &&
            saveItemSelectTime > 0f && selectTime - saveItemSelectTime < 0.5f)
        {
            // Double click
            EditSaveItemDoubleClick();
            return;
        }
        else if (caller != null && caller.Equals(selectedSaveScrollItem) && caller.isFolder &&
            saveItemSelectTime > 0f && selectTime - saveItemSelectTime >= 0.5f)
        {
            // Unselect the folder
            saveItemSelectTime = selectTime;

            foreach (OpenSaveScrollItem i in editorSaveScrollContent.GetComponentsInChildren<OpenSaveScrollItem>())
            {
                i.isSelected = false;
            }
            selectedSaveScrollItem = null;

            editorOpenButton6.gameObject.SetActive(false);
            editorSaveButton6.gameObject.SetActive(true);
            editorOpenButton6.interactable = false;
            editorSavePathButton.IsTextHighlighted = true;
        }
        else
        {
            saveItemSelectTime = selectTime;

            foreach (OpenSaveScrollItem i in editorSaveScrollContent.GetComponentsInChildren<OpenSaveScrollItem>())
            {
                i.isSelected = false;
            }
            selectedSaveScrollItem = caller;

            if (caller.isFolder)
            {
                caller.isSelected = true;
                editorOpenButton6.gameObject.SetActive(true);
                editorSaveButton6.gameObject.SetActive(false);
                editorOpenButton6.interactable = true;
                editorSavePathButton.IsTextHighlighted = false;
            }
            else
            {
                editorOpenButton6.gameObject.SetActive(false);
                editorSaveButton6.gameObject.SetActive(true);
                editorOpenButton6.interactable = false;
                editorSavePathButton.IsTextHighlighted = true;
            }
        }
    }

    public void EditSaveItemDoubleClick()
    {
        if (editPhase != EditPhase.Save || selectedSaveScrollItem is null) return;

        if (selectedSaveScrollItem.isFolder)
        {
            RenderSaveScrollView(selectedSaveScrollItem.path);
        }
        else
        {
            EditMapName(selectedSaveScrollItem.labelName);
            selectedSaveScrollItem.isSelected = true;
        }
    }

    public void EditSaveCurrentPathSelect()
    {
        foreach (OpenSaveScrollItem i in editorSaveScrollContent.GetComponentsInChildren<OpenSaveScrollItem>())
        {
            i.isSelected = false;
        }
        selectedSaveScrollItem = null;
        editorOpenButton6.gameObject.SetActive(false);
        editorSaveButton6.gameObject.SetActive(true);
        editorOpenButton6.interactable = false;
        editorSavePathButton.IsTextHighlighted = true;
    }

    public void EditNewFolder()
    {
        folderName = "";
        inputMessageUI.Initialize(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_create_folder_initial"), () => CreateNewFolder(), null);
    }

    public void EditNewFolderName(InputField caller)
    {
        string oldFolderName = folderName;

        string newName = caller.text;

        if (IsBadFileName(newName))
        {
            Debug.LogWarning("Editor warning: illegal folder name");
            statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_illegal_folder_name"), 1f);
            folderName = oldFolderName;
            caller.text = folderName;
            return;
        }

        if (newName != null)
        {
            folderName = newName;
            caller.text = folderName;
        }
    }

    private void CreateNewFolder()
    {
        if (folderName == null || folderName == "")
        {
            Debug.LogWarning("Editor warning: illegal folder name");
            statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_cannot_create_folder"), 2f);
            return;
        }

        if (!Directory.Exists(currentSavePath.TrimEnd('/') + "/" + folderName))
        {
            MetaUtil.CreateDirectory(currentSavePath.TrimEnd('/') + "/" + folderName);
            statusUI.SetStatusMessageWithFlashing(
                LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_create_folder_success_part1")
                + folderName
                + LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_create_folder_success_part2"),
                2f);
            RenderSaveScrollView(currentSavePath);
            foreach (OpenSaveScrollItem i in editorSaveScrollContent.GetComponentsInChildren<OpenSaveScrollItem>())
            {
                if (i.isFolder && i.labelName != null && i.labelName.Equals(folderName))
                {
                    i.Select();
                    break;
                }
            }
            folderName = "";
        }
        else
        {
            Debug.LogWarning("Editor warning: folder name already exists");
            statusUI.SetStatusMessageWithFlashing(
                LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_create_folder_fail_part1")
                + folderName
                + LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_create_folder_fail_part2"),
                2.5f);
            foreach (OpenSaveScrollItem i in editorSaveScrollContent.GetComponentsInChildren<OpenSaveScrollItem>())
            {
                if (i.isFolder && i.labelName != null && i.labelName.Equals(folderName))
                {
                    i.Select();
                    break;
                }
            }
            folderName = "";
        }
    }

    public void EditSave()
    {
        if (solution == null || solution == "" || mapName == null || mapName == "" || !dirtyBit ||
            mm == null || !mm.IsReady || editPhase != EditPhase.Save || isSaving) return;

        isSaving = true;

        if (!ValidateMapInGame())
        {
            Debug.LogError("File invalid: map validation failed");
            statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_create_map_not_validated"), 1.5f);
            isSaving = false;
            return;
        }

        try
        {
            if (!Directory.Exists(currentSavePath.TrimEnd('/')))
            {
                MetaUtil.CreateDirectory(currentSavePath.TrimEnd('/'));
            }
        }
        catch (Exception e)
        {
            Debug.LogError("File invalid: exception while creating a directory");
            statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_create_map_not_saved") + e.Message, 3f);
            isSaving = false;
            throw;
        }

        try
        {
            if (File.Exists(currentSavePath + "/" + mapName + ".txt"))
            {
                // 같은 이름의 파일이 있는데 그래도 저장할 것인지 메시지로 물어보기
                string truncated = mapName;
                if (truncated.Length > editorMapNameInputs[0].characterLimit)
                {
                    truncated = truncated.Substring(0, editorMapNameInputs[0].characterLimit - 3) + "...";
                }
                messageUI.Initialize(
                    LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_create_map_duplicate_name_ask_part1")
                    + truncated
                    + LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_create_map_duplicate_name_ask_part2"),
                    () => EditSaveHelper(), () => { isSaving = false; });
            }
            else
            {
                EditSaveHelper();
            }
        }
        catch (Exception e)
        {
            Debug.LogError("File invalid: exception while checking a file");
            statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_create_map_not_saved") + e.Message, 3f);
            isSaving = false;
            throw;
        }
    }

    private void EditSaveHelper()
    {
        FileStream fs = new FileStream(currentSavePath + "/" + mapName + ".txt", FileMode.Create);
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
                    case ObjectInfo.Type.Hole:
                        sw.WriteLine("/ " + o.x + " " + o.y);
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
                    case WallInfo.Type.HorizontalShutter:
                        sw.WriteLine(": - " + w.x + " " + w.y);
                        break;
                    case WallInfo.Type.VerticalShutter:
                        sw.WriteLine(": | " + w.x + " " + w.y);
                        break;
                    case WallInfo.Type.ExitHorizontal:
                        sw.WriteLine("$ - " + w.x + " " + w.y);
                        break;
                    case WallInfo.Type.ExitVertical:
                        sw.WriteLine("$ | " + w.x + " " + w.y);
                        break;
                }
            }
            sw.WriteLine("t " + timeLimit);
            sw.WriteLine("s " + Convert.ToBase64String(Encoding.UTF8.GetBytes(solution)));
        }
        catch (Exception)
        {
            Debug.LogError("File invalid: exception while saving a map");
            isSaving = false;
            throw;
        }
        finally
        {
            sw.Close();
            fs.Close();
        }

        string truncated = mapName;
        if (truncated.Length > editorMapNameInputs[0].characterLimit)
        {
            truncated = truncated.Substring(0, editorMapNameInputs[0].characterLimit - 3) + "...";
        }
        //Debug.Log("Saved as " + truncated);
        statusUI.SetStatusMessage(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_create_map_success_part1") +
            truncated + LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_create_map_success_part2"));

        dirtyBit = false;
        hasSavedOnce = true;
        isSaving = false;

        EditNext();
    }

    public void EditNext()
    {
        switch (editPhase)
        {
            case EditPhase.Initialize:
                editorMapNameInputs[0].interactable = true;
                editorSizeXDropdowns[0].interactable = true;
                editorSizeYDropdowns[0].interactable = true;
                editorPhases[0].SetActive(false);
                editorPhases[1].SetActive(true);
                SetEditModeToNone();
                editPhase = EditPhase.Build;
                hasPassedInitPhaseOnce = true;
                statusUI.SetStatusMessage(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_reset_initial"));
                GameManager.gm.canPlay = false;
                break;
            case EditPhase.Build:
                SetEditTimerUI(mm.TimeLimit);
                editorPhases[1].SetActive(false);
                editorPhases[2].SetActive(true);
                editPhase = EditPhase.Request;
                GameManager.gm.canPlay = false;
                break;
            case EditPhase.Request:
                editorPhases[2].SetActive(false);
                editorPhases[3].SetActive(true);
                statusUI.gameObject.SetActive(false);
                timerUI.gameObject.SetActive(true);
                editPhase = EditPhase.Test;
                mm.Initialize(sizeX, sizeY, walls, objects, solution, timeLimit);
                mm.afterGravity = EditorAfterGravity;
                EditorAfterGravity(MapManager.Flag.Continued);
                mm.TimeActivate();
                GameManager.gm.canPlay = true;
                GameManager.gm.EditorChangeBGM(editPhase);
                break;
            case EditPhase.Test:
                // Validation finished
                timerUI.gameObject.SetActive(false);
                statusUI.gameObject.SetActive(true);
                SetEditTimerUI(mm.TimeLimit);
                editorPhases[3].SetActive(false);
                editorPhases[2].SetActive(true);
                editPhase = EditPhase.Request;
                solution = mm.ActionHistory;
                mm.Initialize(sizeX, sizeY, walls, objects, solution, timeLimit);
                GameManager.gm.canPlay = false;
                GameManager.gm.EditorChangeBGM(editPhase);
                break;
            case EditPhase.Save:
                SetEditTimerUI(mm.TimeLimit);
                editorPhases[5].SetActive(false);
                editorPhases[2].SetActive(true);
                editPhase = EditPhase.Request;
                GameManager.gm.canPlay = false;
                break;
        }
        foreach (var t in tooltipUI.GetComponentsInChildren<TooltipBox>())
        {
            Destroy(t.gameObject);
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
                statusUI.SetStatusMessage(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_create_map_reinitialize_message"));
                GameManager.gm.canPlay = false;
                break;
            case EditPhase.Request:
                editorPhases[2].SetActive(false);
                editorPhases[1].SetActive(true);
                editPhase = EditPhase.Build;
                statusUI.SetStatusMessage(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_reset_initial"));
                SetEditModeToNone();
                GameManager.gm.canPlay = false;
                break;
            case EditPhase.Test:
                timerUI.gameObject.SetActive(false);
                statusUI.gameObject.SetActive(true);
                SetEditTimerUI(mm.TimeLimit);
                editorPhases[3].SetActive(false);
                editorPhases[2].SetActive(true);
                editPhase = EditPhase.Request;
                mm.Initialize(sizeX, sizeY, walls, objects, solution, timeLimit);
                GameManager.gm.canPlay = false;
                GameManager.gm.EditorChangeBGM(editPhase);
                break;
            case EditPhase.Open:
                editorPhases[4].SetActive(false);
                editorPhases[0].SetActive(true);
                editPhase = EditPhase.Initialize;
                ClearOpenScrollItems();
                if (hasCreated)
                {
                    mm.Initialize(sizeX, sizeY, walls, objects, solution, timeLimit);
                    statusUI.SetStatusMessage(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_create_map_reinitialize_message"));
                }
                else
                {
                    mm.Initialize();
                    statusUI.SetStatusMessage(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_start_welcome"));
                }
                GameManager.gm.canPlay = false;
                break;
            case EditPhase.Save:
                SetEditTimerUI(mm.TimeLimit);
                editorPhases[5].SetActive(false);
                editorPhases[2].SetActive(true);
                editPhase = EditPhase.Request;
                if (!hasSavedOnce)
                {
                    statusUI.SetStatusMessage(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_warning_map_not_saved_yet"));
                }
                ClearSaveScrollItems();
                GameManager.gm.canPlay = false;
                break;
        }
        foreach (var t in tooltipUI.GetComponentsInChildren<TooltipBox>())
        {
            Destroy(t.gameObject);
        }
    }

    public void EditorAfterGravity(MapManager.Flag flag)
    {
        switch (flag)
        {
            case MapManager.Flag.Continued:
                editorBackHighlightedButton4.gameObject.SetActive(false);
                editorBackButton4.gameObject.SetActive(true);

                editorRetryButton.gameObject.SetActive(true);
                editorRetryHighlightedButton.gameObject.SetActive(false);
                editorRetryTimeButton.gameObject.SetActive(false);
                editorRetryTimeHighlightedButton.gameObject.SetActive(false);

                editorNextButton4.interactable = false;
                editorBackButton4.interactable = true;
                break;
            case MapManager.Flag.Escaped:
                editorBackHighlightedButton4.gameObject.SetActive(false);
                editorBackButton4.gameObject.SetActive(true);

                editorRetryButton.gameObject.SetActive(false);
                editorRetryHighlightedButton.gameObject.SetActive(false);
                editorRetryTimeButton.gameObject.SetActive(true);
                editorRetryTimeButton.interactable = false;
                editorRetryTimeHighlightedButton.gameObject.SetActive(false);

                editorNextButton4.interactable = true;
                editorBackButton4.interactable = false;
                break;
            case MapManager.Flag.Burned:
            case MapManager.Flag.Squashed:
                editorBackButton4.gameObject.SetActive(false);
                editorBackHighlightedButton4.gameObject.SetActive(true);

                editorRetryButton.gameObject.SetActive(false);
                editorRetryHighlightedButton.gameObject.SetActive(true);
                editorRetryTimeButton.gameObject.SetActive(false);
                editorRetryTimeHighlightedButton.gameObject.SetActive(false);

                editorNextButton4.interactable = false;
                editorBackHighlightedButton4.interactable = true;
                break;
            case MapManager.Flag.TimeOver:
                editorBackButton4.gameObject.SetActive(false);
                editorBackHighlightedButton4.gameObject.SetActive(true);

                editorRetryButton.gameObject.SetActive(false);
                editorRetryHighlightedButton.gameObject.SetActive(false);
                editorRetryTimeButton.gameObject.SetActive(false);
                editorRetryTimeHighlightedButton.gameObject.SetActive(true);

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
                statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_undo_map_name"), 1f);
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

                mm.Initialize(sizeX, sizeY, walls, objects, "", timeLimit);
                statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_undo_multi"), 1f);
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

                mm.Initialize(sizeX, sizeY, walls, objects, "", timeLimit);
                statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_undo_object"), 1f);
#endregion
                break;
            case EditActionInfo.Type.SeveralChange:
                foreach (WallInfo wi in eai.newWalls)
                    walls.Remove(wi);
                foreach (ObjectInfo oi in eai.newObjects)
                    objects.Remove(oi);
                walls.AddRange(eai.oldWalls);
                objects.AddRange(eai.oldObjects);
                mm.Initialize(sizeX, sizeY, walls, objects, "", timeLimit);
                statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_undo_several"), 1f);
                break;
            case EditActionInfo.Type.SizeX:
                EditSizeX(eai.oldSize);
                walls.AddRange(eai.oldWalls);
                objects.AddRange(eai.oldObjects);
                mm.Initialize(sizeX, sizeY, walls, objects, "", timeLimit);
                statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_undo_map_size"), 1f);
                break;
            case EditActionInfo.Type.SizeY:
                EditSizeY(eai.oldSize);
                walls.AddRange(eai.oldWalls);
                objects.AddRange(eai.oldObjects);
                mm.Initialize(sizeX, sizeY, walls, objects, "", timeLimit);
                statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_undo_map_size"), 1f);
                break;
            case EditActionInfo.Type.MassRemoval:
                walls.AddRange(eai.oldWalls);
                objects.AddRange(eai.oldObjects);
                mm.Initialize(sizeX, sizeY, walls, objects, "", timeLimit);
                statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_undo_reset"), 1f);
                break;
            case EditActionInfo.Type.MassChange:
                EditMapName(eai.oldName);
                EditSizeX(eai.oldSizeX);
                EditSizeY(eai.oldSizeY);
                foreach (WallInfo wi in eai.newWalls)
                    walls.Remove(wi);
                foreach (ObjectInfo oi in eai.newObjects)
                    objects.Remove(oi);
                walls.AddRange(eai.oldWalls);
                objects.AddRange(eai.oldObjects);
                mm.Initialize(sizeX, sizeY, walls, objects, "", timeLimit);
                statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_undo_reintialization"), 1f);
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
                statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_redo_map_name"), 1f);
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

                mm.Initialize(sizeX, sizeY, walls, objects, "", timeLimit);
                statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_redo_multi"), 1f);
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

                mm.Initialize(sizeX, sizeY, walls, objects, "", timeLimit);
                statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_redo_object"), 1f);
#endregion
                break;
            case EditActionInfo.Type.SeveralChange:
                foreach (WallInfo wi in eai.oldWalls)
                    walls.Remove(wi);
                foreach (ObjectInfo oi in eai.oldObjects)
                    objects.Remove(oi);
                walls.AddRange(eai.newWalls);
                objects.AddRange(eai.newObjects);
                mm.Initialize(sizeX, sizeY, walls, objects, "", timeLimit);
                statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_redo_several"), 1f);
                break;
            case EditActionInfo.Type.SizeX:
                EditSizeX(eai.newSize);
                mm.Initialize(sizeX, sizeY, walls, objects, "", timeLimit);
                statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_redo_map_size"), 1f);
                break;
            case EditActionInfo.Type.SizeY:
                EditSizeY(eai.newSize);
                mm.Initialize(sizeX, sizeY, walls, objects, "", timeLimit);
                statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_redo_map_size"), 1f);
                break;
            case EditActionInfo.Type.MassRemoval:
                foreach (WallInfo wi in eai.oldWalls)
                    walls.Remove(wi);
                foreach (ObjectInfo oi in eai.oldObjects)
                    objects.Remove(oi);
                mm.Initialize(sizeX, sizeY, walls, objects, "", timeLimit);
                statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_redo_reset"), 1f);
                break;
            case EditActionInfo.Type.MassChange:
                EditMapName(eai.newName);
                EditSizeX(eai.newSizeX);
                EditSizeY(eai.newSizeY);
                foreach (WallInfo wi in eai.oldWalls)
                    walls.Remove(wi);
                foreach (ObjectInfo oi in eai.oldObjects)
                    objects.Remove(oi);
                walls.AddRange(eai.newWalls);
                objects.AddRange(eai.newObjects);
                mm.Initialize(sizeX, sizeY, walls, objects, "", timeLimit);
                statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_redo_reinitialization"), 1f);
                break;
        }
        redoStack.RemoveAt(redoStack.Count - 1);
        undoStack.Add(eai);
        solution = "";
        dirtyBit = true;
    }
#pragma warning restore CS0162 // 접근할 수 없는 코드가 있습니다.

    public void EditTimerDragging()
    {
        if (mm == null || !mm.IsReady || editPhase != EditPhase.Request) return;
        timeLimit = Mathf.Max(3f, editorTimerSlider.value);
        SetEditTimerUI(timeLimit);
    }

    public void EditTimerDragUp()
    {
        if (mm == null || !mm.IsReady || editPhase != EditPhase.Request) return;
        timeLimit = Mathf.Max(3f, editorTimerSlider.value);
        if (mm.TimeLimit != timeLimit)
        {
            mm.TimeLimit = timeLimit;
            solution = "";
            dirtyBit = true;
        }

        SetEditTimerUI(mm.TimeLimit);
    }

    private void SetEditTimerUI(float displayingTimeLimit)
    {
        //Debug.Log("SetEditTimerUI");
        if (!mm.IsReady)
        {
            editorTimerSlider.SetValueWithoutNotify(0f);
            editorTimerLabel10.sprite = timerUI.numberLabels[0];
            editorTimerLabel1.sprite = timerUI.numberLabels[0];
            return;
        }

        if (displayingTimeLimit > 99f)
        {
            editorTimerSlider.SetValueWithoutNotify(editorTimerSlider.maxValue);
            editorTimerLabel10.sprite = timerUI.numberLabels[9];
            editorTimerLabel1.sprite = timerUI.numberLabels[9];
        }
        else
        {
            editorTimerSlider.SetValueWithoutNotify(displayingTimeLimit);
            editorTimerLabel10.sprite = timerUI.numberLabels[Mathf.CeilToInt(displayingTimeLimit) / 10];
            editorTimerLabel1.sprite = timerUI.numberLabels[Mathf.CeilToInt(displayingTimeLimit) % 10];
        }
    }

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
        mm.Initialize(sizeX, sizeY, walls, objects, solution, timeLimit, true);
        return mm.IsReady;
    }

    private void ClearOpenScrollItems()
    {
        selectedOpenScrollItem = null;
        foreach (OpenSaveScrollItem i in editorOpenScrollContent.GetComponentsInChildren<OpenSaveScrollItem>())
        {
            Destroy(i.gameObject);
        }

        editorOpenButton5.gameObject.SetActive(true);
        editorOpenHighlightedButton5.gameObject.SetActive(false);

        editorOpenButton5.interactable = false;
        editorOpenHighlightedButton5.interactable = false;
    }

    private void ClearSaveScrollItems()
    {
        selectedSaveScrollItem = null;
        foreach (OpenSaveScrollItem i in editorSaveScrollContent.GetComponentsInChildren<OpenSaveScrollItem>())
        {
            Destroy(i.gameObject);
        }

        editorSaveButton6.gameObject.SetActive(true);
        editorOpenButton6.gameObject.SetActive(false);

        editorOpenButton6.interactable = false;
    }

    public void PlayButtonSFX()
    {
        GameManager.gm.PlayButtonSFX();
    }

    private class EditActionInfo
    {
        public enum Type { MapName, SizeX, SizeY, Wall, Object, SeveralChange, MassRemoval, MassChange }

        public Type type;

        // MapName, MassChange
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

        // SizeX, SizeY, MassRemoval, MassChange
        public List<WallInfo> oldWalls;
        public List<ObjectInfo> oldObjects;

        // MassChange
        public int oldSizeX;
        public int oldSizeY;
        public int newSizeX;
        public int newSizeY;
        public List<WallInfo> newWalls;
        public List<ObjectInfo> newObjects;

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
        /// Type: SeveralChange (Hole)
        /// </summary>
        /// <param name="oldWalls">없으면 null</param>
        /// <param name="oldObjects">없으면 null</param>
        /// <param name="newWalls">없으면 null</param>
        /// <param name="newObjects">없으면 null</param>
        public EditActionInfo(List<WallInfo> oldWalls, List<ObjectInfo> oldObjects, List<WallInfo> newWalls, List<ObjectInfo> newObjects)
        {
            type = Type.SeveralChange;
            this.oldWalls = oldWalls;
            this.oldObjects = oldObjects;
            this.newWalls = newWalls;
            this.newObjects = newObjects;
        }


        /// <summary>
        /// Type: MassRemoval (Reset, New)
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

        /// <summary>
        /// Type: MassChange (Open)
        /// </summary>
        public EditActionInfo(string oldMapName, string newMapName,
            int oldSizeX, int oldSizeY, int newSizeX, int newSizeY,
            List<WallInfo> oldWalls, List<ObjectInfo> oldObjects,
            List<WallInfo> newWalls, List<ObjectInfo> newObjects)
        {
            type = Type.MassChange;
            oldName = oldMapName;
            newName = newMapName;
            this.oldSizeX = oldSizeX;
            this.oldSizeY = oldSizeY;
            this.newSizeX = newSizeX;
            this.newSizeY = newSizeY;
            this.oldWalls = new List<WallInfo>();
            this.oldObjects = new List<ObjectInfo>();
            this.newWalls = new List<WallInfo>();
            this.newObjects = new List<ObjectInfo>();
            if (!(oldWalls is null))
                this.oldWalls.AddRange(oldWalls);       // Clone()
            if (!(oldObjects is null))
                this.oldObjects.AddRange(oldObjects);   // Clone()
            if (!(newWalls is null))
                this.newWalls.AddRange(newWalls);       // Clone()
            if (!(newObjects is null))
                this.newObjects.AddRange(newObjects);   // Clone()
        }
    }
}