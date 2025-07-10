using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlePool : MonoBehaviour
{
    [System.Serializable]
    public class ParticleEntry
    {
        public string key;
        public GameObject prefab;
        public int initialSize = 10;
    }

    public List<ParticleEntry> particlesToPool;
    private Dictionary<string, Queue<GameObject>> pool = new();
    private Dictionary<string, GameObject> prefabLookup = new();

    private void Awake()
    {
        foreach (var entry in particlesToPool)
        {
            prefabLookup[entry.key] = entry.prefab;
            var queue = new Queue<GameObject>();

            for (int i = 0; i < entry.initialSize; i++)
            {
                GameObject obj = Instantiate(entry.prefab, transform);
                obj.SetActive(false);
                queue.Enqueue(obj);
            }

            pool[entry.key] = queue;
        }
    }

    public GameObject Spawn(string key, Vector3 position, Quaternion rotation)
    {
        if (!prefabLookup.ContainsKey(key))
        {
            Debug.LogWarning($"[ParticlePool] No prefab found for key: {key}");
            return null;
        }

        GameObject obj = null;

        // Попробовать взять из пула
        if (pool.ContainsKey(key))
        {
            while (pool[key].Count > 0)
            {
                var candidate = pool[key].Dequeue();
                if (candidate != null && !candidate.activeInHierarchy)
                {
                    obj = candidate;
                    break;
                }
            }
        }

        // Если не нашли, создаём новый
        if (obj == null)
        {
            obj = Instantiate(prefabLookup[key], transform);
        }

        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);

        // Запускаем автоматический возврат в пул
        StartCoroutine(AutoDespawn(key, obj, 3f));

        return obj;
    }

    private IEnumerator AutoDespawn(string key, GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);

        // Проверка, что объект всё ещё активен перед возвратом
        if (obj != null && obj.activeInHierarchy)
        {
            obj.SetActive(false);

            if (!pool.ContainsKey(key))
                pool[key] = new Queue<GameObject>();

            pool[key].Enqueue(obj);
        }
    }
}
