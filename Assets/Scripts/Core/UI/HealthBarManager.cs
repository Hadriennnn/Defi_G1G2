using System.Collections.Generic;
using UnityEngine;

public class HealthBarManager : MonoBehaviour
{
    [Header("Configuration")]
    public GameObject healthBarPrefab;
    public Transform canvasContainer; //UI où ranger les barres
    public float yOffset = 2f; // Hauteur de la barre au-dessus du monstre

    // Le dictionnaire qui relie un Monstre 3D à sa Barre 2D
    private Dictionary<EnemyCore, GameObject> activeBars = new Dictionary<EnemyCore, GameObject>();

    private Camera mainCam;

    public void Init()
    {
        mainCam = Camera.main;
    }


    void OnEnable()
    {
        GameEvents.OnEnemySpawned += AddHealthBar;
        GameEvents.OnEnemyRemoved += RemoveHealthBar;
    }

    void OnDisable()
    {
        GameEvents.OnEnemySpawned -= AddHealthBar;
        GameEvents.OnEnemyRemoved -= RemoveHealthBar;
    }

    private void AddHealthBar(EnemyCore enemy)
    {
        // On demande une barre au PoolManager pour éviter le Garbage Collector !
        GameObject barGO = PoolManager.Instance.Spawn(healthBarPrefab, Vector3.zero, Quaternion.identity);
        barGO.transform.SetParent(canvasContainer, false); // false pour garder l'échelle propre de l'UI
        
        EnemyHealthBar barScript = barGO.GetComponent<EnemyHealthBar>();
        
        if (barScript != null)
        {
            // On s'abonne aux dégâts de cet ennemi spécifique
            enemy.OnHealthChanged += barScript.UpdateFill;
            // On initialise la barre pleine
            barScript.UpdateFill(enemy.MaxHealth.Value, enemy.MaxHealth.Value);
        }

        activeBars.Add(enemy, barGO);
    }

    private void RemoveHealthBar(EnemyCore enemy)
    {
        if (activeBars.TryGetValue(enemy, out GameObject barGO))
        {
            EnemyHealthBar barScript = barGO.GetComponent<EnemyHealthBar>();
            if (barScript != null)
            {
                enemy.OnHealthChanged -= barScript.UpdateFill;
            }

            // On rend la barre au PoolManager (via le composant PooledItem qu'il a ajouté automatiquement)
            if (barGO.TryGetComponent<PooledItem>(out PooledItem poolItem))
            {
                poolItem.ReleaseToPool();
            }
            else
            {
                Destroy(barGO);
            }

            activeBars.Remove(enemy);
        }
    }

    // LateUpdate s'exécute APRÈS que les ennemis aient bougé et que la caméra ait tourné
    void LateUpdate()
    {
        if (mainCam == null) return;

        foreach (var kvp in activeBars)
        {
            EnemyCore enemy = kvp.Key;
            GameObject barGO = kvp.Value;

            if (enemy != null && barGO != null)
            {
                // On transforme la position 3D (tête de l'ennemi) en position 2D (écran)
                Vector3 worldPos = enemy.transform.position + new Vector3(0, yOffset, 0);
                Vector3 screenPos = mainCam.WorldToScreenPoint(worldPos);

                // Si l'ennemi est derrière la caméra, on cache la barre
                if (screenPos.z < 0)
                {
                    barGO.SetActive(false);
                }
                else
                {
                    barGO.SetActive(true);
                    barGO.transform.position = screenPos;
                }
            }
        }
    }
}