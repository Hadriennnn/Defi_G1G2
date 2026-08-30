using UnityEngine;

public class DamagePopupManager : MonoBehaviour
{
    public GameObject damagePopupPrefab;

    void OnEnable()
    {
        GameEvents.OnDamagePopupRequested += SpawnPopup;
    }

    void OnDisable()
    {
        GameEvents.OnDamagePopupRequested -= SpawnPopup;
    }

    private void SpawnPopup(Vector3 position, float amount, ElementType element, bool isCrit)
    {
        // Décalage aléatoire 
        Vector3 randomOffset = new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0);
        
        GameObject popupGO = PoolManager.Instance.Spawn(damagePopupPrefab, position + randomOffset, Quaternion.identity);
        
        DamagePopup popupScript = popupGO.GetComponent<DamagePopup>();
        if (popupScript != null)
        {
            popupScript.Setup(amount, element, isCrit);
        }
    }
}