using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayManager : MonoBehaviour
{
    public enum Mode { Tutorial = 0, Custom = 1, Training = 2,
        AdvEasy = 11, AdvNormal = 12, AdvHard = 13, AdvInsane = 14 }

    public enum CustomPhase { Open = 0, Ingame = 1 }
    public enum TrainingPhase { Open = 0, Ingame = 1 }

    public enum TrainingMapSelect { Root = -1, Basic = 0, Fire = 1, Iron = 2, Block = 3, Exit = 4, Shutter = 5, Gate = 6 }
   
    public Button pauseButton;                   // quitHighlightedButton이 활성화될 때 비활성화
    public Button quitHighlightedButton;        // 모든 맵을 탈출하거나 라이프가 0이 되어 게임이 종료될 때 활성화
    public Button nextButton;                   // 탈출 또는 시간 초과 시 활성화 (튜토리얼에서는 탈출 시에만 활성화), quitHighlightedButton이 활성화될 때 비활성화
    public Button retryButton;                  // Continued일 때 활성화, 사망 또는 탈출 또는 시간 초과 시 비활성화
    public Button retryHighlightedButton;       // Burned 또는 Squashed일 때 활성화
    public Button retryTimeButton;              // 시간 초과 시 활성화 (튜토리얼에서는 탈출 시 활성화)
    public Button retryTimeHighlightedButton;   // (튜토리얼에서만 시간 초과 시 활성화)
    // public MessageUI messageUI;
    public PauseUI pauseUI;
    public GameObject pausePanel;
    public ResultUI resultUI;
    public GameObject tooltipUI;
    public GameObject timerUI;

    private OpenSaveScrollItem selectedOpenScrollItem;
    public GameObject openScrollContent;
    public Button openButton;
    public Button openHighlightedButton;
    public StatusUI statusUI;
    private string currentOpenPath = MapManager.MAP_ROOT_PATH;
    public Text openPathText;
    public GameObject openScrollItemPrefab;
    public Scrollbar openScrollbar;
    public GameObject openScrollEmptyText;
    private float openItemSelectTime = 0f;

    public GameObject openUI;
    public GameObject ingameUI;

    public TutorialGuide tutorialGuide;//추가

    public List<string> trainingFolders = new List<string>();

    private Mode playMode;
    private CustomPhase customPhase;
    private TrainingPhase trainingPhase;

    private TrainingMapSelect selection;
    private string customSelectedPath;

    public Mode PlayMode{
        get{return playMode;}
    }

    [Header("Tutorial")]
    [SerializeField]
    private List<TextAsset> tutorialMapFiles = new List<TextAsset>();

    [Header("Easy")]
    [SerializeField]
    private List<TextAsset> adventureEasyMapFiles = new List<TextAsset>();
    [SerializeField]
    private List<string> adventureEasyMapSeries = new List<string>();
    [SerializeField]
    private int adventureEasyPlayLength = int.MaxValue;
    [SerializeField]
    private int adventureEasyLife = 5;

    [Header("Normal")]
    [SerializeField]
    private List<TextAsset> adventureNormalMapFiles = new List<TextAsset>();
    [SerializeField]
    private List<string> adventureNormalMapSeries = new List<string>();
    [SerializeField]
    private int adventureNormalPlayLength = int.MaxValue;
    [SerializeField]
    private int adventureNormalLife = 5;

    [Header("Hard")]
    [SerializeField]
    private List<TextAsset> adventureHardMapFiles = new List<TextAsset>();
    [SerializeField]
    private List<string> adventureHardMapSeries = new List<string>();
    [SerializeField]
    private int adventureHardPlayLength = int.MaxValue;
    [SerializeField]
    private int adventureHardLife = 5;

    [Header("Insane")]
    [SerializeField]
    private List<TextAsset> adventureInsaneMapFiles = new List<TextAsset>();
    [SerializeField]
    private List<string> adventureInsaneMapSeries = new List<string>();
    [SerializeField]
    private int adventureInsanePlayLength = int.MaxValue;
    [SerializeField]
    private int adventureInsaneLife = 5;

    [Header("Training")]
    [SerializeField]
    private List<TextAsset> trainingBasicMapFiles = new List<TextAsset>();
    [SerializeField]
    private List<TextAsset> trainingFireMapFiles = new List<TextAsset>();
    [SerializeField]
    private List<TextAsset> trainingIronMapFiles = new List<TextAsset>();
    [SerializeField]
    private List<TextAsset> trainingBlockMapFiles = new List<TextAsset>();
    [SerializeField]
    private List<TextAsset> trainingExitMapFiles = new List<TextAsset>();
    [SerializeField]
    private List<TextAsset> trainingShutterMapFiles = new List<TextAsset>();
    [SerializeField]
    private List<TextAsset> trainingGateMapFiles = new List<TextAsset>();



    private List<TextAsset> _mapFiles;

    public int AdventureEasyLife{
        get{
            return adventureEasyLife;
        }
    }

    public int AdventureNormalLife{
        get{
            return adventureNormalLife;
        }
    }

    public int AdventureHardLife{
        get{
            return adventureHardLife;
        }
    }

    public int AdventureInsaneLife{
        get{
            return adventureInsaneLife;
        }
    }

    public List<TextAsset> MapFiles
    {
        get
        {
            if (!IsReady) return null;
            return _mapFiles.ToList();
        }
    }

    public int PlayLength
    {
        get;
        private set;
    } = int.MaxValue;

    public int SkippedCount
    {
        get;
        private set;
    } = 0;

    public bool IsRandomOrder
    {
        get;
        private set;
    } = false;

    public int Life
    {
        get;
        set;
    } = 5;

    public int EscapedCount
    {
        get;
        private set;
    } = 0;

    public bool HasClearedAll
    {
        get
        {
            return IsReady && Life > 0 && EscapedCount >= PlayLength;
        }
    }

    public bool IsReady
    {
        get;
        private set;
    } = false;

    public bool IsHurt
    {
        get;
        private set;
    } = false;

    public void Initialize(Mode mode, bool isRandomOrder = false, int maxPlayLength = int.MaxValue, int initialLife = 5)
    {
        IsReady = false;
        EscapedCount = 0;
        SkippedCount = 0;
        playMode = mode;
        // messageUI.gameObject.SetActive(false);
        pauseUI.gameObject.SetActive(false);
        pausePanel.SetActive(false);
        if (mode != Mode.Custom && mode != Mode.Training)
        { 
            resultUI.gameObject.SetActive(false); 
        }
        switch (playMode)
        {
            case Mode.Tutorial:
                _mapFiles = tutorialMapFiles;
                IsRandomOrder = false;
                Life = int.MaxValue;
                PlayLength = _mapFiles.Count;
                break;
            case Mode.AdvEasy:
                _mapFiles = SeriesToMapFiles(adventureEasyMapSeries, adventureEasyMapFiles);
                IsRandomOrder = isRandomOrder;
                Life = adventureEasyLife;
                PlayLength = Mathf.Clamp(adventureEasyPlayLength, 1, _mapFiles.Count - Life + 1);
                break;
            case Mode.AdvNormal:
                _mapFiles = SeriesToMapFiles(adventureNormalMapSeries, adventureNormalMapFiles);
                IsRandomOrder = isRandomOrder;
                Life = adventureNormalLife;
                PlayLength = Mathf.Clamp(adventureNormalPlayLength, 1, _mapFiles.Count - Life + 1);
                break;
            case Mode.AdvHard:
                _mapFiles = SeriesToMapFiles(adventureHardMapSeries, adventureHardMapFiles);
                IsRandomOrder = isRandomOrder;
                Life = adventureHardLife;
                PlayLength = Mathf.Clamp(adventureHardPlayLength, 1, _mapFiles.Count - Life + 1);
                break;
            case Mode.AdvInsane:
                _mapFiles = SeriesToMapFiles(adventureInsaneMapSeries, adventureInsaneMapFiles);
                IsRandomOrder = isRandomOrder;
                Life = adventureInsaneLife;
                PlayLength = Mathf.Clamp(adventureInsanePlayLength, 1, _mapFiles.Count - Life + 1);
                break;
            case Mode.Custom:
                CustomOpenPhase();
                Life = int.MaxValue;
                break;
            case Mode.Training:
                TrainingOpenPhase(TrainingMapSelect.Root);
                Life = int.MaxValue;
                break;
            default:
                IsRandomOrder = isRandomOrder;
                Life = Mathf.Max(initialLife, 1);
                PlayLength = Mathf.Clamp(maxPlayLength, 1, _mapFiles.Count - Life + 1);
                // TODO
                return;
        }

        if (_mapFiles == null || _mapFiles.Count < 1 || maxPlayLength < 1) return;

        //Debug.Log("Remaining life: " + Life);

        if (IsRandomOrder)
        {
            List<TextAsset> tempList = _mapFiles.OrderBy(_ => UnityEngine.Random.value).ToList();
            _mapFiles = tempList;
        }
        IsReady = true;
    }

    private List<TextAsset> SeriesToMapFiles(List<string> series, List<TextAsset> maps)
    {
        List<TextAsset> mapFiles = new List<TextAsset>();
        foreach (string s in series)
        {
            string[] splits = s.Split(',');
            int index = int.Parse(splits[UnityEngine.Random.Range(0, splits.Length)]);
            mapFiles.Add(maps[index]);
        }
        return mapFiles;
    }

    public void Pause()
    {
        pauseButton.interactable = false;
        pausePanel.SetActive(true);
        pauseUI.gameObject.SetActive(true);
        GameManager.mm.TimePause();
        if (SceneManager.GetActiveScene().name == "Adventure" || SceneManager.GetActiveScene().name == "Tutorial")
        {
            /*
            pauseUI.Initialize(
                () => resultUI.Initialize(playMode),
                () =>
                {
                    GameManager.mm.TimeResume();
                    pausePanel.SetActive(false);
                    pauseButton.interactable = true;
                }
            );
            */
        }
        else if (SceneManager.GetActiveScene().name == "Custom")
        {
            /*
            pauseUI.Initialize(
                () => CustomIngameToOpen(),
                () =>
                {
                    GameManager.mm.TimeResume();
                    pausePanel.SetActive(false);
                    pauseButton.interactable = true;
                }
            );
            */
        }
        // TODO
        /* else if (SceneManager.GetActiveScene().name == "Training")
        {
            messageUI.Initialize("<b>Paused</b>\n\nDo you want to quit game?",
                  () => TrainingIngameToOpen(),
                  () =>
                  {
                      GameManager.mm.TimeResume();
                      messagePanel.SetActive(false);
                      pauseButton.interactable = true;
                  })
                      ;
        } */
    }

    public void Quit()
    {
        if (SceneManager.GetActiveScene().name.Equals("Tutorial") || SceneManager.GetActiveScene().name.Equals("Custom") ||
            SceneManager.GetActiveScene().name.Equals("Training"))
        {
            GameManager.gm.LoadMode();
        }
        else if (SceneManager.GetActiveScene().name.Equals("Adventure"))
        {
            GameManager.gm.LoadAdventureLevel();
        }
        else 
        {
            GameManager.gm.LoadMain();
        }
    }

    public void Ending()
    {
        if (SceneManager.GetActiveScene().name.Equals("Tutorial") && HasClearedAll)
        {
            //StreamReader sr = null;
            StreamWriter sw = null;
            //bool hasReadSuccess = true;

            /*
            try
            {
                sr = new StreamReader(Application.persistentDataPath + "/TutorialDone.txt");
                sr.ReadLine();
            }
            catch (Exception)
            {
                Debug.LogWarning("File invalid: TutorialDone.txt seems to be corrupted");
                hasReadSuccess = false;
            }
            finally
            {
                try
                {
                    sr.Close();
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
            */

            try
            {
                sw = new StreamWriter(Application.persistentDataPath + "/TutorialDone.txt");
                sw.WriteLine("3");
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
            finally
            {
                try
                {
                    sw.Close();
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
        }
        resultUI.Initialize(playMode);
    }

    public void TutorialNext()
    {
        GameManager.gm.TutorialNext();
        if (HasClearedAll)
        {
            Ending();
        }
        /*
        Debug.Log("initialMapCoord");
        for (int i = 0; i < GameManager.mm.SizeX; i++)
        {
            for (int j = 0; j < GameManager.mm.SizeY; j++)
            {
                Debug.Log("(" + i + ", " + j + "): " + GameManager.mm.initialMapCoord[i, j]);
            }
        }
        Debug.Log("currentMapCoord");
        for (int i = 0; i < GameManager.mm.SizeX; i++)
        {
            for (int j = 0; j < GameManager.mm.SizeY; j++)
            {
                Debug.Log("(" + i + ", " + j + "): " + GameManager.mm.currentMapCoord[i, j]);
            }
        }
        Debug.Log("map.mapCoord");
        for (int i = 0; i < GameManager.mm.SizeX; i++)
        {
            for (int j = 0; j < GameManager.mm.SizeY; j++)
            {
                Debug.Log("(" + i + ", " + j + "): " + GameManager.mm.map.mapCoord[i, j]);
            }
        }
        */
    }

    public void PlayNext()
    {
        if (IsHurt)
        {
            SkippedCount++;
            IsHurt = false;
        }
        GameManager.gm.PlayNext();
        if (HasClearedAll)
        {
            Ending();
        }
    }

    public void PlayRetryWithTime()
    {
        if (IsHurt)
        {
            IsHurt = false;
        }
    }
    
    public void TutorialRetryWithTime()
    {
        if (EscapedCount > 0) EscapedCount--;
    }

    public void TutorialAfterGravity(MapManager.Flag flag)
    {
        if(tutorialGuide != null){
            tutorialGuide.SpecificCaseGuide(flag);
        }
        switch (flag)
        {
            case MapManager.Flag.Continued:
                retryButton.gameObject.SetActive(true);
                retryHighlightedButton.gameObject.SetActive(false);
                retryTimeButton.gameObject.SetActive(false);
                retryTimeHighlightedButton.gameObject.SetActive(false);
                nextButton.gameObject.SetActive(true);
                quitHighlightedButton.gameObject.SetActive(false);

                pauseButton.interactable = true;
                nextButton.interactable = false;
                break;
            case MapManager.Flag.Escaped:
                retryButton.gameObject.SetActive(false);
                retryHighlightedButton.gameObject.SetActive(false);
                retryTimeButton.gameObject.SetActive(true);
                retryTimeHighlightedButton.gameObject.SetActive(false);
                EscapedCount++;
                if (!HasClearedAll)
                {
                    // 다음 맵이 존재할 때
                    nextButton.gameObject.SetActive(true);
                    quitHighlightedButton.gameObject.SetActive(false);

                    pauseButton.interactable = true;
                    nextButton.interactable = true;
                }
                else
                {
                    // 모든 맵을 탈출했을 때
                    nextButton.gameObject.SetActive(false);
                    quitHighlightedButton.gameObject.SetActive(true);

                    pauseButton.interactable = false;
                }
                break;
            case MapManager.Flag.Burned:
            case MapManager.Flag.Squashed:
                retryButton.gameObject.SetActive(false);
                retryHighlightedButton.gameObject.SetActive(true);
                retryTimeButton.gameObject.SetActive(false);
                retryTimeHighlightedButton.gameObject.SetActive(false);
                nextButton.gameObject.SetActive(true);
                quitHighlightedButton.gameObject.SetActive(false);

                pauseButton.interactable = true;
                nextButton.interactable = false;
                break;
            case MapManager.Flag.TimeOver:
                retryButton.gameObject.SetActive(false);
                retryHighlightedButton.gameObject.SetActive(false);
                retryTimeButton.gameObject.SetActive(false);
                retryTimeHighlightedButton.gameObject.SetActive(true);
                nextButton.gameObject.SetActive(true);
                quitHighlightedButton.gameObject.SetActive(false);

                pauseButton.interactable = true;
                nextButton.interactable = false;
                break;
        }
    }

    public void AdventureAfterGravity(MapManager.Flag flag)
    {
        switch (flag)
        {
            case MapManager.Flag.Continued:
                retryButton.gameObject.SetActive(true);
                retryHighlightedButton.gameObject.SetActive(false);
                retryTimeButton.gameObject.SetActive(false);
                retryTimeHighlightedButton.gameObject.SetActive(false);
                nextButton.gameObject.SetActive(true);
                quitHighlightedButton.gameObject.SetActive(false);

                pauseButton.interactable = true;
                nextButton.interactable = false;
                break;
            case MapManager.Flag.Escaped:
                retryButton.gameObject.SetActive(false);
                retryHighlightedButton.gameObject.SetActive(false);
                retryTimeButton.gameObject.SetActive(false);
                retryTimeHighlightedButton.gameObject.SetActive(false);
                EscapedCount++;
                if (!HasClearedAll)
                {
                    // 다음 맵이 존재할 때
                    nextButton.gameObject.SetActive(true);
                    quitHighlightedButton.gameObject.SetActive(false);

                    pauseButton.interactable = true;
                    nextButton.interactable = true;
                }
                else
                {
                    // 모든 맵을 탈출했을 때
                    nextButton.gameObject.SetActive(false);
                    quitHighlightedButton.gameObject.SetActive(true);

                    pauseButton.interactable = false;
                }
                break;
            case MapManager.Flag.Burned:
            case MapManager.Flag.Squashed:
                retryButton.gameObject.SetActive(false);
                retryHighlightedButton.gameObject.SetActive(true);
                retryTimeButton.gameObject.SetActive(false);
                retryTimeHighlightedButton.gameObject.SetActive(false);
                nextButton.gameObject.SetActive(true);
                quitHighlightedButton.gameObject.SetActive(false);

                pauseButton.interactable = true;
                nextButton.interactable = false;
                break;
            case MapManager.Flag.TimeOver:
                Life--;
                IsHurt = true;
                Debug.Log("Remaining life: " + Life);
                retryButton.gameObject.SetActive(false);
                retryHighlightedButton.gameObject.SetActive(false);
                retryTimeButton.gameObject.SetActive(false);
                if (Life > 0)
                {
                    // 라이프가 남아있을 때
                    retryTimeHighlightedButton.gameObject.SetActive(true);
                    nextButton.gameObject.SetActive(true);
                    quitHighlightedButton.gameObject.SetActive(false);

                    pauseButton.interactable = true;
                    nextButton.interactable = true;
                }
                else
                {
                    // 라이프가 0일 때
                    retryTimeHighlightedButton.gameObject.SetActive(false);
                    nextButton.gameObject.SetActive(false);
                    quitHighlightedButton.gameObject.SetActive(true);

                    pauseButton.interactable = false;
                }
                break;
        }
    }

    public void CustomAfterGravity(MapManager.Flag flag)
    {
        switch (flag)
        {
            case MapManager.Flag.Continued:
                retryButton.gameObject.SetActive(true);
                retryHighlightedButton.gameObject.SetActive(false);
                retryTimeButton.gameObject.SetActive(false);
                retryTimeHighlightedButton.gameObject.SetActive(false);
                nextButton.gameObject.SetActive(true);
                quitHighlightedButton.gameObject.SetActive(false);

                pauseButton.interactable = true;
                nextButton.interactable = false;
                break;
            case MapManager.Flag.Escaped:
                retryButton.gameObject.SetActive(false);
                retryHighlightedButton.gameObject.SetActive(false);
                retryTimeButton.gameObject.SetActive(false);
                retryTimeHighlightedButton.gameObject.SetActive(false);

                nextButton.gameObject.SetActive(false);
                quitHighlightedButton.gameObject.SetActive(true);

                pauseButton.interactable = false;
                break;
            case MapManager.Flag.Burned:
            case MapManager.Flag.Squashed:
                retryButton.gameObject.SetActive(false);
                retryHighlightedButton.gameObject.SetActive(true);
                retryTimeButton.gameObject.SetActive(false);
                retryTimeHighlightedButton.gameObject.SetActive(false);
                nextButton.gameObject.SetActive(true);
                quitHighlightedButton.gameObject.SetActive(false);

                pauseButton.interactable = true;
                nextButton.interactable = false;
                break;
            case MapManager.Flag.TimeOver:
                retryButton.gameObject.SetActive(false);
                retryHighlightedButton.gameObject.SetActive(false);
                retryTimeButton.gameObject.SetActive(false);
                retryTimeHighlightedButton.gameObject.SetActive(true);
                nextButton.gameObject.SetActive(true);
                quitHighlightedButton.gameObject.SetActive(false);

                pauseButton.interactable = true;
                nextButton.interactable = true;

                break;
        }
    }

    public void TrainingAfterGravity(MapManager.Flag flag)
    {
        switch (flag)
        {
            case MapManager.Flag.Continued:
                retryButton.gameObject.SetActive(true);
                retryHighlightedButton.gameObject.SetActive(false);
                retryTimeButton.gameObject.SetActive(false);
                retryTimeHighlightedButton.gameObject.SetActive(false);
                nextButton.gameObject.SetActive(true);
                quitHighlightedButton.gameObject.SetActive(false);

                pauseButton.interactable = true;
                nextButton.interactable = false;
                break;
            case MapManager.Flag.Escaped:
                retryButton.gameObject.SetActive(false);
                retryHighlightedButton.gameObject.SetActive(false);
                retryTimeButton.gameObject.SetActive(false);
                retryTimeHighlightedButton.gameObject.SetActive(false);

                nextButton.gameObject.SetActive(false);
                quitHighlightedButton.gameObject.SetActive(true);

                pauseButton.interactable = false;
                break;
            case MapManager.Flag.Burned:
            case MapManager.Flag.Squashed:
                retryButton.gameObject.SetActive(false);
                retryHighlightedButton.gameObject.SetActive(true);
                retryTimeButton.gameObject.SetActive(false);
                retryTimeHighlightedButton.gameObject.SetActive(false);
                nextButton.gameObject.SetActive(true);
                quitHighlightedButton.gameObject.SetActive(false);

                pauseButton.interactable = true;
                nextButton.interactable = false;
                break;
            case MapManager.Flag.TimeOver:
                retryButton.gameObject.SetActive(false);
                retryHighlightedButton.gameObject.SetActive(false);
                retryTimeButton.gameObject.SetActive(false);
                retryTimeHighlightedButton.gameObject.SetActive(true);
                nextButton.gameObject.SetActive(true);
                quitHighlightedButton.gameObject.SetActive(false);

                pauseButton.interactable = true;
                nextButton.interactable = true;

                break;
        }
    }

    public void PlayButtonSFX()
    {
        GameManager.gm.PlayButtonSFX();
    }

    public void SetBGMVolume(Slider slider)
    {
        GameManager.gm.bgmVolume = slider.value;
    }

    public void SetSFXVolume(Slider slider)
    {
        GameManager.gm.sfxVolume = slider.value;
    }

    public void CustomOpenPhase()
    {
        customPhase = CustomPhase.Open;
        GameManager.gm.CustomChangeBGM(customPhase);

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
            statusUI.SetStatusMessageWithFlashing(e.Message, 2f);
            return;
        }
#endif

        try
        {
            if (!Directory.Exists(MapManager.MAP_ROOT_PATH))
            {
                Debug.LogWarning("File warning: there is no directory \"" + MapManager.MAP_ROOT_PATH + "\"");
                Directory.CreateDirectory(MapManager.MAP_ROOT_PATH);
            }
        }
        catch (Exception e)
        {
            statusUI.SetStatusMessageWithFlashing(e.Message, 2f);
            Debug.LogError(e.Message);
            return;
        }

        RenderOpenScrollView(MapManager.MAP_ROOT_PATH);

        statusUI.SetStatusMessage("Choose a map to open.");
    }

    public void TrainingOpenPhase(TrainingMapSelect selected) // 만들다 만 함수
    {
        trainingPhase = TrainingPhase.Open;
        GameManager.gm.TrainingChangeBGM(trainingPhase);

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
            statusUI.SetStatusMessageWithFlashing(e.Message, 2f);
            return;
        }
#endif
        RenderOpenScrollViewForTraining(selected);

        statusUI.SetStatusMessage("Choose a map to open");


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
            statusUI.SetStatusMessageWithFlashing("The path doesn't exist anymore.", 2f);
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
            openPathText.text = currentOpenPath;
        }
        else if (currentPath.Length <= 17)
        {
            string tempPath = currentOpenPath.Substring(currentOpenPath.Length - 17);
            tempPath = tempPath.Substring(tempPath.IndexOf('/') + 1);
            openPathText.text = ".../" + tempPath;
        }
        else
        {
            openPathText.text = ".../" + currentPath.Remove(14) + "...";
        }

        openScrollContent.GetComponent<RectTransform>().sizeDelta =
            new Vector2(openScrollContent.GetComponent<RectTransform>().sizeDelta.x, SCROLL_ITEM_HEIGHT * length);

        if (!openPath.TrimEnd('/').Equals(MapManager.MAP_ROOT_PATH.TrimEnd('/')))
        {
            GameObject g = Instantiate(openScrollItemPrefab, openScrollContent.transform);
            g.GetComponent<RectTransform>().offsetMin = new Vector2(12f, -SCROLL_ITEM_HEIGHT / 2);
            g.GetComponent<RectTransform>().offsetMax = new Vector2(-12f, SCROLL_ITEM_HEIGHT / 2);
            g.GetComponent<RectTransform>().anchoredPosition =
                new Vector3(g.GetComponent<RectTransform>().anchoredPosition.x, (SCROLL_ITEM_HEIGHT / 2) * (length - 1 - 2 * index), 0f);

            g.GetComponent<OpenSaveScrollItem>().Initialize(OpenSaveScrollItem.Type.Open, currentOpenPath.Remove(currentOpenPath.LastIndexOf('/')), true, this, true);
            index++;
        }

        if (dirs != null)
        {
            foreach (string s in dirs)
            {
                GameObject g = Instantiate(openScrollItemPrefab, openScrollContent.transform);
                g.GetComponent<RectTransform>().offsetMin = new Vector2(12f, -SCROLL_ITEM_HEIGHT / 2);
                g.GetComponent<RectTransform>().offsetMax = new Vector2(-12f, SCROLL_ITEM_HEIGHT / 2);
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
                GameObject g = Instantiate(openScrollItemPrefab, openScrollContent.transform);
                g.GetComponent<RectTransform>().offsetMin = new Vector2(12f, -SCROLL_ITEM_HEIGHT / 2);
                g.GetComponent<RectTransform>().offsetMax = new Vector2(-12f, SCROLL_ITEM_HEIGHT / 2);
                g.GetComponent<RectTransform>().anchoredPosition =
                    new Vector3(g.GetComponent<RectTransform>().anchoredPosition.x, (SCROLL_ITEM_HEIGHT / 2) * (length - 1 - 2 * index), 0f);
                g.GetComponent<OpenSaveScrollItem>().Initialize(OpenSaveScrollItem.Type.Open, s, false, this);
                index++;
            }
        }

        openScrollbar.numberOfSteps = Mathf.Max(1, length - 4);

        if (length == 0)
        {
            openScrollEmptyText.GetComponent<RectTransform>().offsetMax = new Vector3(0f, 0f, 0f);
            openScrollEmptyText.SetActive(true);
        }
        else if (!isRoot && length == 1)
        {
            openScrollEmptyText.GetComponent<RectTransform>().offsetMax = new Vector3(0f, -42f, 0f);
            openScrollEmptyText.SetActive(true);
        }
        else
        {
            openScrollEmptyText.SetActive(false);
        }
    }

    private void RenderOpenScrollViewForTraining(TrainingMapSelect selection)
    {
        ClearOpenScrollItems();

        const float SCROLL_ITEM_HEIGHT = 84f;

        List<TextAsset> files = new List<TextAsset>();
        int index = 0;
        int length = 0;
        bool isRoot = true;
        string name = "";

        switch (selection)
        {
            case TrainingMapSelect.Root:
                length = trainingFolders.Count;
                isRoot = true;
                openPathText.text = "Maps";
                break;
            case TrainingMapSelect.Basic:
                files = trainingBasicMapFiles;
                length = files.Count + 1;
                isRoot = false;
                openPathText.text = "Maps/Basic";
                name = "Basic";
                break;
            case TrainingMapSelect.Fire:
                files = trainingFireMapFiles;
                length = files.Count + 1;
                isRoot = false;
                openPathText.text = "Maps/Fire";
                name = "Fire";
                break;
            case TrainingMapSelect.Iron:
                files = trainingIronMapFiles;
                length = files.Count + 1;
                isRoot = false;
                openPathText.text = "Maps/Iron";
                name = "Iron";
                break;
            case TrainingMapSelect.Block:
                files = trainingBlockMapFiles;
                length = files.Count + 1;
                isRoot = false;
                openPathText.text = "Maps/Block";
                name = "Block";
                break;
            case TrainingMapSelect.Exit:
                files = trainingExitMapFiles;
                length = files.Count + 1;
                isRoot = false;
                openPathText.text = "Maps/Exit";
                name = "Exit";
                break;
            case TrainingMapSelect.Shutter:
                files = trainingShutterMapFiles;
                length = files.Count + 1;
                isRoot = false;
                openPathText.text = "Maps/Shutter";
                name = "Shutter";
                break;
            case TrainingMapSelect.Gate:
                files = trainingGateMapFiles;
                length = files.Count + 1;
                isRoot = false;
                openPathText.text = "Maps/Gate";
                name = "Gate";
                break;
        }

        openScrollContent.GetComponent<RectTransform>().sizeDelta =
            new Vector2(openScrollContent.GetComponent<RectTransform>().sizeDelta.x, SCROLL_ITEM_HEIGHT * length);

        if (!isRoot)
        {
            GameObject g = Instantiate(openScrollItemPrefab, openScrollContent.transform);
            g.GetComponent<RectTransform>().offsetMin = new Vector2(12f, -SCROLL_ITEM_HEIGHT / 2);
            g.GetComponent<RectTransform>().offsetMax = new Vector2(-12f, SCROLL_ITEM_HEIGHT / 2);
            g.GetComponent<RectTransform>().anchoredPosition =
                new Vector3(g.GetComponent<RectTransform>().anchoredPosition.x, (SCROLL_ITEM_HEIGHT / 2) * (length - 1 - 2 * index), 0f);

            g.GetComponent<OpenSaveScrollItem>().Initialize(name, true, this, null, true);
            index++;
        }

        if (selection == TrainingMapSelect.Root)
        {
            foreach (string s in trainingFolders)
            {
                GameObject g = Instantiate(openScrollItemPrefab, openScrollContent.transform);
                g.GetComponent<RectTransform>().offsetMin = new Vector2(12f, -SCROLL_ITEM_HEIGHT / 2);
                g.GetComponent<RectTransform>().offsetMax = new Vector2(-12f, SCROLL_ITEM_HEIGHT / 2);
                g.GetComponent<RectTransform>().anchoredPosition =
                    new Vector3(g.GetComponent<RectTransform>().anchoredPosition.x, (SCROLL_ITEM_HEIGHT / 2) * (length - 1 - 2 * index), 0f);

                g.GetComponent<OpenSaveScrollItem>().Initialize(s, true, this, null, false);
                index++;
            }
        }

        else if (files.Count > 0)
        {
            foreach (TextAsset s in files)
            {
                GameObject g = Instantiate(openScrollItemPrefab, openScrollContent.transform);
                g.GetComponent<RectTransform>().offsetMin = new Vector2(12f, -SCROLL_ITEM_HEIGHT / 2);
                g.GetComponent<RectTransform>().offsetMax = new Vector2(-12f, SCROLL_ITEM_HEIGHT / 2);
                g.GetComponent<RectTransform>().anchoredPosition =
                    new Vector3(g.GetComponent<RectTransform>().anchoredPosition.x, (SCROLL_ITEM_HEIGHT / 2) * (length - 1 - 2 * index), 0f);
                g.GetComponent<OpenSaveScrollItem>().Initialize(s.name, false, this, s, false);
                index++;
            }
        }

        openScrollbar.numberOfSteps = Mathf.Max(1, length - 4);
    }

    private void ClearOpenScrollItems()
    {
        selectedOpenScrollItem = null;
        foreach (OpenSaveScrollItem i in openScrollContent.GetComponentsInChildren<OpenSaveScrollItem>())
        {
            Destroy(i.gameObject);
        }

        openButton.gameObject.SetActive(true);
        openHighlightedButton.gameObject.SetActive(false);

        openButton.interactable = false;
        openHighlightedButton.interactable = false;
    }

    public void OpenItemSelect(OpenSaveScrollItem caller)
    {
        float selectTime = Time.time;
        if (caller != null && caller.Equals(selectedOpenScrollItem) &&
            openItemSelectTime > 0f && selectTime - openItemSelectTime < 0.5f)
        {
            // Double click
            if (SceneManager.GetActiveScene().name == "Custom")
            { 
                CustomOpen();
            }
            else if (SceneManager.GetActiveScene().name == "Training")
            { 
                TrainingOpen();
            }
            return;
        }
        openItemSelectTime = selectTime;

        foreach (OpenSaveScrollItem i in openScrollContent.GetComponentsInChildren<OpenSaveScrollItem>())
        {
            i.isSelected = false;
        }
        caller.isSelected = true;
        selectedOpenScrollItem = caller;

        if (caller.isFolder)
        {
            openButton.gameObject.SetActive(true);
            openHighlightedButton.gameObject.SetActive(false);
            openButton.interactable = true;
            GameManager.mm.Initialize();
        }
        else
        {
            if (caller.type == OpenSaveScrollItem.Type.Open)
            {
                bool b = CustomOpenFile(selectedOpenScrollItem.path, true);
                openHighlightedButton.gameObject.SetActive(b);
                openButton.gameObject.SetActive(!b);
                openButton.interactable = b;
                openHighlightedButton.interactable = b;
            }
            else if (caller.type == OpenSaveScrollItem.Type.TrainingOpen)
            {
                bool b = TrainingOpenFile(selectedOpenScrollItem.textAsset, true);
                openHighlightedButton.gameObject.SetActive(b);
                openButton.gameObject.SetActive(!b);
                openButton.interactable = b;
                openHighlightedButton.interactable = b;
            }
        }
    }

    public void CustomOpen()
    {
        if (customPhase != CustomPhase.Open || selectedOpenScrollItem is null) return;

        if (selectedOpenScrollItem.isFolder)
        {
            RenderOpenScrollView(selectedOpenScrollItem.path);
            GameManager.mm.Initialize();
        }
        else
        {
            CustomOpenFile(selectedOpenScrollItem.path, false);
        }
    }

    public void TrainingOpen()
    {
        if (trainingPhase != TrainingPhase.Open || selectedOpenScrollItem is null) return;

        //TrainingMapSelect selection = new TrainingMapSelect();

        if (selectedOpenScrollItem.isFolder)
        {
            if (selectedOpenScrollItem.isUpOneLevel)
                selection = TrainingMapSelect.Root;
            else
            {
                switch (selectedOpenScrollItem.labelName)
                {
                    case "Basic":
                        selection = TrainingMapSelect.Basic;
                        break;
                    case "Fire":
                        selection = TrainingMapSelect.Fire;
                        break;
                    case "Iron":
                        selection = TrainingMapSelect.Iron;
                        break;
                    case "Block":
                        selection = TrainingMapSelect.Block;
                        break;
                    case "Exit":
                        selection = TrainingMapSelect.Exit;
                        break;
                    case "Shutter":
                        selection = TrainingMapSelect.Shutter;
                        break;
                    case "Gate":
                        selection = TrainingMapSelect.Gate;
                        break;
                }
            }
            RenderOpenScrollViewForTraining(selection);
            GameManager.mm.Initialize();
        }
        else
        {
            TrainingOpenFile(selectedOpenScrollItem.textAsset, false);
        }
    }

    public bool CustomOpenFile(string path, bool isPreview)
    {
        MapManager.OpenFileFlag openFileFlag = GameManager.mm.InitializeFromFile(path, out int sizeX, out int sizeY,
            out List<ObjectInfo> objects, out List<WallInfo> walls,
            out string solution, out float timeLimit, statusUI);

        switch (openFileFlag)
        {
            case MapManager.OpenFileFlag.Restore:
                GameManager.mm.Initialize();
                return false;
            case MapManager.OpenFileFlag.Success:
                if (isPreview)
                {
                    return true;
                }
                else
                {
                    GameManager.mm.Initialize(sizeX, sizeY, walls, objects, solution, timeLimit);

                    openUI.SetActive(false);
                    ingameUI.SetActive(true);
                    customPhase = CustomPhase.Ingame;
                    GameManager.gm.CustomChangeBGM(customPhase);

                    ClearOpenScrollItems();

                    foreach (var t in tooltipUI.GetComponentsInChildren<TooltipBox>())
                    {
                        Destroy(t.gameObject);
                    }
                    statusUI.gameObject.SetActive(false);
                    timerUI.SetActive(true);
                    GameManager.gm.canPlay = true;
                    GameManager.mm.TimeActivate();

                    return true;
                }
            case MapManager.OpenFileFlag.Failed:
            default:
                return false;
        }
    }
    public bool TrainingOpenFile(TextAsset textAsset, bool isPreview)
    {
        MapManager.OpenFileFlag openFileFlag = GameManager.mm.InitializeFromText(textAsset.text, out int sizeX, out int sizeY,
            out List<ObjectInfo> objects, out List<WallInfo> walls,
            out string solution, out float timeLimit, statusUI);

        switch (openFileFlag)
        {
            case MapManager.OpenFileFlag.Restore:
                GameManager.mm.Initialize();
                return false;
            case MapManager.OpenFileFlag.Success:
                if (isPreview)
                {
                    return true;
                }
                else
                {
                    GameManager.mm.Initialize(sizeX, sizeY, walls, objects, solution, timeLimit);

                    openUI.SetActive(false);
                    ingameUI.SetActive(true);
                    trainingPhase = TrainingPhase.Ingame;
                    GameManager.gm.TrainingChangeBGM(trainingPhase);

                    ClearOpenScrollItems();

                    foreach (var t in tooltipUI.GetComponentsInChildren<TooltipBox>())
                    {
                        Destroy(t.gameObject);
                    }
                    statusUI.gameObject.SetActive(false);
                    timerUI.SetActive(true);
                    GameManager.gm.canPlay = true;
                    GameManager.mm.TimeActivate();




                    return true;
                }
            case MapManager.OpenFileFlag.Failed:
            default:
                return false;
        }

    }
    public void CustomIngameToOpen()
    {
        if (customPhase != CustomPhase.Ingame) return;

        ingameUI.SetActive(false);
        openUI.SetActive(true);
        GameManager.mm.Initialize();
        statusUI.gameObject.SetActive(true);
        timerUI.SetActive(false);
        nextButton.interactable = false;
        pauseButton.interactable = true;
        pausePanel.SetActive(false);
        CustomOpenPhase();
        customPhase = CustomPhase.Open;
        GameManager.gm.CustomChangeBGM(customPhase);
        GameManager.gm.canPlay = false;
    }

    public void TrainingIngameToOpen()
    {
        if (trainingPhase != TrainingPhase.Ingame) return;

        ingameUI.SetActive(false);
        openUI.SetActive(true);
        GameManager.mm.Initialize();
        statusUI.gameObject.SetActive(true);
        timerUI.SetActive(false);
        nextButton.interactable = false;
        pauseButton.interactable = true;
        // TODO
        // messagePanel.SetActive(false);
        TrainingOpenPhase(selection);
        trainingPhase = TrainingPhase.Open;
        GameManager.gm.TrainingChangeBGM(trainingPhase);
        GameManager.gm.canPlay = false;
    }
}
