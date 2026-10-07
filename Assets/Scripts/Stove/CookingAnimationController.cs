using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CookingAnimationController : MonoBehaviour
{
    [Header("Спавнер продуктов")]
    [SerializeField] private FallingProductsSpawner productsSpawner;

    [Header("Кастрюля")]
    [SerializeField] private GameObject pot;
    [SerializeField] private float potAppearDelay = 0f;

    [Header("Лопатка")]
    [SerializeField] private Transform spatula;
    [SerializeField] private float spatulaSpeed = 720f;

    [Header("Дым")]
    [SerializeField] private ParticleSystem smoke;

    [Header("Вспышка")]
    [SerializeField] private Image flashImage;
    [SerializeField] private float flashDuration = 0.2f;

    [Header("Готовое блюдо")]
    [Tooltip("Точка, где появится готовое блюдо (над кастрюлей)")]
    [SerializeField] private Transform dishDisplayPoint;
    [Tooltip("Сколько блюдо красуется до того, как исчезнуть")]
    [SerializeField] private float dishShowDuration = 1.5f;

    [Header("Тайминги")]
    [SerializeField] private float fallTimeAfterSpawn = 1f;
    [SerializeField] private float smokeDuration = 1f;
    [SerializeField] private float lifetimeAfterFlash = 1f;

    private Coroutine flashRoutine;
    private GameObject currentDishInstance;

    public void PlayCookingAnimation(string dishName, Action onComplete)
    {
        StartCoroutine(AnimationRoutine(dishName, onComplete));
    }

    IEnumerator AnimationRoutine(string dishName, Action onComplete)
    {
        // 1. Кастрюля
        if (pot != null)
        {
            if (potAppearDelay > 0f) yield return new WaitForSeconds(potAppearDelay);
            pot.SetActive(true);
        }

        // 2. Продукты
        if (productsSpawner != null) productsSpawner.StartSpawning();

        // 3. Лопатка
        if (spatula != null) spatula.gameObject.SetActive(true);

        // 4. Ждём, пока все упадут
        float waitTime = 2f;
        if (productsSpawner != null)
            waitTime = productsSpawner.GetTotalSpawnTime() + fallTimeAfterSpawn;

        yield return new WaitForSeconds(waitTime);

        // 5. Дым
        if (smoke != null) smoke.Play();
        yield return new WaitForSeconds(smokeDuration);

        // 6. Стоп всего
        if (productsSpawner != null) productsSpawner.StopSpawning();
        if (smoke != null) smoke.Stop();
        if (spatula != null) spatula.gameObject.SetActive(false);

        // 7. Вспышка
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine());

        // 8. Пока вспышка — прячем кастрюлю и продукты, спавним блюдо
        yield return new WaitForSeconds(flashDuration * 0.5f);

        if (productsSpawner != null) productsSpawner.ClearAll();
        if (pot != null) pot.SetActive(false);

        SpawnDish(dishName);

        yield return new WaitForSeconds(flashDuration * 0.5f);

        // 9. Блюдо красуется
        yield return new WaitForSeconds(dishShowDuration);

        // 10. Удаляем блюдо
        if (currentDishInstance != null)
        {
            Destroy(currentDishInstance);
            currentDishInstance = null;
        }

        // 11. Сообщаем наружу
        onComplete?.Invoke();
    }

    void SpawnDish(string dishName)
    {
        if (dishDisplayPoint == null)
        {
            Debug.LogWarning("[Cooking] Не назначен dishDisplayPoint!");
            return;
        }

        if (ItemSystem.Instance == null)
        {
            Debug.LogWarning("[Cooking] ItemSystem не найден!");
            return;
        }

        GameObject prefab = ItemSystem.Instance.GetDishPrefab(dishName);
        if (prefab == null)
        {
            Debug.LogWarning($"[Cooking] Нет префаба для '{dishName}'!");
            return;
        }

        currentDishInstance = Instantiate(prefab, dishDisplayPoint.position, dishDisplayPoint.rotation);
        Debug.Log($"[Cooking] Появилось блюдо: {dishName}");
    }

    IEnumerator FlashRoutine()
    {
        if (flashImage == null) yield break;

        flashImage.gameObject.SetActive(true);
        flashImage.color = new Color(1f, 1f, 1f, 1f);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / flashDuration;
            Color c = flashImage.color;
            c.a = Mathf.Lerp(1f, 0f, t);
            flashImage.color = c;
            yield return null;
        }

        flashImage.gameObject.SetActive(false);
    }

    void Update()
    {
        if (spatula != null && spatula.gameObject.activeSelf)
            spatula.Rotate(0f, spatulaSpeed * Time.deltaTime, 0f, Space.Self);
    }
}