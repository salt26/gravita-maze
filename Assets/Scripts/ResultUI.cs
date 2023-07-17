using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using Interhaptics.Internal;

public class ResultUI : MonoBehaviour
{

    [SerializeField]
    private EventHapticSource[] eventHapticSource;
    [SerializeField]
    private float delayPlayTime = 0.0f;

#if UNITY_ANDROID && !UNITY_EDITOR
    private void OntriggerHaptic(int hapticNum)
    {
        eventHapticSource[hapticNum].delayPlay = delayPlayTime;
        eventHapticSource[hapticNum].PlayEventVibration();
    }
#endif


    public string tableName = "StringTable";

    public Text modeText;
    public Text difficultyText;
    public Text upperMessage;
    public Text lowerMessage;
    public Text mapsPassedCount;
    public Text mapsSkippedCount;
    public Text livesLeftCount;

    public float uiTimer = 0;
    public bool uiOnce = false;
    public GameObject star1;
    public GameObject star2;
    public GameObject star3;

    Animator starBang;

    private float star3Life;
    private float star2Life;
    private float levelMaxLife;

    public bool Ended = false;

    public float starDelay = 0.5f;

    public void Start(){
        starBang = GetComponent<Animator>(); 
        star1.SetActive(false); 
        star2.SetActive(false); 
        star3.SetActive(false);
    }
    

    public PlayManager pm;

