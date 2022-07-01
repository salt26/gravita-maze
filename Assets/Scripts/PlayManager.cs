using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayManager : MonoBehaviour
{
    public Button quitButton;                   // quitHighlightedButton이 활성화될 때 비활성화
    public Button quitHighlightedButton;        // 모든 맵을 탈출하거나 라이프가 0이 되어 게임이 종료될 때 활성화
    public Button nextButton;                   // 탈출 또는 시간 초과 시 활성화 (튜토리얼에서는 탈출 시에만 활성화), quitHighlightedButton이 활성화될 때 비활성화
    public Button retryButton;                  // Continued일 때 활성화, 사망 또는 탈출 또는 시간 초과 시 비활성화
    public Button retryHighlightedButton;       // Burned 또는 Squashed일 때 활성화
    public Button retryTimeButton;              // 시간 초과 시 활성화 (튜토리얼에서는 탈출 시 활성화)
    public Button retryTimeHighlightedButton;   // (튜토리얼에서만 시간 초과 시 활성화)

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
                GameManager.gm.Life--;
                retryButton.gameObject.SetActive(false);
                retryHighlightedButton.gameObject.SetActive(false);
                retryTimeHighlightedButton.gameObject.SetActive(false);
                if (GameManager.gm.Life > 0)
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
