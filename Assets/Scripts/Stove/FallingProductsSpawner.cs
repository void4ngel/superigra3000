using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingProductsSpawner : MonoBehaviour
{
    [Header("Префабы продуктов")]
    [Tooltip("Идут по порядку. Если продуктов больше — начнёт заново.")]
    [SerializeField] private List<GameObject> productPrefabs = new List<GameObject>();

    [Header("Где появляются")]
    [SerializeField] private Transform spawnPoint;

    [Header("Настройки полёта")]
    [SerializeField] private int totalProducts = 3;
    [SerializeField] private float spawnInterval = 0.4f;
    [SerializeField] private float startScale = 0.01f;
    [SerializeField] private float popDuration = 0.15f;

    [Header("Разброс позиции")]
    [SerializeField] private float horizontalSpread = 0.1f;
    [SerializeField] private float depthSpread = 0.1f;

    [Header("Время жизни")]
    [SerializeField] private float lifetime = 3f;

    private List<GameObject> spawnedProducts = new List<GameObject>();
    private Coroutine routine;

    // Индекс текущего продукта (не сбрасывается между спавнами, обнуляется в StartSpawning)
    private int currentProductIndex = 0;

    public void StartSpawning()
    {
        currentProductIndex = 0;
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(SpawnRoutine());
    }

    /// <summary>
    /// Просто останавливает спавн. Уже заспавненные продукты продолжают падать.
    /// </summary>
    public void StopSpawning()
    {
        if (routine != null) StopCoroutine(routine);
        // НЕ вызываем ClearAll — продукты живут своей жизнью
    }

    /// <summary>
    /// Принудительно удаляет все продукты. Используй в конце анимации, если надо очистить сцену.
    /// </summary>
    public void ClearAll()
    {
        foreach (var go in spawnedProducts)
            if (go != null) Destroy(go);
        spawnedProducts.Clear();
    }

    IEnumerator SpawnRoutine()
    {
        for (int i = 0; i < totalProducts; i++)
        {
            SpawnOne();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnOne()
    {
        if (productPrefabs.Count == 0)
        {
            Debug.LogWarning("[Spawner] Нет префабов продуктов!");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogWarning("[Spawner] Не назначен spawnPoint!");
            return;
        }

        // Идём ПО ПОРЯДКУ. Если продуктов больше, чем префабов — начинает заново.
        GameObject prefab = productPrefabs[currentProductIndex % productPrefabs.Count];
        currentProductIndex++;

        // Позиция с разбросом
        Vector3 pos = spawnPoint.position + new Vector3(
            Random.Range(-horizontalSpread, horizontalSpread),
            0f,
            Random.Range(-depthSpread, depthSpread)
        );

        GameObject go = Instantiate(prefab, pos, prefab.transform.rotation);

        // Сохраняем ОРИГИНАЛЬНЫЙ масштаб префаба
        Vector3 originalScale = prefab.transform.localScale;

        // Ставим стартовый масштаб (относительно оригинала)
        go.transform.localScale = originalScale * startScale;

        // Отключаем физику пока растёт
        Rigidbody rb = go.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = true;
        }

        StartCoroutine(PopAndFall(go, originalScale));

        spawnedProducts.Add(go);
    }

    IEnumerator PopAndFall(GameObject go, Vector3 originalScale)
    {
        // 1. Растём от startScale до 1 (относительно оригинала)
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / popDuration;
            float scale = Mathf.Lerp(startScale, 1f, t);
            if (go != null) go.transform.localScale = originalScale * scale;
            yield return null;
        }

        if (go != null) go.transform.localScale = originalScale;

        // 2. Отпускаем физику — падает
        Rigidbody rb = go.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;

        // 3. Удаляем через lifetime
        yield return new WaitForSeconds(lifetime);

        if (go != null)
        {
            spawnedProducts.Remove(go);
            Destroy(go);
        }
    }
    public float GetTotalSpawnTime()
    {
        return totalProducts * spawnInterval;
    }
}