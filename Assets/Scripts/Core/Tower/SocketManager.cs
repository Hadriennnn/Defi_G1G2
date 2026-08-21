using System.Collections.Generic;
using UnityEngine;


 // Gère l'insertion physique des gemmes et les limites de place.
 
[RequireComponent(typeof(BasicTower))]
public class SocketManager : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Nombre maximum de gemmes que cette tour peut accueillir.")]
    public int maxSockets = 1; 

    // La liste des gemmes actuellement insérées
    private List<IGemEffect> equippedGems = new List<IGemEffect>();
    
    // Référence à la tour pour lui passer les gemmes
    private BasicTower tower;

    void Awake()
    {
        tower = GetComponent<BasicTower>();
    }
    
    public bool CanInsertGem()
    {
        return equippedGems.Count < maxSockets;
    }

    public bool InsertGem(IGemEffect gem)
    {
        if (!CanInsertGem())
        {
            Debug.LogWarning($"[SocketManager] Plus de place dans la tour {gameObject.name}");
            return false;
        }

        equippedGems.Add(gem);
        
        gem.Equip(tower); 
        
        Debug.Log($"[SocketManager] Gemme {gem.GemId} insérée avec succès");
        return true;
    }

    public void RemoveGem(IGemEffect gem)
    {
        if (equippedGems.Contains(gem))
        {
            gem.Unequip(tower); 
            equippedGems.Remove(gem);
        }
    }


    void OnDestroy()
    {
        foreach (var gem in equippedGems)
        {
            gem.Unequip(tower);
        }
        equippedGems.Clear();
    }
}