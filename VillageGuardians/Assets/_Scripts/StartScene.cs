using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class StartScene : MonoBehaviour {

    public void StartGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void ContinueGame()
    {
        PlayerPrefs.SetInt("load", 1);
        SceneManager.LoadScene("Game");
    }
}
