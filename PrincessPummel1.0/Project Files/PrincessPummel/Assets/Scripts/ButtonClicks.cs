using UnityEngine;
using System.Collections;

public class ButtonClicks : MonoBehaviour {

    public void RestartLevel()
    {
        GameManager.GameInstance.RestartLevel();
    }

    public void ExitLevel()
    {
        GameManager.GameInstance.ExitLevel();
    }

    public void Play()
    {
        GameManager.GameInstance.Play();
    }

    public void Resume()
    {
        GameManager.GameInstance.Resume();
    }

    public void Continue()
    {
        GameManager.GameInstance.Continue();
    }

    public void ExitGame()
    {
        GameManager.GameInstance.ExitGame();
    }
}
