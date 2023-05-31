using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

public class GameManager : MonoBehaviour
{

    public static GameManager gm;
    public static MapManager mm = null;
    public static PlayManager pm = null;
    public static EditorManager em = null;

    public enum GravityDirection { Up, Down, Left, Right }

    [HideInInspector]
    public bool canPlay = true;

    private enum AdventureLevel { NULL = 0, Easy = 1, Normal = 2, Hard = 3, Insane = 4 }

    [SerializeField]
    private AdventureLevel adventureLevel;
    private int playingMapIndex = 0;

    public int PlayingMapIndex{
        get{return playingMapIndex;}
    }

    public GameObject floorStarPrefab;

    public AudioSource bgmAudioSource;
    public List<AudioClip> bgms;
    public float bgmVolume = 0.8f;
    private List<float> bgmVolumeForEach = new List<float>() { 1f, 0.7f, 1f };

    public AudioSource sfxAudioSource;
    public List<AudioClip> ballSfxs;
    public List<AudioClip> ironSfxs;
    public AudioClip wallSfx;
    public AudioClip shutterSfx;
    public AudioClip squashedSfx;
    public AudioClip burnedSfx;
    public AudioClip escapedSfx;
    public AudioClip timeoutSfx;
    public AudioClip retrySfx;
    public List<AudioClip> buttonSfxs;
    public AudioClip removeSfx;
    public AudioClip fallSfx;
    public List<AudioClip> starSfxs;
    public float sfxVolume = 0.8f;

    public enum Language { English = 0, Korean = 1 }

    private void Awake()
    {
        if (gm != null && gm != this)
        {
            Destroy(gameObject);
            return;
        }
        gm = this;
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        bgmAudioSource.volume = Mathf.Clamp01(bgmVolume);
        sfxAudioSource.volume = 1f;
        Initialize();   

#if UNITY_ANDROID && !UNITY_EDITOR
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        }
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
        }
