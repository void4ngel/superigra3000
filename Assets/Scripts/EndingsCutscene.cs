using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndingsCutscene : MonoBehaviour
{
    [System.Serializable]
    public class Shot
    {
        public Sprite image;
        public float duration = 2f;
        public AudioClip sound;
    }

    [Header("UI")]
    [SerializeField] private Image shotImage;
    [SerializeField] private Image fadeOverlay;
    [SerializeField] private GameObject returnButton;

    [Header("Кадры — ЛЮБОВЬ")]
    [SerializeField] private Shot[] loveEndingShots;

    [Header("Кадры — БЕЗУМИЕ")]
    [SerializeField] private Shot[] insanityEndingShots;

    [Header("Кадры — ИСТИННАЯ")]
    [SerializeField] private Shot[] trueEndingShots;

    [Header("Тайминги")]
    [SerializeField] private float fadeInDuration = 1.5f;
    [SerializeField] private float fadeOutDuration = 2f;
    [SerializeField] private float beforeButtonDelay = 1f;

    [Header("Сцена меню")]
    [SerializeField] private string mainMenuScene = "MainMenu";

    private AudioSource audioSource;
    private Shot[] currentShots;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        if (returnButton != null) returnButton.SetActive(false);

        if (returnButton != null)
        {
            Button btn = returnButton.GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(GoToMenu);
        }

        // Начинаем с чёрного экрана
        if (fadeOverlay != null)
        {
            fadeOverlay.color = new Color(0f, 0f, 0f, 1f);
            fadeOverlay.gameObject.SetActive(true);
        }

        // Какую концовку играть
        string endingId = PlayerPrefs.GetString("EndingToPlay", "Love");
        Debug.Log($"[Endings] Играем концовку: {endingId}");

        switch (endingId)
        {
            case "Love":
                currentShots = loveEndingShots;
                break;
            case "Insanity":
                currentShots = insanityEndingShots;
                break;
            case "True":
                currentShots = trueEndingShots;
                break;
            default:
                currentShots = loveEndingShots;
                break;
        }

        StartCoroutine(PlayCutscene());
    }

    IEnumerator PlayCutscene()
    {
        // 1. Проявляемся из чёрного
        yield return StartCoroutine(Fade(fadeOverlay, 1f, 0f, fadeInDuration));

        // 2. Играем кадры
        for (int i = 0; i < currentShots.Length; i++)
        {
            Shot shot = currentShots[i];

            if (shotImage != null && shot.image != null)
                shotImage.sprite = shot.image;

            if (shot.sound != null && audioSource != null)
                audioSource.PlayOneShot(shot.sound);

            yield return new WaitForSeconds(shot.duration);
        }

        // 3. Затемнение в конце
        if (fadeOverlay != null) fadeOverlay.gameObject.SetActive(true);
        yield return StartCoroutine(Fade(fadeOverlay, 0f, 1f, fadeOutDuration));

        // 4. Пауза и кнопка
        yield return new WaitForSeconds(beforeButtonDelay);

        if (returnButton != null) returnButton.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    IEnumerator Fade(Image img, float from, float to, float duration)
    {
        if (img == null) yield break;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            Color c = img.color;
            c.a = Mathf.Lerp(from, to, t);
            img.color = c;
            yield return null;
        }

        Color final = img.color;
        final.a = to;
        img.color = final;
    }

    void GoToMenu()
    {
        PlayerPrefs.DeleteKey("EndingToPlay");
        PlayerPrefs.Save();
        SceneManager.LoadScene(mainMenuScene);
    }
}