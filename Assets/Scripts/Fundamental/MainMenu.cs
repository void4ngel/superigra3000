using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Кнопки")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button backButton;

    [Header("Панели")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Настройки звука")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Сцена игры")]
    [SerializeField] private string gameSceneName = "Indoors";

    void Start()
    {
        // Подключаем кнопки
        playButton.onClick.AddListener(Play);
        settingsButton.onClick.AddListener(OpenSettings);
        quitButton.onClick.AddListener(Quit);
        backButton.onClick.AddListener(CloseSettings);

        // Слайдеры
        if (musicSlider != null)
        {
            musicSlider.value = AudioListener.volume;
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = 1f;
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }

        // Стартовое состояние
        mainPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    void Play()
    {
        Debug.Log("[MainMenu] Запуск игры");
        SceneManager.LoadScene(gameSceneName);
    }

    void OpenSettings()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    void CloseSettings()
    {
        mainPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    void Quit()
    {
        Debug.Log("[MainMenu] Выход из игры");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void SetMusicVolume(float value)
    {
        // Если у тебя есть AudioMixer — управляй через него
        // Пока просто через AudioListener
        AudioListener.volume = value;
    }

    void SetSFXVolume(float value)
    {
        // Позже через AudioMixer
        PlayerPrefs.SetFloat("SFXVolume", value);
        PlayerPrefs.Save();
    }
}