using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayManager : MonoBehaviour
{
    public enum Mode { Tutorial = 0, Custom = 1, Survival = 2,
        AdvEasy = 11, AdvNormal = 12, AdvHard = 13, AdvInsane = 14 }

    public Button quitButton;                   // quitHighlightedButton이 활성화될 때 비활성화
    public Button quitHighlightedButton;        // 모든 맵을 탈출하거나 라이프가 0이 되어 게임이 종료될 때 활성화
    public Button nextButton;                   // 탈출 또는 시간 초과 시 활성화 (튜토리얼에서는 탈출 시에만 활성화), quitHighlightedButton이 활성화될 때 비활성화
    public Button retryButton;                  // Continued일 때 활성화, 사망 또는 탈출 또는 시간 초과 시 비활성화
    public Button retryHighlightedButton;       // Burned 또는 Squashed일 때 활성화
    public Button retryTimeButton;              // 시간 초과 시 활성화 (튜토리얼에서는 탈출 시 활성화)
    public Button retryTimeHighlightedButton;   // (튜토리얼에서만 시간 초과 시 활성화)

    private Mode playMode;

    [Header("Tutorial")]
    [SerializeField]
    private List<TextAsset> tutorialMapFiles = new List<TextAsset>();

    [Header("Easy")]
    [SerializeField]
    private List<TextAsset> adventureEasyMapFiles = new List<TextAsset>();
    [SerializeField]
    private int adventureEasyPlayLength = int.MaxValue;
    [SerializeField]
    private int adventureEasyLife = 5;

    [Header("Normal")]
    [SerializeField]
    private List<TextAsset> adventureNormalMapFiles = new List<TextAsset>();
    [SerializeField]
    private int adventureNormalPlayLength = int.MaxValue;
    [SerializeField]
    private int adventureNormalLife = 5;

    [Header("Hard")]
    [SerializeField]
    private List<TextAsset> adventureHardMapFiles = new List<TextAsset>();
    [SerializeField]
    private int adventureHardPlayLength = int.MaxValue;
    [SerializeField]
    private int adventureHardLife = 5;

    [Header("Insane")]
    [SerializeField]
    private List<TextAsset> adventureInsaneMapFiles = new List<TextAsset>();
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

    public bool IsReady
    {
        get;
        private set;
    } = false;

    public void Initialize(Mode mode, bool isRandomOrder = false, int maxPlayLength = int.MaxValue, int initialLife = 5)
    {
        IsReady = false;
        playMode = mode;
        switch (playMode)
        {
            case Mode.Tutorial:
                _mapFiles = tutorialMapFiles;
                IsRandomOrder = false;
                PlayLength = _mapFiles.Count;
                Life = int.MaxValue;
                break;
            case Mode.AdvEasy:
                _mapFiles = adventureEasyMapFiles;
                IsRandomOrder = isRandomOrder;
                PlayLength = Mathf.Clamp(adventureEasyPlayLength, 1, _mapFiles.Count);
                Life = adventureEasyLife;
                break;
            case Mode.AdvNormal:
                _mapFiles = adventureNormalMapFiles;
                IsRandomOrder = isRandomOrder;
                PlayLength = Mathf.Clamp(adventureNormalPlayLength, 1, _mapFiles.Count);
                Life = adventureNormalLife;
                break;
            case Mode.AdvHard:
                _mapFiles = adventureHardMapFiles;
                IsRandomOrder = isRandomOrder;
                PlayLength = Mathf.Clamp(adventureHardPlayLength, 1, _mapFiles.Count);
                Life = adventureHardLife;
                break;
            case Mode.AdvInsane:
                _mapFiles = adventureInsaneMapFiles;
                IsRandomOrder = isRandomOrder;
                PlayLength = Mathf.Clamp(adventureInsanePlayLength, 1, _mapFiles.Count);
                Life = adventureInsaneLife;
                break;
            default:
                IsRandomOrder = isRandomOrder;
                PlayLength = Mathf.Clamp(maxPlayLength, 1, _mapFiles.Count);
                Life = Mathf.Max(initialLife, 1);
                // TODO
                return;
        }

        if (_mapFiles == null || _mapFiles.Count < 1 || maxPlayLength < 1) return;

        Debug.Log("Remaining life: " + Life);

        if (IsRandomOrder)
        {
            List<TextAsset> tempList = _mapFiles.OrderBy(_ => Random.value).ToList();
            _mapFiles = tempList;
        }
        IsReady = true;
    }

    public void Quit()
    {
        // TODO 시간 멈추고 맵 가리고 확인 메시지 띄우기
        GameManager.gm.LoadMain();
    }

    public void Ending()
    {
        // TODO 결과창 보여주고 확인 메시지 띄우기
        GameManager.gm.LoadMain();
    }

    public void TutorialNext()
    {
        GameManager.gm.TutorialNext();
        if (GameManager.gm.HasClearedAll)
        {
            Ending();
        }
    }

    public void PlayNext()
    {
        GameManager.gm.PlayNext();
        if (GameManager.gm.HasClearedAll)
        {
            Ending();
        }
    }

    public void TutorialAfterGravity(MapManager.Flag flag)
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

                quitButton.interactable = true;
                nextButton.interactable = false;
                break;
            case MapManager.Flag.Escaped:
                retryButton.gameObject.SetActive(false);
                retryHighlightedButton.gameObject.SetActive(false);
                retryTimeButton.gameObject.SetActive(true);
                retryTimeHighlightedButton.gameObject.SetActive(false);
                if (!GameManager.gm.HasClearedAll)
                {
                    // 다음 맵이 존재할 때
                    nextButton.gameObject.SetActive(true);
                    quitHighlightedButton.gameObject.SetActive(false);

                    quitButton.interactable = true;
                    nextButton.interactable = true;
                }
                else
                {
                    // 모든 맵을 탈출했을 때
                    nextButton.gameObject.SetActive(false);
                    quitHighlightedButton.gameObject.SetActive(true);

                    quitButton.interactable = false;
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

                quitButton.interactable = true;
                nextButton.interactable = false;
                break;
            case MapManager.Flag.TimeOver:
                retryButton.gameObject.SetActive(false);
                retryHighlightedButton.gameObject.SetActive(false);
                retryTimeButton.gameObject.SetActive(false);
                retryTimeHighlightedButton.gameObject.SetActive(true);
                nextButton.gameObject.SetActive(true);
                quitHighlightedButton.gameObject.SetActive(false);

                quitButton.interactable = true;
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

                quitButton.interactable = true;
                nextButton.interactable = false;
                break;
            case MapManager.Flag.Escaped:
                retryButton.gameObject.SetActive(false);
                retryHighlightedButton.gameObject.SetActive(false);
                retryTimeButton.gameObject.SetActive(false);
                retryTimeHighlightedButton.gameObject.SetActive(false);
                if (!GameManager.gm.HasClearedAll)
                {
                    // 다음 맵이 존재할 때
                    nextButton.gameObject.SetActive(true);
                    quitHighlightedButton.gameObject.SetActive(false);

                    quitButton.interactable = true;
                    nextButton.interactable = true;
                }
                else
                {
                    // 모든 맵을 탈출했을 때
                    nextButton.gameObject.SetActive(false);
                    quitHighlightedButton.gameObject.SetActive(true);

                    quitButton.interactable = false;
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

                quitButton.interactable = true;
                nextButton.interactable = false;
                break;
            case MapManager.Flag.TimeOver:
                Life--;
                Debug.Log("Remaining life: " + Life);
                retryButton.gameObject.SetActive(false);
                retryHighlightedButton.gameObject.SetActive(false);
                retryTimeHighlightedButton.gameObject.SetActive(false);
                if (Life > 0)
                {
                    // 라이프가 남아있을 때
                    retryTimeButton.gameObject.SetActive(true);
                    nextButton.gameObject.SetActive(true);
                    quitHighlightedButton.gameObject.SetActive(false);

                    quitButton.interactable = true;
                    nextButton.interactable = true;
                }
                else
                {
                    // 라이프가 0일 때
                    retryTimeButton.gameObject.SetActive(false);
                    nextButton.gameObject.SetActive(false);
                    quitHighlightedButton.gameObject.SetActive(true);

                    quitButton.interactable = false;
                }
                break;
        }
    }
}
