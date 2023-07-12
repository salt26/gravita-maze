using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Localization.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

public class PlayManager : MonoBehaviour
{
    public string tableName = "StringTable";

    public enum Mode { Tutorial = 0, Custom = 1, Training = 2,
        AdvEasy = 11, AdvNormal = 12, AdvHard = 13, AdvInsane = 14 }

    public enum CustomPhase { Default = 0, Open = 1, Ingame = 2 }
    public enum TrainingPhase { Default = 0, Open = 1, Ingame = 2 }

    public enum TrainingMapSelect { Root = -1, Basic = 0, Fire = 1, Iron = 2, Block = 3, Exit = 4, Shutter = 5, Gate = 6 }
   
    public Button pauseButton;                   // quitHighlightedButton이 활성화될 때 비활성화
    public Button quitHighlightedButton;        // 모든 맵을 탈출하거나 라이프가 0이 되어 게임이 종료될 때 활성화
    public Button nextButton;                   // 탈출 또는 시간 초과 시 활성화 (튜토리얼에서는 탈출 시에만 활성화), quitHighlightedButton이 활성화될 때 비활성화
    public Button retryButton;                  // Continued일 때 활성화, 사망 또는 탈출 또는 시간 초과 시 비활성화
    public Button retryHighlightedButton;       // Burned 또는 Squashed일 때 활성화
    public Button retryTimeButton;              // 시간 초과 시 활성화 (튜토리얼에서는 탈출 시 활성화)
    public Button retryTimeHighlightedButton;   // (튜토리얼에서만 시간 초과 시 활성화)
    public MessageUI messageUI;
    public PauseUI pauseUI;
    public GameObject pausePanel;
    public ResultUI resultUI;
    public GameObject tooltipUI;
    public GameObject timerUI;
    public GameObject moveLimitUI;

    public List<GameObject> tryCountUis = new List<GameObject>();
    public List<GameObject> minMoveCountUis = new List<GameObject>();

    private OpenScrollItemWithMark selectedOpenScrollItem;
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
    public CustomPhase customPhase;
    public TrainingPhase trainingPhase;

    private string customSelection;
    private TrainingMapSelect selection;
    private string mapPath;
    private FileStream fileStream;
    private StreamWriter streamWriter;
    private StreamReader streamReader;
    private string mapHash;
    private string metaPath;

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
                CustomOpenPhase(MapManager.MAP_ROOT_PATH);
                Life = int.MaxValue;
                customSelection = MapManager.MAP_ROOT_PATH;
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

    private void Update() {
        if (GameManager.mm == null || !GameManager.mm.IsReady) return;
        if (SceneManager.GetActiveScene().name.Equals("Custom") || SceneManager.GetActiveScene().name.Equals("Training")) 
        {
            if (GameManager.mm.tryCountUpTrigger) {
                //Debug.Log("TryCountUp in PM ");
                GameManager.mm.TryCountUp(this, metaPath, mapHash);
            }
        }

    }

