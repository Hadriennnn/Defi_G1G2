using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/*
 * RÉSUMÉ : Singleton universel qui génère un Object Pool unique par type de Prefab.
 */
public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    private Dictionary<GameObject, ObjectPool<GameObject>> pools = new Dictionary<GameObject, ObjectPool<GameObject>>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!pools.ContainsKey(prefab))
        {
            pools[prefab] = new ObjectPool<GameObject>(
                createFunc: () => 
                {
                    GameObject obj = Instantiate(prefab);
                    PooledItem tracker = obj.AddComponent<PooledItem>();
                    tracker.myPool = pools[prefab];
                    return obj;
                },
                actionOnGet: (obj) => 
                {
                    // On se contente juste de l'allumer ici
                    obj.SetActive(true);
                },
                actionOnRelease: (obj) => 
                {
                    obj.SetActive(false);
                },
                actionOnDestroy: (obj) => 
                {
                    Destroy(obj);
                },
                collectionCheck: true,
                defaultCapacity: 20,
                maxSize: 200
            );
        }
        
        GameObject spawnedObj = pools[prefab].Get();
        
        spawnedObj.transform.position = position;
        spawnedObj.transform.rotation = rotation;

        return spawnedObj;
    }
}