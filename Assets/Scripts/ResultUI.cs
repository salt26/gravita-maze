using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultUI : MonoBehaviour
{

    public Text modeText;
    public Text difficultyText;
    public Text upperMessage;
    public Text lowerMessage;
    public Text mapsPassedCount;
    public Text mapsSkippedCount;
    public Text livesLeftCount;

    public PlayManager pm;

    public void Initialize(PlayManager.Mode mode)
    {
        if (mode == PlayManager.Mode.Tutorial)
        {
            modeText.text = "Tutorial";
            //difficultyText.text = "";
            if (pm.HasClearedAll)
            {
                upperMessage.text = "Congratulations!";
                lowerMessage.text = "You seems very good at this game...";
            }
            else
            {
                upperMessage.text = "Your Result";
                lowerMessage.text = "You can try again at any time.";
            }
            mapsPassedCount.text = pm.EscapedCount.ToString();
            //mapsSkippedCount.text = "";
            //livesLeftCount.text = "";
            mapsPassedCount.color = new Color(0f, 100 / 255f, 0f);

        }

        else if (mode == PlayManager.Mode.AdvEasy)
        {
            modeText.text = "Adventure";
            difficultyText.text = "Easy";
            difficultyText.color = Color.blue;
            if (pm.HasClearedAll)
            {
                upperMessage.text = "Congratulations!";
                lowerMessage.text = "Nice! Your journey has started!";
            }
            else if (pm.Life == 0)
            {
                upperMessage.text = "Failed";
                lowerMessage.text = "You can do better next time!";
            }
            else 
            {
                upperMessage.text = "Your Result";
                lowerMessage.text = "You can try again at any time.";
            }
            mapsPassedCount.text = pm.EscapedCount.ToString();
            mapsSkippedCount.text = pm.SkippedCount.ToString();
            livesLeftCount.text = pm.Life.ToString();
            mapsPassedCount.color = new Color(0f, 100 / 255f, 0f);
            mapsSkippedCount.color = Color.blue;
            livesLeftCount.color = Color.red;
        }

        else if (mode == PlayManager.Mode.AdvNormal)
        {
            modeText.text = "Adventure";
            difficultyText.text = "Normal";
            difficultyText.color = new Color(0f, 100 / 255f, 0f);
            if (pm.HasClearedAll)
            {
                upperMessage.text = "Congratulations!";
                lowerMessage.text = "Great! You have the potential of a master!";
            }
            else if (pm.Life == 0)
            {
                upperMessage.text = "Failed";
                lowerMessage.text = "You can do better next time!";
            }
            else
            {
                upperMessage.text = "Your Result";
                lowerMessage.text = "You can try again at any time.";
            }
            mapsPassedCount.text = pm.EscapedCount.ToString();
            mapsSkippedCount.text = pm.SkippedCount.ToString();
            livesLeftCount.text = pm.Life.ToString();
            mapsPassedCount.color = new Color(0f, 100 / 255f, 0f);
            mapsSkippedCount.color = Color.blue;
            livesLeftCount.color = Color.red;
        }

        else if (mode == PlayManager.Mode.AdvHard)
        {
            modeText.text = "Adventure";
            difficultyText.text = "Hard";
            difficultyText.color = new Color(1f, 165 / 255f, 0f);
            if (pm.HasClearedAll)
            {
                upperMessage.text = "Congratulations!";
                lowerMessage.text = "Wow! How did you do this?!";
            }
            else if (pm.Life == 0)
            {
                upperMessage.text = "Failed";
                lowerMessage.text = "You can do better next time!";
            }
            else
            {
                upperMessage.text = "Your Result";
                lowerMessage.text = "You can try again at any time.";
            }
            mapsPassedCount.text = pm.EscapedCount.ToString();
            mapsSkippedCount.text = pm.SkippedCount.ToString();
            livesLeftCount.text = pm.Life.ToString();
            mapsPassedCount.color = new Color(0f, 100 / 255f, 0f);
            mapsSkippedCount.color = Color.blue;
            livesLeftCount.color = Color.red;
        }

        else if (mode == PlayManager.Mode.AdvInsane)
        {
            modeText.text = "Adventure";
            difficultyText.text = "Insane";
            difficultyText.color = Color.red;
            if (pm.HasClearedAll)
            {
                upperMessage.text = "Congratulations!";
                lowerMessage.text = "Unbelievable! You overwhelmed the map creators...";
            }
            else if (pm.Life == 0)
            {
                upperMessage.text = "Failed";
                lowerMessage.text = "You can do better next time!";
            }
            else
            {
                upperMessage.text = "Your Result";
                lowerMessage.text = "You can try again at any time.";
            }
            mapsPassedCount.text = pm.EscapedCount.ToString();
            mapsSkippedCount.text = pm.SkippedCount.ToString();
            livesLeftCount.text = pm.Life.ToString();
            mapsPassedCount.color = new Color(0f, 100 / 255f, 0f);
            mapsSkippedCount.color = Color.blue;
            livesLeftCount.color = Color.red;
        }

        gameObject.SetActive(true);
    }




}