    public void Resume()
    {
        GameManager.gm.SaveSettingsValue();
        GameManager.gm.canPlay = true;
    }
    public void Pause()
    {
        GameManager.gm.canPlay = false;
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
                sw = new StreamWriter(Application.persistentDataPath.TrimEnd('/') + "/TutorialDone.txt");
                sw.WriteLine("3");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                try
                {
                    sw.Close();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
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
                retryButton.gameObject.SetActive(true);
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
                retryButton.gameObject.SetActive(true);
                retryHighlightedButton.gameObject.SetActive(false);
                retryTimeButton.gameObject.SetActive(false);
                retryTimeHighlightedButton.gameObject.SetActive(false);

                nextButton.gameObject.SetActive(false);
                quitHighlightedButton.gameObject.SetActive(true);

                pauseButton.interactable = false;

                if (GameManager.mm.LimitMode == MapManager.LimitModeEnum.Time && !GameManager.mm.hasClearedOnceInTime)
                {
                    GameManager.mm.hasClearedOnceInTime = true;
                    Dictionary<string, object> keyValuePairs = new Dictionary<string, object>();
                    if (GameManager.mm.ActionHistory.Length == 1)
                    {
                        keyValuePairs.Add("tryCount", GameManager.mm.tryCount + 1);
                    }
                    else
                    {
                        keyValuePairs.Add("tryCount", GameManager.mm.tryCount);
                    }
                    keyValuePairs.Add("hasClearedOnce", GameManager.mm.hasClearedOnceInTime);
                    MetaUtil.ModifyMetaFile(metaPath, mapHash, MapManager.LimitModeEnum.Time, keyValuePairs);
                }
                else if (GameManager.mm.LimitMode == MapManager.LimitModeEnum.Move)
                {
                    GameManager.mm.hasClearedOnceInMove = true;
                    Dictionary<string, object> keyValuePairs = new Dictionary<string, object>();
                    if (GameManager.mm.ActionHistory.Length < GameManager.mm.MoveLimit)
                    {
                        // TODO 특별한 애니메이션으로 최소 이동 횟수 갱신 축하하기!
                        keyValuePairs.Add("minMoveCount", GameManager.mm.ActionHistory.Length);
                    }
                    keyValuePairs.Add("hasClearedOnce", GameManager.mm.hasClearedOnceInMove);
                    MetaUtil.ModifyMetaFile(metaPath, mapHash, MapManager.LimitModeEnum.Move, keyValuePairs);
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
                retryButton.gameObject.SetActive(true);
                retryHighlightedButton.gameObject.SetActive(false);
                retryTimeButton.gameObject.SetActive(false);
                retryTimeHighlightedButton.gameObject.SetActive(false);

                nextButton.gameObject.SetActive(false);
                quitHighlightedButton.gameObject.SetActive(true);

                pauseButton.interactable = false;

                if (GameManager.mm.LimitMode == MapManager.LimitModeEnum.Time && !GameManager.mm.hasClearedOnceInTime)
                {
                    GameManager.mm.hasClearedOnceInTime = true;
                    Dictionary<string, object> keyValuePairs = new Dictionary<string, object>();
                    if (GameManager.mm.ActionHistory.Length == 1)
                    {
                        keyValuePairs.Add("tryCount", GameManager.mm.tryCount + 1);
                    }
                    else
                    {
                        keyValuePairs.Add("tryCount", GameManager.mm.tryCount);
                    }
                    keyValuePairs.Add("hasClearedOnce", GameManager.mm.hasClearedOnceInTime);
                    MetaUtil.ModifyMetaFile(metaPath, mapHash, MapManager.LimitModeEnum.Time, keyValuePairs);
                }
                else if (GameManager.mm.LimitMode == MapManager.LimitModeEnum.Move)
                {
                    GameManager.mm.hasClearedOnceInMove = true;
                    Dictionary<string, object> keyValuePairs = new Dictionary<string, object>();
                    if (GameManager.mm.ActionHistory.Length < GameManager.mm.MoveLimit)
                    {
                        // TODO 특별한 애니메이션으로 최소 이동 횟수 갱신 축하하기!
                        keyValuePairs.Add("minMoveCount", GameManager.mm.ActionHistory.Length);
                    }
                    keyValuePairs.Add("hasClearedOnce", GameManager.mm.hasClearedOnceInMove);
                    MetaUtil.ModifyMetaFile(metaPath, mapHash, MapManager.LimitModeEnum.Move, keyValuePairs);
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
                //nextButton.gameObject.SetActive(true);
                quitHighlightedButton.gameObject.SetActive(false);

                pauseButton.interactable = true;
                //nextButton.interactable = true;

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

    public void CustomOpenPhase(string path)
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
            statusUI.SetStatusMessageWithFlashing(e.ToString(), 2f);
            return;
        }
#endif

        try
        {
            if (!Directory.Exists(path))
            {
                Debug.LogWarning("File warning: there is no directory \"" + path + "\"");
                MetaUtil.CreateDirectory(path);
            }
        }
        catch (Exception e)
        {
            statusUI.SetStatusMessageWithFlashing(e.ToString(), 2f);
            Debug.LogError(e);
            return;
        }

        RenderOpenScrollView(path);

        statusUI.SetStatusMessage(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_open_map"));
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
            statusUI.SetStatusMessageWithFlashing(e.ToString(), 2f);
            return;
        }
#endif
        RenderOpenScrollViewForTraining(selected);

        statusUI.SetStatusMessage(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "editor_open_map"));


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
        if (currentOpenPath.Length <= 14)
        {
            openPathText.text = currentOpenPath;
        }
        else if (currentPath.Length <= 10)
        {
            string tempPath = currentOpenPath.Substring(currentOpenPath.Length - 10);
            tempPath = tempPath.Substring(tempPath.IndexOf('/') + 1);
            openPathText.text = ".../" + tempPath;
        }
        else
        {
            openPathText.text = ".../" + currentPath.Remove(7) + "...";
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

            g.GetComponent<OpenScrollItemWithMark>().Initialize(OpenScrollItemWithMark.Type.Open, currentOpenPath.Remove(currentOpenPath.LastIndexOf('/')), true, this, true);
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

                g.GetComponent<OpenScrollItemWithMark>().Initialize(OpenScrollItemWithMark.Type.Open, s, true, this, false);
                index++;
            }
        }

        if (files != null)
        {
            string[] metafiles;
            SHA256 sha256Hash = SHA256.Create();
            //Debug.Log(openPath);
            string metaPath = Application.persistentDataPath.TrimEnd('/') + "/Meta/" + openPath.TrimStart('/');
            string path = Application.persistentDataPath;

            string[] dirs2 = ("Meta/" + openPath.Replace('\\', '/')).Split('/');
            int len = dirs2.Length;
            for (int i = 0; i < len; i++) {
                string dir = dirs2[i];
                path += '/' + dir;
                if (!Directory.Exists(path)) {
                    MetaUtil.CreateDirectory(path);
                }
            }

            metafiles = Directory.GetFiles(metaPath, "*.json");
            List<string> metafiles2 = metafiles.ToList<string>();

            foreach (string s in files)
            {
                bool isClearedInTime = false;
                bool isClearedInMove = false;
                string s2 = Application.persistentDataPath.TrimEnd('/') + "/Meta/" + MetaUtil.ExtentionTxtToJson(s);
                print(s2);

                if (metafiles2.Contains(s2))
                {
                    try
                    {
                        /*
                        FileStream fs = new FileStream(s2.Replace('\\', '/'), FileMode.Open);
                        StreamReader sr = new StreamReader(fs, Encoding.UTF8);
                        sr.ReadLine();
                        bool b = bool.TryParse(sr.ReadLine(), out isCleared);
                        if (!b) isCleared = false;
                        string metaHash = sr.ReadToEnd().Trim();
                        */

                        JObject json = MetaUtil.ReadMetaFile(s2.Replace('\\', '/'));
                        string metaHash = MetaUtil.GetMapHashFromMetaObject(json);

                        FileStream fileStream = new FileStream(s.Replace('\\', '/'), FileMode.Open);
                        StreamReader streamReader = new StreamReader(fileStream, Encoding.UTF8);
                        string mapHash = GetHash(sha256Hash, streamReader.ReadToEnd().Trim());
                        if (!metaHash.Equals(mapHash))
                        {
                            isClearedInTime = false;
                            isClearedInMove = false;
                        }
                        streamReader.Close();
                        fileStream.Close();

                        JToken cleared = MetaUtil.GetValueFromMetaObject(json, mapHash, MapManager.LimitModeEnum.Time, "hasClearedOnce");
                        if (cleared == null) isClearedInTime = false;
                        else isClearedInTime = (bool)cleared;

                        cleared = MetaUtil.GetValueFromMetaObject(json, mapHash, MapManager.LimitModeEnum.Move, "hasClearedOnce");
                        if (cleared == null) isClearedInMove = false;
                        else isClearedInMove = (bool)cleared;

                        /*
                        sr.Close();
                        fs.Close();
                        */
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                    metafiles2.Remove(s2);
                }

                GameObject g = Instantiate(openScrollItemPrefab, openScrollContent.transform);
                g.GetComponent<RectTransform>().offsetMin = new Vector2(12f, -SCROLL_ITEM_HEIGHT / 2);
                g.GetComponent<RectTransform>().offsetMax = new Vector2(-12f, SCROLL_ITEM_HEIGHT / 2);
                g.GetComponent<RectTransform>().anchoredPosition =
                    new Vector3(g.GetComponent<RectTransform>().anchoredPosition.x, (SCROLL_ITEM_HEIGHT / 2) * (length - 1 - 2 * index), 0f);
                g.GetComponent<OpenScrollItemWithMark>().Initialize(OpenScrollItemWithMark.Type.Open, s, false, isClearedInTime, isClearedInMove, this);
                index++;
            }

            foreach (string s in metafiles2)
            {
                File.Delete(s.Replace('\\', '/'));
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
                openPathText.text = "Training";
                break;
            case TrainingMapSelect.Basic:
                files = trainingBasicMapFiles;
                length = files.Count + 1;
                isRoot = false;
                openPathText.text = "Training/Basic";
                name = "Basic";
                break;
            case TrainingMapSelect.Fire:
                files = trainingFireMapFiles;
                length = files.Count + 1;
                isRoot = false;
                openPathText.text = "Training/Fire";
                name = "Fire";
                break;
            case TrainingMapSelect.Iron:
                files = trainingIronMapFiles;
                length = files.Count + 1;
                isRoot = false;
                openPathText.text = "Training/Iron";
                name = "Iron";
                break;
            case TrainingMapSelect.Block:
                files = trainingBlockMapFiles;
                length = files.Count + 1;
                isRoot = false;
                openPathText.text = "Training/Block";
                name = "Block";
                break;
            case TrainingMapSelect.Exit:
                files = trainingExitMapFiles;
                length = files.Count + 1;
                isRoot = false;
                openPathText.text = "Training/Exit";
                name = "Exit";
                break;
            case TrainingMapSelect.Shutter:
                files = trainingShutterMapFiles;
                length = files.Count + 1;
                isRoot = false;
                openPathText.text = ".../Shutter";
                name = "Shutter";
                break;
            case TrainingMapSelect.Gate:
                files = trainingGateMapFiles;
                length = files.Count + 1;
                isRoot = false;
                openPathText.text = "Training/Gate";
                name = "Gate";
                break;
        }

        openScrollContent.GetComponent<RectTransform>().sizeDelta =
            new Vector2(openScrollContent.GetComponent<RectTransform>().sizeDelta.x, SCROLL_ITEM_HEIGHT * length);
    
        string[] metafiles;
        SHA256 sha256Hash = SHA256.Create();
        //Debug.Log(openPathText.text);

        string metaPath = Application.persistentDataPath.TrimEnd('/') + "/Meta";
        if(openPathText.text.TrimEnd('/').LastIndexOf('/') >= 1)
        {
            metaPath += "/" + openPathText.text.Substring(0, openPathText.text.TrimEnd('/').LastIndexOf('/'));
        }
        else
        {
            metaPath += "/Training";
        }
        string path = Application.persistentDataPath;
        string[] dirs2;

        if(openPathText.text.TrimEnd('/').LastIndexOf('/') >= 1)
        {
            dirs2 = ("Meta/" + openPathText.text.Substring(0, openPathText.text.TrimEnd('/').LastIndexOf('/'))).Split('/');
        }
        else
        {
            dirs2 = ("Meta/Training").Split('/');
        }
        int len = dirs2.Length;
        for (int i = 0; i < len; i++) {
            string dir = dirs2[i];
            path += '/' + dir;
            if (!Directory.Exists(path)) {
                MetaUtil.CreateDirectory(path);
            }
        }

        metafiles = Directory.GetFiles(metaPath.Replace("...", "Training"), "*.json");
        List<string> metafiles2 = metafiles.ToList<string>();

        if (!isRoot)
        {
            GameObject g = Instantiate(openScrollItemPrefab, openScrollContent.transform);
            g.GetComponent<RectTransform>().offsetMin = new Vector2(12f, -SCROLL_ITEM_HEIGHT / 2);
            g.GetComponent<RectTransform>().offsetMax = new Vector2(-12f, SCROLL_ITEM_HEIGHT / 2);
            g.GetComponent<RectTransform>().anchoredPosition =
                new Vector3(g.GetComponent<RectTransform>().anchoredPosition.x, (SCROLL_ITEM_HEIGHT / 2) * (length - 1 - 2 * index), 0f);

            g.GetComponent<OpenScrollItemWithMark>().Initialize(name, true, this, null, true);
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

                g.GetComponent<OpenScrollItemWithMark>().Initialize(s, true, this, null, false);
                index++;
            }
        }

        else if (files.Count > 0)
        {
            foreach (TextAsset s in files)
            {
                bool isClearedInTime = false;
                bool isClearedInMove = false;

                string s2 = Application.persistentDataPath.TrimEnd('/') + "/Meta/Training\\" + s.name + ".json";
                print(s2);
                if (metafiles2.Contains(s2.Replace('\\', '/')) || metafiles2.Contains(s2))
                {
                    try
                    {
                        JObject json = MetaUtil.ReadMetaFile(s2.Replace('\\', '/'));
                        string metaHash = MetaUtil.GetMapHashFromMetaObject(json);
                        string mapHash = GetHash(sha256Hash, s.text.Trim());
                        if (!metaHash.Equals(mapHash))
                        {
                            isClearedInTime = false;
                            isClearedInMove = false;
                        }

                        JToken cleared = MetaUtil.GetValueFromMetaObject(json, mapHash, MapManager.LimitModeEnum.Time, "hasClearedOnce");
                        if (cleared == null) isClearedInTime = false;
                        else isClearedInTime = (bool)cleared;

                        cleared = MetaUtil.GetValueFromMetaObject(json, mapHash, MapManager.LimitModeEnum.Move, "hasClearedOnce");
                        if (cleared == null) isClearedInMove = false;
                        else isClearedInMove = (bool)cleared;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                    metafiles2.Remove(s2);
                }
                GameObject g = Instantiate(openScrollItemPrefab, openScrollContent.transform);
                g.GetComponent<RectTransform>().offsetMin = new Vector2(12f, -SCROLL_ITEM_HEIGHT / 2);
                g.GetComponent<RectTransform>().offsetMax = new Vector2(-12f, SCROLL_ITEM_HEIGHT / 2);
                g.GetComponent<RectTransform>().anchoredPosition =
                    new Vector3(g.GetComponent<RectTransform>().anchoredPosition.x, (SCROLL_ITEM_HEIGHT / 2) * (length - 1 - 2 * index), 0f);
                g.GetComponent<OpenScrollItemWithMark>().Initialize(s.name, false, isClearedInTime, isClearedInMove, this, s, false);
                index++;
            }
        }
        //남은거 삭제
        openScrollbar.numberOfSteps = Mathf.Max(1, length - 4);
    }

    private void ClearOpenScrollItems()
    {
        selectedOpenScrollItem = null;
        foreach (OpenScrollItemWithMark i in openScrollContent.GetComponentsInChildren<OpenScrollItemWithMark>())
        {
            Destroy(i.gameObject);
        }

        openButton.gameObject.SetActive(true);
        openHighlightedButton.gameObject.SetActive(false);

        openButton.interactable = false;
        openHighlightedButton.interactable = false;
    }

    public void OpenItemSelect(OpenScrollItemWithMark caller)
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

        foreach (OpenScrollItemWithMark i in openScrollContent.GetComponentsInChildren<OpenScrollItemWithMark>())
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
            if (caller.type == OpenScrollItemWithMark.Type.Open)
            {
                string s = selectedOpenScrollItem.path;
                s = "/Meta/" + MetaUtil.ExtentionTxtToJson(s);
                print(s);
                CreateMeta(s);
                Debug.Log(metaPath);
                bool b = CustomOpenFile(selectedOpenScrollItem.path, true);
                openHighlightedButton.gameObject.SetActive(b);
                openButton.gameObject.SetActive(!b);
                openButton.interactable = b;
                openHighlightedButton.interactable = b;
            }
            else if (caller.type == OpenScrollItemWithMark.Type.TrainingOpen)
            {
                string s = selectedOpenScrollItem.path;
                Debug.Log(s);
                s = "/Meta/Training/" + MetaUtil.ExtentionTxtToJson(s + ".json");
                print(s);
                CreateMeta(s);
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
            customSelection = selectedOpenScrollItem.path;
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

                    metaPath = Application.persistentDataPath.TrimEnd('/') + "/Meta/" + MetaUtil.ExtentionTxtToJson(mapPath);
                    print(metaPath);
                    JObject metaObject = MetaUtil.ReadMetaFile(metaPath);

                    /*
                    fileStream = new FileStream(metaPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                    streamReader = new StreamReader(fileStream, Encoding.UTF8);

                    int tryCount;
                    try
                    {
                        bool b = int.TryParse(streamReader.ReadLine().Trim(), out tryCount);
                        bool hasClearedOnce = bool.Parse(streamReader.ReadLine().Trim());

                        if (!hasClearedOnce)
                        {
                            GameManager.mm.tryCount = tryCount;
                        }
                        else
                        {
                            GameManager.mm.tryCount = 0;
                        }

                        streamReader.Close();
                        if (!b)
                        {
                            Debug.LogError("Meta invalid");
                            streamWriter = new StreamWriter(fileStream, Encoding.UTF8);
                            streamWriter.WriteLine("0");
                            streamWriter.WriteLine("False");
                            streamWriter.WriteLine(mapHash);
                            GameManager.mm.tryCount = 0;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Meta invalid: " + e.Message);
                    }

                    try
                    {
                        streamWriter?.Close();
                        fileStream?.Close();
                        streamWriter = null;
                        fileStream = null;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                    */

                    ClearOpenScrollItems();

                    foreach (var t in tooltipUI.GetComponentsInChildren<TooltipBox>())
                    {
                        Destroy(t.gameObject);
                    }
                    statusUI.gameObject.SetActive(false);

                    if (GameManager.mm.LimitMode == MapManager.LimitModeEnum.Time)
                    {
                        JToken hasClearedOnce = MetaUtil.GetValueFromMetaObject(metaObject, mapHash, MapManager.LimitModeEnum.Time, "hasClearedOnce");
                        JToken tryCount = MetaUtil.GetValueFromMetaObject(metaObject, mapHash, MapManager.LimitModeEnum.Time, "tryCount");

                        if (hasClearedOnce == null)
                        {
                            Debug.LogError("Meta invalid: hasClearedOnce");
                            MetaUtil.ReinitializeMetaFile(metaPath, mapHash, MapManager.LimitModeEnum.Time);
                        }
                        else if (tryCount == null)
                        {
                            Debug.LogError("Meta invalid: try count");
                            MetaUtil.ReinitializeMetaFile(metaPath, mapHash, MapManager.LimitModeEnum.Time);
                        }
                        else if ((bool)hasClearedOnce)
                        {
                            GameManager.mm.tryCount = 0;
                        }
                        else
                        {
                            GameManager.mm.tryCount = (int)tryCount;
                        }
                        timerUI.SetActive(true);
                        moveLimitUI.SetActive(false);
                    }
                    else
                    {
                        JToken hasClearedOnce = MetaUtil.GetValueFromMetaObject(metaObject, mapHash, MapManager.LimitModeEnum.Move, "hasClearedOnce");
                        JToken minMoveCount = MetaUtil.GetValueFromMetaObject(metaObject, mapHash, MapManager.LimitModeEnum.Move, "minMoveCount");

                        if (hasClearedOnce == null)
                        {
                            Debug.LogError("Meta invalid: hasClearedOnce");
                            MetaUtil.ReinitializeMetaFile(metaPath, mapHash, MapManager.LimitModeEnum.Move);
                        }
                        else if (minMoveCount == null)
                        {
                            Debug.LogError("Meta invalid: try count");
                            MetaUtil.ReinitializeMetaFile(metaPath, mapHash, MapManager.LimitModeEnum.Move);
                        }
                        else
                        {
                            GameManager.mm.hasClearedOnceInMove = (bool)hasClearedOnce;
                            GameManager.mm.MoveLimit = (int)minMoveCount;
                        }
                        timerUI.SetActive(false);
                        moveLimitUI.SetActive(true);
                    }

                    GameManager.gm.canPlay = true;
                    GameManager.mm.TimeActivate();

                    nextButton.gameObject.SetActive(true);
                    nextButton.interactable = false;
                    quitHighlightedButton.gameObject.SetActive(false);

                    retryButton.gameObject.SetActive(true);
                    retryHighlightedButton.gameObject.SetActive(false);
                    //quitHighlightedButton.interactable = false;
                    retryTimeHighlightedButton.gameObject.SetActive(false);

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

                    metaPath = Application.persistentDataPath.TrimEnd('/') + "/Meta/" + MetaUtil.ExtentionTxtToJson(mapPath);
                    print(metaPath);
                    JObject metaObject = MetaUtil.ReadMetaFile(metaPath);

                    ClearOpenScrollItems();

                    foreach (var t in tooltipUI.GetComponentsInChildren<TooltipBox>())
                    {
                        Destroy(t.gameObject);
                    }
                    statusUI.gameObject.SetActive(false);

                    if (GameManager.mm.LimitMode == MapManager.LimitModeEnum.Time)
                    {
                        JToken hasClearedOnce = MetaUtil.GetValueFromMetaObject(metaObject, mapHash, MapManager.LimitModeEnum.Time, "hasClearedOnce");
                        JToken tryCount = MetaUtil.GetValueFromMetaObject(metaObject, mapHash, MapManager.LimitModeEnum.Time, "tryCount");

                        if (hasClearedOnce == null)
                        {
                            Debug.LogError("Meta invalid: hasClearedOnce");
                            MetaUtil.ReinitializeMetaFile(metaPath, mapHash, MapManager.LimitModeEnum.Time);
                        }
                        else if (tryCount == null)
                        {
                            Debug.LogError("Meta invalid: try count");
                            MetaUtil.ReinitializeMetaFile(metaPath, mapHash, MapManager.LimitModeEnum.Time);
                        }
                        else if ((bool)hasClearedOnce)
                        {
                            GameManager.mm.tryCount = 0;
                        }
                        else
                        {
                            GameManager.mm.tryCount = (int)tryCount;
                        }
                        timerUI.SetActive(true);
                        moveLimitUI.SetActive(false);
                    }
                    else
                    {
                        JToken hasClearedOnce = MetaUtil.GetValueFromMetaObject(metaObject, mapHash, MapManager.LimitModeEnum.Move, "hasClearedOnce");
                        JToken minMoveCount = MetaUtil.GetValueFromMetaObject(metaObject, mapHash, MapManager.LimitModeEnum.Move, "minMoveCount");

                        if (hasClearedOnce == null)
                        {
                            Debug.LogError("Meta invalid: hasClearedOnce");
                            MetaUtil.ReinitializeMetaFile(metaPath, mapHash, MapManager.LimitModeEnum.Move);
                        }
                        else if (minMoveCount == null)
                        {
                            Debug.LogError("Meta invalid: try count");
                            MetaUtil.ReinitializeMetaFile(metaPath, mapHash, MapManager.LimitModeEnum.Move);
                        }
                        else
                        {
                            GameManager.mm.hasClearedOnceInMove = (bool)hasClearedOnce;
                            GameManager.mm.MoveLimit = (int)minMoveCount;
                        }
                        timerUI.SetActive(false);
                        moveLimitUI.SetActive(true);
                    }

                    GameManager.gm.canPlay = true;
                    GameManager.mm.TimeActivate();

                    nextButton.gameObject.SetActive(true);
                    nextButton.interactable = false;
                    quitHighlightedButton.gameObject.SetActive(false);

                    retryButton.gameObject.SetActive(true);
                    retryHighlightedButton.gameObject.SetActive(false);
                    //quitHighlightedButton.interactable = false;
                    retryTimeHighlightedButton.gameObject.SetActive(false);

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
        moveLimitUI.SetActive(false);
        nextButton.interactable = false;
        pauseButton.interactable = true;
        pausePanel.SetActive(false);
        pauseButton.interactable = true;
        CustomOpenPhase(customSelection);
        customPhase = CustomPhase.Open;
        GameManager.gm.CustomChangeBGM(customPhase);
        streamReader?.Close();
        streamWriter?.Close();
        fileStream?.Close();
        streamReader = null;
        streamWriter = null;
        fileStream = null;
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
        moveLimitUI.SetActive(false);
        nextButton.interactable = false;
        pauseButton.interactable = true;
        // TODO
        // messagePanel.SetActive(false);
        TrainingOpenPhase(selection);
        trainingPhase = TrainingPhase.Open;
        GameManager.gm.TrainingChangeBGM(trainingPhase);
        GameManager.gm.canPlay = false;
    }

    public void CreateMeta(string s)
    {
        FileStream fs = null;
        StreamReader sr = null;
        SHA256 sha256Hash = SHA256.Create();
        string mapinfo = "";

        if (SceneManager.GetActiveScene().name.Equals("Custom"))
        {
            mapPath = selectedOpenScrollItem.path;
            try
            {
                if (!File.Exists(mapPath))
                {
                    Debug.LogError("File invalid: there is no file \"" + Path.GetFileNameWithoutExtension(mapPath) + "\"");
                    statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "warning_no_such_file"), 2f);
                    return;
                }
                else if (Path.GetExtension(mapPath) != ".txt")
                {
                    Debug.LogError("File invalid: \"" + Path.GetFileNameWithoutExtension(mapPath) + "\" is not a .txt file");
                    statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "warning_invalid_map_file"), 2f);
                    return;
                }
            }
            catch (Exception)
            {
                Debug.LogError("File invalid: exception while checking a file");
                statusUI.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "warning_exception_while_check"), 3f);
                throw;
            }

            fs = new FileStream(mapPath, FileMode.Open);
            sr = new StreamReader(fs, Encoding.UTF8);
            try
            {
                mapinfo = sr.ReadToEnd().Trim();
                mapHash = GetHash(sha256Hash, mapinfo);
            }
            catch (Exception e)
            {
                Debug.LogError("File invalid: exception while opening a map");
                statusUI?.SetStatusMessageWithFlashing(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "warning_invalid_map"), 1.5f);
                Debug.LogException(e);
                return;
            }
            finally
            {
                GameManager.mm.hasClearedOnceInTime = false;
                GameManager.mm.tryCount = 0;
                GameManager.mm.hasClearedOnceInMove = false;
                GameManager.mm.MoveLimit = int.MaxValue;
                sr.Close();
                fs.Close();
                sr = null;
                fs = null;
            }
        }
        else if (SceneManager.GetActiveScene().name.Equals("Training"))
        {
            mapPath = "Training/" + selectedOpenScrollItem.path.TrimStart('/') + ".txt";
            mapinfo = selectedOpenScrollItem.textAsset.text.Trim();
            mapHash = GetHash(sha256Hash, mapinfo);
        }

        metaPath = Application.persistentDataPath.TrimEnd('/') + '/' + MetaUtil.ExtentionTxtToJson(s);
        print(metaPath);

        if (!File.Exists(metaPath))
        {
            //StreamWriter sw = null;
            string path = Application.persistentDataPath;
            string[] dirs = s.Split('/');
            int len = dirs.Length - 1;
            for (int i = 0; i < len; i++) {
                string dir = dirs[i];
                path += '/' + dir;
                if (!Directory.Exists(path)) {
                    MetaUtil.CreateDirectory(path);
                }
            }
            MetaUtil.CreateNewMetaFile(metaPath, mapHash, FileMode.OpenOrCreate);
        }
        else
        {
            try
            {
                JObject json = MetaUtil.ReadMetaFile(metaPath);
                string metaHash = MetaUtil.GetMapHashFromMetaObject(json);
                JToken tryCount = MetaUtil.GetValueFromMetaObject(json, mapHash, MapManager.LimitModeEnum.Time, "tryCount");
                JToken hasClearedOnceInTime = MetaUtil.GetValueFromMetaObject(json, mapHash, MapManager.LimitModeEnum.Time, "hasClearedOnce");
                JToken minMoveCount = MetaUtil.GetValueFromMetaObject(json, mapHash, MapManager.LimitModeEnum.Move, "minMoveCount");
                JToken hasClearedOnceInMove = MetaUtil.GetValueFromMetaObject(json, mapHash, MapManager.LimitModeEnum.Move, "hasClearedOnce");
                if (VerifyHash(sha256Hash, mapinfo.Trim(), metaHash))
                {
                    bool b1 = true;
                    bool b2 = true;
                    if (tryCount == null || hasClearedOnceInTime == null)
                    {
                        MetaUtil.ReinitializeMetaFile(metaPath, mapHash, MapManager.LimitModeEnum.Time);
                        b1 = false;
                    }

                    if (minMoveCount == null || hasClearedOnceInMove == null)
                    {
                        MetaUtil.ReinitializeMetaFile(metaPath, mapHash, MapManager.LimitModeEnum.Move);
                        b2 = false;
                    }
                    
                    if (b1)
                    {
                        GameManager.mm.hasClearedOnceInTime = (bool)hasClearedOnceInTime;
                        GameManager.mm.tryCount = (int)tryCount;
                    }
                    if (b2)
                    {
                        GameManager.mm.hasClearedOnceInMove = (bool)hasClearedOnceInMove;
                        GameManager.mm.MoveLimit = (int)minMoveCount;
                    }
                }
                else
                {
                    // 다를 경우
                    MetaUtil.CreateNewMetaFile(metaPath, mapHash, FileMode.Create);
                }
            }
            catch (Exception)
            {
                sr?.Close();
                fs?.Close();
                MetaUtil.CreateNewMetaFile(metaPath, mapHash, FileMode.Create);
                //Debug.LogError(e.Message);
            }
            
        }

    }

    public string GetHash(HashAlgorithm hashAlgorithm, string text)
    {
        byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(text));

        var sBuilder = new StringBuilder();

        for (int i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }

        return sBuilder.ToString();
    }

    public bool VerifyHash(HashAlgorithm hashAlgorithm, string input, string hash)
    {
        // Hash the input.
        var hashOfInput = GetHash(hashAlgorithm, input);

        // Create a StringComparer an compare the hashes.
        StringComparer comparer = StringComparer.OrdinalIgnoreCase;

        return comparer.Compare(hashOfInput, hash) == 0;
    }

    public void ChangeLimitMode(MapManager.LimitModeEnum limitMode)
    {
        if (limitMode == MapManager.LimitModeEnum.Time)
        {
            foreach (GameObject g in GameManager.pm.minMoveCountUis)
            {
                g.SetActive(false);
            }
            foreach (GameObject g in GameManager.pm.tryCountUis)
            {
                g.SetActive(true);
            }
        }
        else
        {
            foreach (GameObject g in GameManager.pm.tryCountUis)
            {
                g.SetActive(false);
            }
            foreach (GameObject g in GameManager.pm.minMoveCountUis)
            {
                g.SetActive(true);
            }
        }
    }
}
