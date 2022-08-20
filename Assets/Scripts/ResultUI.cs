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

    public float uiTimer = 0;
    public bool uiOnce = false;
    public GameObject star1;
    public GameObject star2;
    public GameObject star3;

    public void Start(){
        star1 = GameObject.Find("Star1");
        star2 = GameObject.Find("Star2");
        star3 = GameObject.Find("Star3");

        star1.SetActive(false);
        star2.SetActive(false);
        star3.SetActive(false);
        
    }
    

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
        
        uiOnce = true;

        /*
            if(uiTimer >= 2.3){
                ScoreStars(pm.PlayMode);
                uiOnce = true;
            }
        }
        */
    }

    public void Update(){
        if(uiOnce){
            uiTimer = uiTimer+Time.deltaTime;
            if(uiTimer >= 2){
                ScoreStars(pm.PlayMode);
                uiOnce = false;
            }
        }

    }


    public void ScoreStars(PlayManager.Mode mode){
        // 1-> 3-> 2 순서대로 줌

        if (mode == PlayManager.Mode.Tutorial)
        {
            if(pm.EscapedCount >= 3){
                Animation star1ani = star1.GetComponent<Animation>();
                star1ani.Play();
                star1.SetActive(true);
                
            }
            if(pm.EscapedCount >= 6){
                Animation star3ani = star3.GetComponent<Animation>();
                star3ani.Play();
                star3.SetActive(true);
            }
            if(pm.EscapedCount == 8){
                Animation star2ani = star2.GetComponent<Animation>();
                star2ani.Play();
                star2.SetActive(true);
            }
        }

        else if (mode == PlayManager.Mode.AdvEasy)
        {
            
        }

        else if (mode == PlayManager.Mode.AdvNormal)
        {
            
        }

        else if (mode == PlayManager.Mode.AdvHard)
        {
            
        }

        else if (mode == PlayManager.Mode.AdvInsane)
        {
            
        }

    }



}