#endif
    }

    // Update is called once per frame
    void Update()
    {
        if (pm != null && pm.IsReady)
        {
            bgmAudioSource.volume = Mathf.Clamp01(pm.pauseUI.bgmVolume);
            sfxAudioSource.volume = 1f;
        }

        for (int i = 0; i < 3; i++)
        {
            if (bgmAudioSource.clip == bgms[i])
            {
                bgmAudioSource.volume = Mathf.Clamp01(bgmVolume * bgmVolumeForEach[i]);
                break;
            }
        }

        // 입력 담당
        if (canPlay)
        {
            if (mm is null || !mm.IsReady) return; // mm : 맵의 미리보기가 떴을 때 또는 플레이 도중에만 값이 할당되어 있는 듯??

            if ((Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.S)) && mm.gravityDownButton.interactable)
            {
                mm.ManipulateGravityDown();
            }
            else if ((Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.W)) && mm.gravityUpButton.interactable)
            {
                mm.ManipulateGravityUp();
            }
            else if ((Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.A)) && mm.gravityLeftButton.interactable)
            {
                mm.ManipulateGravityLeft();
            }
            else if ((Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.D)) && mm.gravityRightButton.interactable)
            {
                mm.ManipulateGravityRight();
            }
            else if (Input.GetKeyUp(KeyCode.Space) && ((mm.gravityRetryButton.gameObject.activeInHierarchy
                && mm.gravityRetryButton.interactable) || (mm.gravityRetryHighlightedButton != null &&
                mm.gravityRetryHighlightedButton.gameObject.activeInHierarchy && mm.gravityRetryHighlightedButton.interactable)))
            {
                mm.Retry();
                if (SceneManager.GetActiveScene().name.Equals("Tutorial") && pm != null && pm.tutorialGuide != null)
                {
                    pm.tutorialGuide.RetryButtonDown();
                }
            }
            else if (Input.GetKeyUp(KeyCode.Return) && pm != null && pm.IsReady)
            {
                if (pm.nextButton.gameObject.activeInHierarchy && pm.nextButton.interactable)
                {
                    if (SceneManager.GetActiveScene().name.Equals("Tutorial"))
                    {
                        pm.TutorialNext();
                        PlayButtonSFX();
                    }
                    else
                    {
                        pm.PlayNext();
                        PlayButtonSFX();
                    }
                }
                else if (pm.resultUI.gameObject.activeInHierarchy)
                {
                    pm.Quit();
                    PlayButtonSFX();
                }
                else if (pm.quitHighlightedButton.gameObject.activeInHierarchy && pm.quitHighlightedButton.interactable)
                {
                    pm.Ending();
                    PlayButtonSFX();
                }
                else if (pm.pauseUI.gameObject.activeInHierarchy && pm.pauseUI.pauseExitButton.interactable)
                {
                    pm.pauseUI.pauseExitButton.onClick.Invoke();
                }
            }
            else if (Input.GetKeyUp(KeyCode.Escape) && pm != null && pm.IsReady)
            {
                if (pm.pauseButton.gameObject.activeInHierarchy && pm.pauseButton.interactable)
                {
                    pm.Pause();
                    PlayButtonSFX();
                }
                else if (pm.pauseUI.gameObject.activeInHierarchy && pm.pauseUI.pauseReturnButton.interactable)
                {
                    pm.pauseUI.pauseReturnButton.onClick.Invoke();
                }
            }
        }
        else 
        {
            if (Input.GetKeyUp(KeyCode.Return)) 
            {
                if (pm != null && !pm.IsReady) //Custom, Training: pm의 객체에 속한 버튼을 누름
                {
                    if (SceneManager.GetActiveScene().name.Equals("Custom"))
                    {
                        if (pm.customPhase == PlayManager.CustomPhase.Open) //Custom 모드에서 인게임이 아닐 때.
                        {
                            if (pm.openButton.gameObject.activeInHierarchy && pm.openButton.interactable)
                            {
                                pm.openButton.onClick.Invoke();
                            }
                            else if (pm.openHighlightedButton.gameObject.activeInHierarchy && pm.openHighlightedButton.interactable)
                            {
                                if (mm is null || !mm.IsReady) return;
                                pm.openHighlightedButton.onClick.Invoke();
                            }
                            else
                            {
                                Debug.Log("Exception: canPlay == false, but customPhase != Open");
                                Debug.Log(pm.customPhase);
                            }
                        }
                    }
                    else if (SceneManager.GetActiveScene().name.Equals("Training"))
                    {
                        if (pm.trainingPhase == PlayManager.TrainingPhase.Open)
                        {
                            if (pm.openButton.gameObject.activeInHierarchy && pm.openButton.interactable)
                            {
                                pm.openButton.onClick.Invoke(); //open folder
                            }
                            else if (pm.openHighlightedButton.gameObject.activeInHierarchy && pm.openHighlightedButton.interactable)
                            {
                                if (mm is null || !mm.IsReady) return;
                                pm.openHighlightedButton.onClick.Invoke(); //open map
                            }
                            else
                            {
                                Debug.Log("Exception: canPlay == false, but trainingPhase != Open");
                                Debug.Log(pm.trainingPhase);
                            }
                        }
                    }
                }
                else if (SceneManager.GetActiveScene().name.Equals("Editor") && em != null) // Editor: em의 객체에 속한 버튼을 누름
                {
                    switch (em.editPhase)
                    {
                        case EditorManager.EditPhase.Open:
                            if (em.editorOpenButton5.gameObject.activeInHierarchy && em.editorOpenButton5.interactable)
                            {
                                em.editorOpenButton5.onClick.Invoke();
                            }
                            else if (em.editorOpenHighlightedButton5.gameObject.activeInHierarchy && em.editorOpenHighlightedButton5.interactable)
                            {
                                em.editorOpenHighlightedButton5.onClick.Invoke();
                            }
                            break;
                        case EditorManager.EditPhase.Save:
                            if (em.editorOpenButton6.gameObject.activeInHierarchy && em.editorOpenButton6.interactable)
                            {
                                em.editorOpenButton6.onClick.Invoke();
                            }
                            else if (em.editorSaveButton6.gameObject.activeInHierarchy && em.editorSaveButton6.interactable)
                            {
                                em.editorSaveButton6.onClick.Invoke();
                            }
                            break;
                    }
                }
            }
        }
    }

    void Initialize()
    {
        playingMapIndex = -1;

        // TODO: 씬 바뀔 때마다 적절한 레벨 선택하고 MapManager 찾아서 맵 로드해야 함
        if (SceneManager.GetActiveScene().name.Equals("Main"))
        {
            bool isTutorialDone = true;
            try
            {
                if (!File.Exists(Application.persistentDataPath + "/TutorialDone.txt"))
                {
                    LoadFirst();
                    isTutorialDone = false;

                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }

            if (isTutorialDone)
            {
                if (bgmAudioSource.clip != bgms[0])
                {
                    bgmAudioSource.Stop();
                    bgmAudioSource.clip = bgms[0];
                    bgmAudioSource.Play();
                }
                StartCoroutine(InitializeMain());
            }
        }
        else if (SceneManager.GetActiveScene().name.Equals("Editor"))
        {
            if (bgmAudioSource.clip != bgms[2])
            {
                bgmAudioSource.Stop();
                bgmAudioSource.clip = bgms[2];
                bgmAudioSource.Play();
            }
            StartCoroutine(InitializeEditor());
        }
        else if (SceneManager.GetActiveScene().name.Equals("Mode"))
        {
            if (bgmAudioSource.clip != bgms[0])
            {
                bgmAudioSource.Stop();
                bgmAudioSource.clip = bgms[0];
                bgmAudioSource.Play();
            }
            StartCoroutine(InitializeMode());
        }
        else if (SceneManager.GetActiveScene().name.Equals("AdventureLevel"))
        {
            if (bgmAudioSource.clip != bgms[0])
            {
                bgmAudioSource.Stop();
                bgmAudioSource.clip = bgms[0];
                bgmAudioSource.Play();
            }
            StartCoroutine(InitializeAdventureLevel());
        }
        else if (SceneManager.GetActiveScene().name.Equals("Tutorial"))
        {
            if (bgmAudioSource.clip != bgms[1])
            {
                bgmAudioSource.Stop();
                bgmAudioSource.clip = bgms[1];
                bgmAudioSource.Play();
            }
            StartCoroutine(InitializeTutorial());
        }
        else if (SceneManager.GetActiveScene().name.Equals("Adventure"))
        {
            if (bgmAudioSource.clip != bgms[1])
            {
                bgmAudioSource.Stop();
                bgmAudioSource.clip = bgms[1];
                bgmAudioSource.Play();
            }
            StartCoroutine(InitializeAdventure());
        }
        else if (SceneManager.GetActiveScene().name.Equals("Custom"))
        {
            if (bgmAudioSource.clip != bgms[0])
            {
                bgmAudioSource.Stop();
                bgmAudioSource.clip = bgms[0];
                bgmAudioSource.Play();
            }
            StartCoroutine(InitializeCustom());
        }
        else if (SceneManager.GetActiveScene().name.Equals("First"))
        {
            if (bgmAudioSource.clip != bgms[0])
            {
                bgmAudioSource.Stop();
                bgmAudioSource.clip = bgms[0];
                bgmAudioSource.Play();
            }
        }
        else if (SceneManager.GetActiveScene().name.Equals("Setting"))
        {
            if (bgmAudioSource.clip != bgms[0])
            {
                bgmAudioSource.Stop();
                bgmAudioSource.clip = bgms[0];
                bgmAudioSource.Play();
            }
            canPlay = false;
        }

        else if (SceneManager.GetActiveScene().name.Equals("Training"))
        {
            if (bgmAudioSource.clip != bgms[0])
            {
                bgmAudioSource.Stop();
                bgmAudioSource.clip = bgms[0];
                bgmAudioSource.Play();
            }
            StartCoroutine(InitializeTraining());
        }
    }
    
    public void EditorChangeBGM(EditorManager.EditPhase editPhase)
    {
        if (editPhase != EditorManager.EditPhase.Test && bgmAudioSource.clip != bgms[2])
        {
            bgmAudioSource.Stop();
            bgmAudioSource.clip = bgms[2];
            bgmAudioSource.Play();
        }
        if (editPhase == EditorManager.EditPhase.Test && bgmAudioSource.clip != bgms[1])
        {
            bgmAudioSource.Stop();
            bgmAudioSource.clip = bgms[1];
            bgmAudioSource.Play();
        }
    }

    public void TrainingChangeBGM(PlayManager.TrainingPhase trainingPhase)
    {
        if (trainingPhase == PlayManager.TrainingPhase.Open && bgmAudioSource.clip != bgms[0])
        {
            bgmAudioSource.Stop();
            bgmAudioSource.clip = bgms[0];
            bgmAudioSource.Play();
        }
        if (trainingPhase == PlayManager.TrainingPhase.Ingame && bgmAudioSource.clip != bgms[1])
        {
            bgmAudioSource.Stop();
            bgmAudioSource.clip = bgms[1];
            bgmAudioSource.Play();
        }
    }

    public void CustomChangeBGM(PlayManager.CustomPhase customPhase)
    {
        if (customPhase == PlayManager.CustomPhase.Open && bgmAudioSource.clip != bgms[0])
            {
            bgmAudioSource.Stop();
            bgmAudioSource.clip = bgms[0];
            bgmAudioSource.Play();
            }
        if (customPhase == PlayManager.CustomPhase.Ingame && bgmAudioSource.clip != bgms[1])
        {
            bgmAudioSource.Stop();
            bgmAudioSource.clip = bgms[1];
            bgmAudioSource.Play();
        }
    }

    public void PlayBallSFX()
    {
        int r = UnityEngine.Random.Range(0, ballSfxs.Count);
        sfxAudioSource.PlayOneShot(ballSfxs[r], Mathf.Clamp01(sfxVolume));
    }

    public void PlayIronSFX(int moveDistance, float volumeScale = 1f)
    {
        if (moveDistance < 1 || moveDistance > 8) return;
        sfxAudioSource.PlayOneShot(ironSfxs[moveDistance - 1], Mathf.Clamp01(volumeScale * sfxVolume));
    }

    public void PlayWallSFX()
    {
        sfxAudioSource.PlayOneShot(wallSfx, Mathf.Clamp01(sfxVolume));
    }

    public void PlayShutterSFX()
    {
        sfxAudioSource.PlayOneShot(shutterSfx, Mathf.Clamp01(sfxVolume));
    }

    public void PlaySquashedSFX()
    {
        sfxAudioSource.PlayOneShot(squashedSfx, Mathf.Clamp01(sfxVolume));
    }

    public void PlayBurnedSFX()
    {
        sfxAudioSource.PlayOneShot(burnedSfx, Mathf.Clamp01(sfxVolume));
    }

    public void PlayEscapedSFX()
    {
        sfxAudioSource.PlayOneShot(escapedSfx, Mathf.Clamp01(sfxVolume));
    }

    public void PlayTimeoutSFX()
    {
        sfxAudioSource.PlayOneShot(timeoutSfx, Mathf.Clamp01(sfxVolume));
    }

    public void PlayRetrySFX()
    {
        sfxAudioSource.PlayOneShot(retrySfx, Mathf.Clamp01(sfxVolume));
    }

    public void PlayButtonSFX()
    {
        int r = UnityEngine.Random.Range(0, buttonSfxs.Count);
        sfxAudioSource.PlayOneShot(buttonSfxs[r], Mathf.Clamp01(sfxVolume));
    }

    public void PlayRemoveSFX()
    {
        sfxAudioSource.PlayOneShot(removeSfx, Mathf.Clamp01(sfxVolume));
    }

    public void PlayFallSFX(float volume)
    {
        sfxAudioSource.PlayOneShot(fallSfx, Mathf.Clamp01(volume * sfxVolume));
    }

    public void PlayStarSFX(int num)
    {
        sfxAudioSource.PlayOneShot(starSfxs[num], Mathf.Clamp01(sfxVolume));
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void LoadEditor()
    {
        StartCoroutine(SceneLoading("Editor"));
    }

    public void LoadMain()
    {
        StartCoroutine(SceneLoading("Main"));
    }

    public void LoadMode()
    {
        StartCoroutine(SceneLoading("Mode"));
    }

    public void LoadAdventureLevel()
    {
        StartCoroutine(SceneLoading("AdventureLevel"));
    }

    public void LoadTutorial()
    {
        StartCoroutine(SceneLoading("Tutorial"));
    }

    public void LoadCustom()
    {
        StartCoroutine(SceneLoading("Custom"));
    }

    public void LoadTraining()
    {
        StartCoroutine(SceneLoading("Training"));

    }
    public void LoadAdventureEasy()
    {
        adventureLevel = AdventureLevel.Easy;
        StartCoroutine(SceneLoading("Adventure"));
    }

    public void LoadAdventureNormal()
    {
        adventureLevel = AdventureLevel.Normal;
        StartCoroutine(SceneLoading("Adventure"));
    }

    public void LoadAdventureHard()
    {
        adventureLevel = AdventureLevel.Hard;
        StartCoroutine(SceneLoading("Adventure"));
    }

    public void LoadAdventureInsane()
    {
        adventureLevel = AdventureLevel.Insane;
        StartCoroutine(SceneLoading("Adventure"));
    }

    public void LoadFirst()
    {
        StartCoroutine(SceneLoading("First"));
    }

    public void LoadSetting()
    {
        StartCoroutine(SceneLoading("Setting"));
    }
    public void LoadCredit()
    {
        StartCoroutine(SceneLoading("Credit"));
    }

    IEnumerator SceneLoading(string sceneName)
    {
        if (mm != null)
        {
            mm.loadingPanel.SetActive(true);
        }
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        while (!operation.isDone)
        {
            yield return null;
        }
        Initialize();
    }

    IEnumerator InitializeMain()
    {
        while (mm == null)
        {
            mm = GameObject.FindGameObjectWithTag("MapManager").GetComponent<MapManager>();
            if (mm == null)
            {
                mm = GameObject.Find("MapManager").GetComponent<MapManager>();
            }
            yield return null;
        }
        List<WallInfo> walls = new List<WallInfo>();

        walls.Add(new WallInfo(WallInfo.Type.Vertical, 5, 3));
        walls.Add(new WallInfo(WallInfo.Type.Vertical, 6, 3));
        walls.Add(new WallInfo(WallInfo.Type.Vertical, 3, 2));
        walls.Add(new WallInfo(WallInfo.Type.Vertical, 4, 2));
        walls.Add(new WallInfo(WallInfo.Type.Vertical, 4, 1));
        walls.Add(new WallInfo(WallInfo.Type.Vertical, 5, 1));
        walls.Add(new WallInfo(WallInfo.Type.Vertical, 6, 1));
        walls.Add(new WallInfo(WallInfo.Type.Vertical, 1, 5));
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
        //walls.Add(new WallInfo(WallInfo.Type.Horizontal, 1, 4));
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
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 7, 2));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 5, 1));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 6, 1));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 7, 1));
        walls.Add(new WallInfo(WallInfo.Type.ExitVertical, 7, 4));

        List<ObjectInfo> objects = new List<ObjectInfo>();

        mm.afterGravity = MainAfterGravity;

        mm.Initialize(7, 7, walls, objects, "d", float.PositiveInfinity);
        mm.TimeActivate();
        canPlay = true;
    }

    IEnumerator InitializeEditor()
    {
        while (mm == null)
        {
            mm = GameObject.FindGameObjectWithTag("MapManager").GetComponent<MapManager>();
            if (mm == null)
            {
                mm = GameObject.Find("MapManager").GetComponent<MapManager>();
            }
            yield return null;
        }
        canPlay = false;

        while (em == null) {
            em = GameObject.FindGameObjectWithTag("EditorManager").GetComponent<EditorManager>();
            if (em == null) {
                em = GameObject.Find("EditorManager").GetComponent<EditorManager>();
            }
            yield return null;
        }
    }

    IEnumerator InitializeCustom()
    {
        while (mm == null)
        {
            mm = GameObject.FindGameObjectWithTag("MapManager").GetComponent<MapManager>();
            if (mm == null)
            {
                mm = GameObject.Find("MapManager").GetComponent<MapManager>();
            }
            yield return null;
        }
        canPlay = false;

        while (pm == null)
        {
            pm = GameObject.FindGameObjectWithTag("PlayManager").GetComponent<PlayManager>();
            if (pm != null) break;
            else
            {
                pm = GameObject.Find("PlayManager").GetComponent<PlayManager>();
            }
            yield return null;
        }

        pm.Initialize(PlayManager.Mode.Custom);

        mm.afterGravity = pm.CustomAfterGravity;
    }

    IEnumerator InitializeTraining()
    {
        while (mm == null)
        {
            mm = GameObject.FindGameObjectWithTag("MapManager").GetComponent<MapManager>();
            if (mm == null)
            {
                mm = GameObject.Find("MapManager").GetComponent<MapManager>();
            }
            yield return null;
        }
        canPlay = false;

        while (pm == null)
        {
            pm = GameObject.FindGameObjectWithTag("PlayManager").GetComponent<PlayManager>();
            if (pm != null) break;
            else
            {
                pm = GameObject.Find("PlayManager").GetComponent<PlayManager>();
            }
            yield return null;
        }

        pm.Initialize(PlayManager.Mode.Training);

        mm.afterGravity = pm.TrainingAfterGravity;
    }

    IEnumerator InitializeMode()
    {
        while (mm == null)
        {
            mm = GameObject.FindGameObjectWithTag("MapManager").GetComponent<MapManager>();
            if (mm == null)
            {
                mm = GameObject.Find("MapManager").GetComponent<MapManager>();
            }
            yield return null;
        }
        List<WallInfo> walls = new List<WallInfo>();

        walls.Add(new WallInfo(WallInfo.Type.Vertical, 1, 1));
        walls.Add(new WallInfo(WallInfo.Type.Vertical, 1, 4));
        walls.Add(new WallInfo(WallInfo.Type.Vertical, 1, 5));
        walls.Add(new WallInfo(WallInfo.Type.Vertical, 1, 7));
        walls.Add(new WallInfo(WallInfo.Type.Vertical, 2, 2));
        walls.Add(new WallInfo(WallInfo.Type.Vertical, 2, 5));
        walls.Add(new WallInfo(WallInfo.Type.Vertical, 3, 3));
        // walls.Add(new WallInfo(WallInfo.Type.Vertical, 9, 2));  // TODO 나중에 해금
        // walls.Add(new WallInfo(WallInfo.Type.Vertical, 9, 4));  // TODO 나중에 해금
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 1, 8));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 2, 8));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 3, 8));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 4, 8));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 5, 8));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 6, 8));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 7, 8));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 8, 8));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 9, 8));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 10, 8));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 2, 7));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 3, 7));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 4, 7));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 5, 7));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 6, 7));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 7, 7));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 8, 7));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 9, 7));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 10, 7));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 2, 6));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 3, 6));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 4, 6));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 5, 6));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 6, 6));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 7, 6));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 8, 6));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 9, 6));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 10, 6));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 3, 5));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 4, 5));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 5, 5));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 6, 5));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 7, 5));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 8, 5));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 9, 5));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 10, 5));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 4, 4));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 5, 4));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 6, 4));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 7, 4));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 8, 4));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 9, 4));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 10, 4));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 3, 4));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 4, 3));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 5, 3));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 6, 3));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 7, 3));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 8, 3));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 9, 3));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 10, 3));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 1, 2));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 2, 2));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 4, 2));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 5, 2));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 6, 2));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 7, 2));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 8, 2));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 9, 2));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 10, 2));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 2, 1));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 3, 1));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 4, 1));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 5, 1));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 6, 1));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 7, 1));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 8, 1));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 9, 1));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 10, 1));
        walls.Add(new WallInfo(WallInfo.Type.ExitVertical, 0, 3));

        if (File.Exists(Application.persistentDataPath + "/TutorialDone.txt"))
        {
            FileStream fs = new FileStream(Application.persistentDataPath + "/TutorialDone.txt", FileMode.Open, FileAccess.ReadWrite);
            StreamReader sr = new StreamReader(fs, Encoding.UTF8);

            try
            {
                string line = sr.ReadLine();
                if (line.TrimEnd().Equals("3"))
                {
                    GameObject g = Instantiate(floorStarPrefab, new Vector3(), Quaternion.identity, mm.movableAndFixedGameObjects.transform);
                    g.transform.localPosition = new Vector3(7f, 6f, 0f);
                    GameObject h = Instantiate(floorStarPrefab, new Vector3(), Quaternion.identity, mm.movableAndFixedGameObjects.transform);
                    h.transform.localPosition = new Vector3(8f, 6f, 0f);
                    GameObject j = Instantiate(floorStarPrefab, new Vector3(), Quaternion.identity, mm.movableAndFixedGameObjects.transform);
                    j.transform.localPosition = new Vector3(9f, 6f, 0f);
                }
            }
            catch (Exception)
            {
                Debug.LogWarning("File warning: TutorialDone.txt seems to be corrupted");
                StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                try
                {
                    fs.Position = 0;
                    sw.WriteLine("0");
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
                finally
                {
                    sw.Close();
                }
            }
            finally
            {
                sr.Close();
                fs.Close();
            }
        }

        List<ObjectInfo> objects = new List<ObjectInfo>();

        mm.afterGravity = ModeAfterGravity;

        mm.Initialize(10, 9, walls, objects, "a", float.PositiveInfinity);
        mm.TimeActivate();
        canPlay = true;
    }

    IEnumerator InitializeAdventureLevel()
    {
        while (mm == null)
        {
            mm = GameObject.FindGameObjectWithTag("MapManager").GetComponent<MapManager>();
            if (mm == null)
            {
                mm = GameObject.Find("MapManager").GetComponent<MapManager>();
            }
            yield return null;
        }
        List<WallInfo> walls = new List<WallInfo>();

        walls.Add(new WallInfo(WallInfo.Type.Vertical, 1, 1));
        walls.Add(new WallInfo(WallInfo.Type.Vertical, 1, 4));
        walls.Add(new WallInfo(WallInfo.Type.Vertical, 1, 5));
        walls.Add(new WallInfo(WallInfo.Type.Vertical, 1, 7));
        walls.Add(new WallInfo(WallInfo.Type.Vertical, 2, 2));
        walls.Add(new WallInfo(WallInfo.Type.Vertical, 2, 5));
        walls.Add(new WallInfo(WallInfo.Type.Vertical, 3, 3));
        walls.Add(new WallInfo(WallInfo.Type.Vertical, 3, 9));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 1, 8));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 2, 8));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 3, 8));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 4, 8));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 5, 8));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 6, 8));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 7, 8));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 8, 8));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 2, 7));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 3, 7));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 4, 7));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 5, 7));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 6, 7));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 7, 7));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 8, 7));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 2, 6));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 3, 6));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 4, 6));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 5, 6));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 6, 6));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 7, 6));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 8, 6));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 3, 5));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 4, 5));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 5, 5));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 6, 5));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 7, 5));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 8, 5));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 3, 4));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 4, 4));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 5, 4));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 6, 4));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 7, 4));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 8, 4));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 4, 3));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 5, 3));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 6, 3));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 7, 3));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 8, 3));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 1, 2));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 2, 2));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 4, 2));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 5, 2));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 6, 2));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 7, 2));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 8, 2));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 2, 1));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 3, 1));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 4, 1));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 5, 1));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 6, 1));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 7, 1));
        walls.Add(new WallInfo(WallInfo.Type.Horizontal, 8, 1));
        walls.Add(new WallInfo(WallInfo.Type.ExitVertical, 0, 3));

        if (!File.Exists(Application.persistentDataPath + "/AdventureLevel.txt"))
        {
            FileStream fs = null;
            StreamWriter sw = null;
            try
            {
                fs = new FileStream(Application.persistentDataPath + "/AdventureLevel.txt", FileMode.Create);
                sw = new StreamWriter(fs, Encoding.UTF8);
                sw.WriteLine("0");
                sw.WriteLine("0");
                sw.WriteLine("0");
                sw.WriteLine("0");
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
                    fs.Close();
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
        }
        else
        {
            FileStream fs = null;
            StreamReader sr = null;
            List<GameObject> floorStars = new List<GameObject>();
            bool hasReadSuccess = true;
            try
            {
                fs = new FileStream(Application.persistentDataPath + "/AdventureLevel.txt", FileMode.Open);
                using (sr = new StreamReader(fs, Encoding.UTF8))
                {
                    string line;
                    float endX = 7f;
                    float startY = 0f;
                    for (int j = 0; j < 4; j++)
                    {
                        line = sr.ReadLine().Trim();
                        if (int.TryParse(line, out int stars) && stars >= 0 && stars <= 3)
                        {
                            for (int i = 0; i < stars; i++)
                            {
                                GameObject g = Instantiate(floorStarPrefab, new Vector3(), Quaternion.identity, mm.movableAndFixedGameObjects.transform);
                                g.transform.localPosition = new Vector3(endX, 8 - 2 * startY, 0f);
                                floorStars.Add(g);
                                endX--;
                            }
                            endX = 7f;
                            startY++;
                        }
                        else
                        {
                            hasReadSuccess = false;
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                hasReadSuccess = false;
                Debug.LogError(e.Message);
            }
            finally
            {
                sr.Close();
                fs.Close();
            }

            if (!hasReadSuccess)
            {
                Debug.LogWarning("File warning: AdventureLevel.txt seems to be corrupted");
                FileStream fs2 = null;
                StreamWriter sw = null;
                try
                {
                    fs2 = new FileStream(Application.persistentDataPath + "/AdventureLevel.txt", FileMode.Create);
                    sw = new StreamWriter(fs2, Encoding.UTF8);
                    sw.WriteLine("0");
                    sw.WriteLine("0");
                    sw.WriteLine("0");
                    sw.WriteLine("0");
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
                        fs2.Close();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message);
                    }
                }
                foreach (GameObject g in floorStars)
                {
                    Destroy(g);
                }
            }
        }

        List<ObjectInfo> objects = new List<ObjectInfo>();

        mm.afterGravity = AdventureLevelAfterGravity;

        mm.Initialize(8, 9, walls, objects, "a", float.PositiveInfinity);
        mm.TimeActivate();
        canPlay = true;
    }

    IEnumerator InitializeTutorial()
    {
        while (mm == null)
        {
            mm = GameObject.FindGameObjectWithTag("MapManager").GetComponent<MapManager>();
            if (mm == null)
            {
                mm = GameObject.Find("MapManager").GetComponent<MapManager>();
            }
            yield return null;
        }

        while (pm == null)
        {
            pm = GameObject.FindGameObjectWithTag("PlayManager").GetComponent<PlayManager>();
            if (pm != null) break;
            else
            {
                pm = GameObject.Find("PlayManager").GetComponent<PlayManager>();
            }
            yield return null;
        }

        pm.Initialize(PlayManager.Mode.Tutorial);
        mm.afterGravity = pm.TutorialAfterGravity;

        //mapList = Directory.GetFiles("Assets/PredefinedMaps/Tutorial/", "*.txt").ToList();

        playingMapIndex = -1;
        PlayNext();
    }
    
    IEnumerator InitializeAdventure()
    {
        while (mm == null)
        {
            mm = GameObject.FindGameObjectWithTag("MapManager").GetComponent<MapManager>();
            if (mm == null)
            {
                mm = GameObject.Find("MapManager").GetComponent<MapManager>();
            }
            yield return null;
        }

        while (pm == null)
        {
            pm = GameObject.FindGameObjectWithTag("PlayManager").GetComponent<PlayManager>();
            if (pm != null) break;
            else
            {
                pm = GameObject.Find("PlayManager").GetComponent<PlayManager>();
            }
            yield return null;
        }
        Debug.Log(adventureLevel);

        switch (adventureLevel)
        {
            case AdventureLevel.Easy:
                pm.Initialize(PlayManager.Mode.AdvEasy, true);
                break;
            case AdventureLevel.Normal:
                pm.Initialize(PlayManager.Mode.AdvNormal, true);
                break;
            case AdventureLevel.Hard:
                pm.Initialize(PlayManager.Mode.AdvHard, true);
                break;
            case AdventureLevel.Insane:
                pm.Initialize(PlayManager.Mode.AdvInsane, true);
                break;
            default:
                Debug.LogError("Play invalid: unknown adventure level");
                LoadAdventureLevel();
                yield break;
        }

        mm.afterGravity = pm.AdventureAfterGravity;

        playingMapIndex = -1;
        PlayNext();
    }

    public void MainAfterGravity(MapManager.Flag flag)
    {
        switch (flag)
        {
            case MapManager.Flag.Escaped:
                LoadMode();
                break;
            case MapManager.Flag.MapEditor:
                LoadEditor();
                break;
            case MapManager.Flag.QuitGame:
                QuitGame();
                break;
            case MapManager.Flag.Setting:
                LoadSetting();
                break;
        }
    }

    public void ModeAfterGravity(MapManager.Flag flag)
    {
        switch (flag)
        {
            case MapManager.Flag.Escaped:
                LoadMain();
                break;
            case MapManager.Flag.Adventure:
                LoadAdventureLevel();
                break;
            case MapManager.Flag.Tutorial:
                LoadTutorial();
                break;
            case MapManager.Flag.Custom:
                LoadCustom();
                break;
            case MapManager.Flag.Training:
                LoadTraining();
                break;
        }
    }

    public void AdventureLevelAfterGravity(MapManager.Flag flag)
    {
        switch (flag)
        {
            case MapManager.Flag.Escaped:
                LoadMode();
                break;
            case MapManager.Flag.AdvEasy:
                LoadAdventureEasy();
                break;
            case MapManager.Flag.AdvNormal:
                LoadAdventureNormal();
                break;
            case MapManager.Flag.AdvHard:
                LoadAdventureHard();
                break;
            case MapManager.Flag.AdvInsane:
                LoadAdventureInsane();
                break;
        }
    }

    public void TutorialNext()
    {
        if (pm == null || pm.MapFiles == null || pm.MapFiles.Count == 0) return;

        foreach (Transform obj in GameObject.Find("Objects").GetComponentsInChildren<Transform>())
        {
            if (obj.gameObject.name.Equals("Objects")) continue;
            Destroy(obj.gameObject);
        }

        PlayNext();
    }

    public void PlayNext()
    {
        // TODO mapList? or pm.mapFiles?
        if (pm == null || pm.MapFiles == null || pm.MapFiles.Count == 0) return;

        for (int i = playingMapIndex + 1; i <= pm.MapFiles.Count; i++)
        {
            if (pm.HasClearedAll)
            {
                // TODO Victory
                break;
            }
            MapManager.OpenFileFlag openFileFlag = mm.InitializeFromText(pm.MapFiles[i].text, out _, out _, out _, out _, out _, out _);
            if (openFileFlag != MapManager.OpenFileFlag.Success)
            {
                continue;
            }
            else
            {
                //Debug.Log("Map name: " + pm.MapFiles[i].name);
                playingMapIndex = i;
                pm.AdventureAfterGravity(MapManager.Flag.Continued);
                mm.TimeActivate();
                canPlay = true;
                break;
            }
        }
    }

    public void ReviseStar(PlayManager.Mode mode, int star)
    {
        FileStream fs = null;
        StreamWriter sw = null;
        StreamReader sr = null;

        if (star < 0 || star > 3) return;

        if (!File.Exists(Application.persistentDataPath + "/AdventureLevel.txt"))
        {
            try
            {
                fs = new FileStream(Application.persistentDataPath + "/AdventureLevel.txt", FileMode.Create);
                sw = new StreamWriter(fs, Encoding.UTF8);
                sw.WriteLine("0");
                sw.WriteLine("0");
                sw.WriteLine("0");
                sw.WriteLine("0");
                sw.Close();
                fs.Close();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        try
        {
            fs = new FileStream(Application.persistentDataPath + "/AdventureLevel.txt", FileMode.Open, FileAccess.ReadWrite);
            using (sr = new StreamReader(fs, Encoding.UTF8))
            using (sw = new StreamWriter(fs, Encoding.UTF8))
            {
                string line;
                List<string> lines = new List<string>();
                bool hasReadSuccess = true;

                for (int j = 0; j < 4; j++)
                {
                    line = sr.ReadLine();
                    if (line != null)
                    {
                        lines.Add(line.Trim());
                    }
                    else
                    {
                        hasReadSuccess = false;
                        break;
                    }
                }

                if (hasReadSuccess)
                {
                    fs.Position = 0;
                    int i = (int)PlayManager.Mode.AdvEasy;
                    foreach (string l in lines)
                    {
                        hasReadSuccess = int.TryParse(l, out int oldStar);
                        if (!hasReadSuccess || oldStar < 0 || oldStar > 3)
                        {
                            break;
                        }

                        if ((int)mode == i)
                        {
                            sw.WriteLine(Math.Max(oldStar, star));
                        }
                        else
                        {
                            sw.WriteLine(oldStar);
                        }
                        i++;
                    }
                }

                if (!hasReadSuccess)
                {
                    Debug.LogWarning("File warning: AdventureLevel.txt seems to be corrupted");
                    fs.Position = 0;
                    for (int j = 0; j < 4; j++)
                    {
                        if ((int)mode == (int)PlayManager.Mode.AdvEasy + j)
                        {
                            sw.WriteLine(star);
                        }
                        else
                        {
                            sw.WriteLine("0");
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }
}
