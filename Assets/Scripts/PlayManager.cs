using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayManager : MonoBehaviour
{
    public enum Mode { Tutorial = 0, Custom = 1, Survival = 2,
        AdvEasy = 11, AdvNormal = 12, AdvHard = 13, AdvInsane = 14 }

    public Button pauseButton;                   // quitHighlightedButton이 활성화될 때 비활성화
    public Button quitHighlightedButton;        // 모든 맵을 탈출하거나 라이프가 0이 되어 게임이 종료될 때 활성화
    public Button nextButton;                   // 탈출 또는 시간 초과 시 활성화 (튜토리얼에서는 탈출 시에만 활성화), quitHighlightedButton이 활성화될 때 비활성화
    public Button retryButton;                  // Continued일 때 활성화, 사망 또는 탈출 또는 시간 초과 시 비활성화
    public Button retryHighlightedButton;       // Burned 또는 Squashed일 때 활성화
    public Button retryTimeButton;              // 시간 초과 시 활성화 (튜토리얼에서는 탈출 시 활성화)
    public Button retryTimeHighlightedButton;   // (튜토리얼에서만 시간 초과 시 활성화)
    public MessageUI messageUI;
    public GameObject messagePanel;
    public ResultUI resultUI;

    public TutorialGuide tutorialGuide;//추가

    private Mode playMode;

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

    private List<TextAsset> _mapFiles;

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
        messageUI.gameObject.SetActive(false);
        messagePanel.SetActive(false);
        resultUI.gameObject.SetActive(false);
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
            List<TextAsset> tempList = _mapFiles.OrderBy(_ => Random.value).ToList();
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
        messagePanel.SetActive(true);
        GameManager.mm.TimePause();
        messageUI.Initialize("<b>Paused</b>\n\nDo you want to quit game?",
            () => resultUI.Initialize(playMode),
            () =>
            {
                GameManager.mm.TimeResume();
                messagePanel.SetActive(false);
                pauseButton.interactable = true;
            }) ;
    }

    public void Quit()
    {
        if (SceneManager.GetActiveScene().name.Equals("Tutorial"))
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

    public void PlayAfterGravity(MapManager.Flag flag)
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

    public void PlayButtonSFX()
    {
        GameManager.gm.PlayButtonSFX();
    }
}
