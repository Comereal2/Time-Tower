using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuBehavior : MonoBehaviour
{
    [SerializeField] private GameObject creditsMenuObject;
    [SerializeField] private GameObject mainMenuObject;
    [SerializeField] private GameObject settingsMenuObject;
    [SerializeField] private Button[] buttons;
    [SerializeField] private GameObject[] settings;

    private void Awake()
    {
        MusicManager.musicManager.ChangeMusic(MusicManager.musicManager.mainMenuTheme);
        BackButton();
        buttons[0].onClick.AddListener(PlayButton);
        buttons[1].onClick.AddListener(OptionsButton);
        buttons[2].onClick.AddListener(CreditsButton);
        buttons[3].onClick.AddListener(QuitButton);
        buttons[4].onClick.AddListener(BackButton);
        buttons[5].onClick.AddListener(() => { BackButton(); SaveSettings(); });
    }

    /// <summary>
    /// Executes the enemy upgrade and loads the game scene
    /// </summary>
    private void PlayButton()
    {
        foreach (var enemy in Resources.LoadAll<Enemy>("Data/Enemies"))
        {
            for(int i = 0; i < PlayerPrefs.GetInt("ChallengeRating", 0); i++)
            {
                enemy.UpgradeEnemy();
            }
        }
        //Just in case
        Time.timeScale = 1;
        SceneManager.LoadScene(1);
    }

    /// <summary>
    /// Enables the options on the main menu canvas
    /// </summary>
    private void OptionsButton()
    {
        settings[0].GetComponent<Toggle>().isOn = PlayerPrefs.GetInt("HardMode", 0) == 1;
        settings[1].GetComponent<Toggle>().isOn = PlayerPrefs.GetInt("EnemyHealthBars", 1) == 1;
        settings[2].GetComponent<Toggle>().isOn = PlayerPrefs.GetInt("ShopTags", 1) == 1;
        settings[3].GetComponent<Toggle>().isOn = PlayerPrefs.GetInt("BonusCash", 0) == 1;
        settings[4].GetComponent<InputField>().text = PlayerPrefs.GetInt("ChallengeRating", 0) == 0 ? "" : PlayerPrefs.GetInt("ChallengeRating", 0).ToString();
        settingsMenuObject.SetActive(true);
        mainMenuObject.SetActive(false);
    }

    /// <summary>
    /// Enables the credits on the main menu canvas
    /// </summary>
    private void CreditsButton()
    {
        creditsMenuObject.SetActive(true);
        mainMenuObject.SetActive(false);
    }

    /// <summary>
    /// Quits the application
    /// </summary>
    private void QuitButton()
    {
        Application.Quit();
    }

    /// <summary>
    /// Puts you back onto the start screen by hiding the other two
    /// </summary>
    private void BackButton()
    {
        creditsMenuObject.SetActive(false);
        mainMenuObject.SetActive(true);
        settingsMenuObject.SetActive(false);
    }

    /// <summary>
    /// Executed by the back button in settings
    /// </summary>
    private void SaveSettings()
    {
        PlayerPrefs.SetInt("HardMode", settings[0].GetComponent<Toggle>().isOn ? 1 : 0);
        PlayerPrefs.SetInt("EnemyHealthBars", settings[1].GetComponent<Toggle>().isOn ? 1 : 0);
        PlayerPrefs.SetInt("ShopTags", settings[2].GetComponent<Toggle>().isOn ? 1 : 0);
        PlayerPrefs.SetInt("BonusCash", settings[3].GetComponent<Toggle>().isOn ? 1 : 0);
        PlayerPrefs.SetInt("ChallengeRating", int.Parse(settings[4].GetComponent<InputField>().text));
        PlayerPrefs.Save();
    }
}
