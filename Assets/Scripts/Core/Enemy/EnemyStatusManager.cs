using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*
 * RÉSUMÉ : Se place sur l'ennemi. Reçoit les statuts, gère les conflits élémentaires,
 * et fait tourner l'horloge des statuts actifs.
 */
public class EnemyStatusManager : MonoBehaviour
{
    // On utilise un Dictionnaire. La clé est l'ID du statut (ex: "Burn").
    private Dictionary<string, StatusEffect> activeEffects = new Dictionary<string, StatusEffect>();

    // Liste temporaire pour stocker les effets à supprimer sans casser la boucle Update
    private List<string> effectsToRemove = new List<string>();

    void Update()
    {
        if (activeEffects.Count == 0) return;

        // On fait tourner tous les statuts actifs
        foreach (var kvp in activeEffects)
        {
            StatusEffect effect = kvp.Value;
            
            effect.OnTick(Time.deltaTime);
            effect.Duration -= Time.deltaTime;

            if (effect.IsFinished)
            {
                effectsToRemove.Add(kvp.Key);
            }
        }

        // On nettoie  ceux qui sont terminés
        if (effectsToRemove.Count > 0)
        {
            foreach (string effectId in effectsToRemove)
            {
                RemoveStatus(effectId);
            }
            effectsToRemove.Clear();
        }
    }

    public void ApplyStatus(StatusEffect newEffect)
    {
        // Vérification des Anti-Synergies par les Tags
        if (CheckAndResolveAntiSynergies(newEffect))
        {
            // Si une annulation a eu lieu, l'effet entrant et l'effet actif sont détruits.
            return; 
        }

        // Gestion des Stacks (Accumulation)
        if (activeEffects.ContainsKey(newEffect.EffectId))
        {
            activeEffects[newEffect.EffectId].OnStack(newEffect);
        }
        // Application normale
        else
        {
            activeEffects.Add(newEffect.EffectId, newEffect);
            newEffect.OnApply();
        }
    }

    public void RemoveStatus(string effectId)
    {
        if (activeEffects.TryGetValue(effectId, out StatusEffect effect))
        {
            effect.OnRemove();
            activeEffects.Remove(effectId);
        }
    }

    
    // Retourne 'true' si le nouvel élément a provoqué une annulation.
    private bool CheckAndResolveAntiSynergies(StatusEffect incomingEffect)
    {
        // Si l'effet entrant n'annule rien, on passe.
        if (incomingEffect.CancelsTags == null || incomingEffect.CancelsTags.Length == 0) 
            return false;

        bool hasCancelled = false;

        // On parcourt les statuts actifs sur le monstre
        foreach (var kvp in activeEffects)
        {
            StatusEffect activeEffect = kvp.Value;
            if (activeEffect.Tags == null) continue;

            // On compare les tags annulés par le nouveau avec les tags de l'actif
            foreach (string cancelTag in incomingEffect.CancelsTags)
            {
                foreach (string activeTag in activeEffect.Tags)
                {
                    if (cancelTag == activeTag)
                    {
                        // On ajoute l'ID à notre liste de nettoyage (déjà déclarée dans le script)
                        effectsToRemove.Add(kvp.Key); 
                        hasCancelled = true;
                    }
                }
            }
        }

        // Si des conflits ont été trouvés
        if (hasCancelled)
        {
            // On supprime les statuts qui ont été annulés
            foreach (string id in effectsToRemove)
            {
                RemoveStatus(id);
            }
            effectsToRemove.Clear();
            
            // On retourne true
            return true;
        }

        return false;
    }
}