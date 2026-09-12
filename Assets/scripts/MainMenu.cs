using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Names")]
    public string mainMenuSceneName = "MainMenu";
    public string raceSceneName = "VelocityGrandPrix";
    public string garageSceneName = "Garage";

    public void PlayRace()
    {
        SceneManager.LoadScene(raceSceneName);
    }

    public void OpenGarage()
    {
        SceneManager.LoadScene(garageSceneName);
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}