using UnityEngine;

public class ExplosionVFX : MonoBehaviour
{
    public float lifetime = 2f; 
    private float timer;

    public void Setup(float aoeRadius)
    {
        // (Un rayon de 2 donne une échelle de 4)
        transform.localScale = new Vector3(aoeRadius * 2, aoeRadius * 2, aoeRadius * 2);
        timer = 0f;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            // Retour dans le PoolManager
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