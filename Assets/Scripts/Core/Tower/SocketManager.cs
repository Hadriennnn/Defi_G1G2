using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BasicTower))]
public class SocketManager : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Nombre maximum de gemmes que cette tour peut accueillir de base.")]
    public int maxSockets = 2; 

    private List<IGemEffect> equippedGems = new List<IGemEffect>();
    private BasicTower tower;
    
    // NOUVEAU
    private TowerVisuals visuals;

    void Awake()
    {
        tower = GetComponent<BasicTower>();
        visuals = GetComponentInChildren<TowerVisuals>(); // On le cherche sur l'objet ou ses enfants
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
        
        if (gem.Category == GemCategory.Noyau)
        {
            tower.hasCore = true;
            Debug.Log($"[SocketManager] Noyau {gem.Element} inséré ! La tour {gameObject.name} est active.");
            
            //On allume le visuel avec la bonne couleur
            if (visuals != null) visuals.TurnOn(gem.Element);
        }
        else
        {
            Debug.Log($"[SocketManager] Gemme amplificatrice {gem.GemId} insérée avec succès.");
        }

        return true;
    }

    public void RemoveGem(IGemEffect gem)
    {
        if (equippedGems.Contains(gem))
        {
            gem.Unequip(tower); 
            equippedGems.Remove(gem);

            if (gem.Category == GemCategory.Noyau)
            {
                CheckForCore();
            }
        }
    }

    private void CheckForCore()
    {
        bool stillHasCore = false;
        ElementType lastCoreElement = ElementType.Normal;

        foreach (var g in equippedGems)
        {
            if (g.Category == GemCategory.Noyau)
            {
                stillHasCore = true;
                lastCoreElement = g.Element;
                break;
            }
        }
        
        tower.hasCore = stillHasCore;
        if (!stillHasCore)
        {
            Debug.LogWarning($"[SocketManager] Attention ! La tour {gameObject.name} n'a plus de noyau, elle est désactivée !");
            // On éteint le visuel
            if (visuals != null) visuals.TurnOff();
        }
        else
        {
            // S'il reste un autre noyau, on met à jour la couleur
            if (visuals != null) visuals.TurnOn(lastCoreElement);
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