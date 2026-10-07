using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class EndingsManager : MonoBehaviour
{
    public static EndingsManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private CanvasGroup endingsCanvas;
    [SerializeField] private TextMeshProUGUI endingText;
    [SerializeField] private Button returnButton;

    [Header("Сцены")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Тайминги")]
    [SerializeField] private float textFadeDuration = 1.5f;
    [SerializeField] private float holdDuration = 5f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (endingsCanvas != null)
        {
            endingsCanvas.alpha = 0f;
            endingsCanvas.blocksRaycasts = false;
        }

        if (returnButton != null)
        {
            returnButton.onClick.AddListener(ReturnToMenu);
            returnButton.gameObject.SetActive(false);
        }
    }

    public void TriggerInsanityEnding()
    {
        StartCoroutine(PlayEnding("...ты любила его слишком сильно."));
    }

    public void TriggerLoveEnding()
    {
        StartCoroutine(PlayEnding("почему ты была так эгоистична?"));
    }

    public void TriggerTrueEnding()
    {
        StartCoroutine(PlayEnding("...я согласна!"));
    }

    IEnumerator PlayEnding(string message)
    {
        if (endingsCanvas == null)
        {
            Debug.LogError("[EndingsManager] endingsCanvas не назначен!");
            yield break;
        }

        // Фон — чёрный, уже должен быть в сцене
        endingsCanvas.blocksRaycasts = true;

        // 1. Появление
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / textFadeDuration;
            endingsCanvas.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }
        endingsCanvas.alpha = 1f;

        // 2. Текст
        if (endingText != null)
            endingText.text = "";

        if (endingText != null)
        {
            // Печатаем текст посимвольно
            string full = message;
            for (int i = 0; i <= full.Length; i++)
            {
                endingText.text = full.Substring(0, i);
                yield return new WaitForSeconds(0.05f);
            }
        }

        // 3. Держим
        yield return new WaitForSeconds(holdDuration);

        // 4. Кнопка «В меню»
        if (returnButton != null)
            returnButton.gameObject.SetActive(true);
    }

    void ReturnToMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}