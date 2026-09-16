using UnityEngine;

public class ExplosionVFX : MonoBehaviour
{
    [Tooltip("Durée de vie avant de disparaître (en secondes)")]
    public float lifetime = 2f; 
    
    [Tooltip("Si coché, l'objet grossit aussi en hauteur (Sphère). Si décoché, l'objet reste plat (Disque au sol).")]
    public bool scaleYAxis = false; 

    private float timer;

    public void Setup(float aoeRadius)
    {
        // On calcule la taille (Diamètre = Rayon * 2)
        float targetSize = aoeRadius * 2f;

        // Si disque,
        float targetY = scaleYAxis ? targetSize : transform.localScale.y;

        // On applique l'échelle
        transform.localScale = new Vector3(targetSize, targetY, targetSize);
        
        timer = 0f;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            if (TryGetComponent<PooledItem>(out PooledItem poolItem))
            {
                poolItem.ReleaseToPool();
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}