    public void Initialize(PlayManager.Mode mode)
    {
        switch (mode)
        {
            case PlayManager.Mode.Tutorial:
                modeText.text = "Tutorial";
                //difficultyText.text = "";
                if (pm.HasClearedAll)
                {
                    upperMessage.text = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "result_ui_upper_message_pass");
                    lowerMessage.text = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "result_ui_lower_message_pass_tutorial");
                }
                else
                {
                    upperMessage.text = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "result_ui_upper_message_abort");
                    lowerMessage.text = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "result_ui_lower_message_abort");
                }
                mapsPassedCount.text = pm.EscapedCount.ToString();
                //mapsSkippedCount.text = "";
                //livesLeftCount.text = "";
                mapsPassedCount.color = new Color(0f, 100 / 255f, 0f);
                break;
            case PlayManager.Mode.AdvEasy:
                modeText.text = "Adventure";
                difficultyText.text = "Easy";
                difficultyText.color = Color.blue;
                if (pm.HasClearedAll)
                {
                    upperMessage.text = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "result_ui_upper_message_pass");
                    lowerMessage.text = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "result_ui_lower_message_pass_easy");
                }
                else if (pm.Life == 0)
                {
                    upperMessage.text = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "result_ui_upper_message_fail");
                    lowerMessage.text = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "result_ui_lower_message_fail");
                }
                else
                {
                    upperMessage.text = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "result_ui_upper_message_abort");
                    lowerMessage.text = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "result_ui_lower_message_abort");
                }
                mapsPassedCount.text = pm.EscapedCount.ToString();
                mapsSkippedCount.text = pm.SkippedCount.ToString();
                livesLeftCount.text = pm.Life.ToString();
                mapsPassedCount.color = new Color(0f, 100 / 255f, 0f);
                mapsSkippedCount.color = Color.blue;
                livesLeftCount.color = Color.red;
                break;
            case PlayManager.Mode.AdvNormal:
                modeText.text = "Adventure";
                difficultyText.text = "Normal";
                difficultyText.color = new Color(0f, 100 / 255f, 0f);
                if (pm.HasClearedAll)
                {
                    upperMessage.text = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "result_ui_upper_message_pass");
                    lowerMessage.text = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "result_ui_lower_message_pass_normal");
                }
                else if (pm.Life == 0)
                {
                    upperMessage.text = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "result_ui_upper_message_fail");
                    lowerMessage.text = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "result_ui_lower_message_fail");
                }
                else
                {
                    upperMessage.text = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "result_ui_upper_message_abort");
                    lowerMessage.text = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "result_ui_lower_message_abort");
                }
                mapsPassedCount.text = pm.EscapedCount.ToString();
                mapsSkippedCount.text = pm.SkippedCount.ToString();
                livesLeftCount.text = pm.Life.ToString();
                mapsPassedCount.color = new Color(0f, 100 / 255f, 0f);
                mapsSkippedCount.color = Color.blue;
                livesLeftCount.color = Color.red;
                break;
            case PlayManager.Mode.AdvHard:
                modeText.text = "Adventure";
                difficultyText.text = "Hard";
                difficultyText.color = new Color(1f, 165 / 255f, 0f);
                if (pm.HasClearedAll)
                {
                    upperMessage.text = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "result_ui_upper_message_pass");
                    lowerMessage.text = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "result_ui_lower_message_pass_hard");
                }
                else if (pm.Life == 0)
                {
                    upperMessage.text = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "result_ui_upper_message_fail");
                    lowerMessage.text = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "result_ui_lower_message_fail");
                }
                else
                {
                    upperMessage.text = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "result_ui_upper_message_abort");
                    lowerMessage.text = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "result_ui_lower_message_abort");
                }
                mapsPassedCount.text = pm.EscapedCount.ToString();
                mapsSkippedCount.text = pm.SkippedCount.ToString();
                livesLeftCount.text = pm.Life.ToString();
                mapsPassedCount.color = new Color(0f, 100 / 255f, 0f);
                mapsSkippedCount.color = Color.blue;
                livesLeftCount.color = Color.red;
                break;
            case PlayManager.Mode.AdvInsane:
                modeText.text = "Adventure";
                difficultyText.text = "Insane";
                difficultyText.color = Color.red;
                if (pm.HasClearedAll)
                {
                    upperMessage.text = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "result_ui_upper_message_pass");
                    lowerMessage.text = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "result_ui_lower_message_pass_insane");
                }
                else if (pm.Life == 0)
                {
                    upperMessage.text = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "result_ui_upper_message_fail");
                    lowerMessage.text = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "result_ui_lower_message_fail");
                }
                else
                {
                    upperMessage.text = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "result_ui_upper_message_abort");
                    lowerMessage.text = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "result_ui_lower_message_abort");
                }
                mapsPassedCount.text = pm.EscapedCount.ToString();
                mapsSkippedCount.text = pm.SkippedCount.ToString();
                livesLeftCount.text = pm.Life.ToString();
                mapsPassedCount.color = new Color(0f, 100 / 255f, 0f);
                mapsSkippedCount.color = Color.blue;
                livesLeftCount.color = Color.red;
                break;
        }

        gameObject.SetActive(true);
        
        if (mode == PlayManager.Mode.Tutorial)
        {
            if (pm.HasClearedAll)
            {
                StartCoroutine(ResultAnimation(3));
            }
            else
            {
                StartCoroutine(ResultAnimation(0));
            }
        }
        else if (mode == PlayManager.Mode.AdvEasy || mode == PlayManager.Mode.AdvNormal ||
            mode == PlayManager.Mode.AdvHard || mode == PlayManager.Mode.AdvInsane)
        {
            if (pm.HasClearedAll)
            {
                switch (mode)
                {
                    case PlayManager.Mode.AdvEasy:
                        levelMaxLife = pm.AdventureEasyLife;
                        break;
                    case PlayManager.Mode.AdvNormal:
                        levelMaxLife = pm.AdventureNormalLife;
                        break;
                    case PlayManager.Mode.AdvHard:
                        levelMaxLife = pm.AdventureHardLife;
                        break;
                    case PlayManager.Mode.AdvInsane:
                        levelMaxLife = pm.AdventureInsaneLife;
                        break;
                }

                star3Life = levelMaxLife * 0.8f;
                star2Life = levelMaxLife * 0.5f;

                if (pm.Life >= star3Life)
                {
                    GameManager.gm.ReviseStar(mode, 3);
                    StartCoroutine(ResultAnimation(3));
                }
                else if (pm.Life >= star2Life)
                {
                    GameManager.gm.ReviseStar(mode, 2);
                    StartCoroutine(ResultAnimation(2));
                }
                else
                {
                    GameManager.gm.ReviseStar(mode, 1);
                    StartCoroutine(ResultAnimation(1));
                }
                
            }
            else if (pm.Life == 0)
            {
                StartCoroutine(ResultAnimation(0));
            }
            else 
            {
                StartCoroutine(ResultAnimation(0));
            }
        }
    }

    IEnumerator ResultAnimation(int starNum)
    {
        yield return new WaitForSeconds(2.0f);

        if (starNum >= 1)
        {
            star1.SetActive(true);
            starBang.SetTrigger("Star1");
            yield return new WaitForSeconds(0.3f);
        }

        if (starNum >= 2)
        {
            star2.SetActive(true);
            starBang.SetTrigger("Star2");
            yield return new WaitForSeconds(0.3f);

        }

        if (starNum >= 3)
        {
            star3.SetActive(true);
            starBang.SetTrigger("Star3");
        }
    }

    public void PlayFallSFX(float volume)
    {
        GameManager.gm.PlayFallSFX(volume);
#if UNITY_ANDROID && !UNITY_EDITOR
        switch (volume)
        {
            case 0.5f:
                OntriggerHaptic(4);
                break;
            case 0.75f:
                OntriggerHaptic(5);
                break;
            case 1.0f:
                OntriggerHaptic(6);
                break;
        }
#endif
    }

    public void PlayStarSFX(int num)
    {
        GameManager.gm.PlayStarSFX(num);
#if UNITY_ANDROID && !UNITY_EDITOR
        OntriggerHaptic(num);
#endif
    }

}
