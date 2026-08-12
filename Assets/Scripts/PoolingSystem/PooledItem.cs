using UnityEngine;
using UnityEngine.Pool; 

/*
 * RÉSUMÉ : Se place automatiquement sur les objets créés par le PoolManager.
 * Permet à l'objet de savoir dans quel Pool il doit retourner.
 */
public class PooledItem : MonoBehaviour
{
    
    public IObjectPool<GameObject> myPool;
    
    public void ReleaseToPool()
    {
        if (myPool != null)
        {
            myPool.Release(gameObject); // On le désactive 
        }
        else
        {
            // Sécurité
            Destroy(gameObject);
        }
    }
}