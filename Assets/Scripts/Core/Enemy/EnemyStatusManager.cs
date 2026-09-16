using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyStatusManager : MonoBehaviour
{
    private Dictionary<string, StatusEffect> activeEffects = new Dictionary<string, StatusEffect>();
    private List<string> effectsToRemove = new List<string>();

    void Update()
    {
        if (activeEffects.Count == 0) return;

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

        if (effectsToRemove.Count > 0)
        {
            foreach (string effectId in effectsToRemove) RemoveStatus(effectId);
            effectsToRemove.Clear();
        }
    }

    public void ApplyStatus(StatusEffect newEffect)
    {
        if (CheckAndResolveAntiSynergies(newEffect)) return; 

        if (activeEffects.ContainsKey(newEffect.EffectId))
        {
            activeEffects[newEffect.EffectId].OnStack(newEffect);
        }
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

    private bool CheckAndResolveAntiSynergies(StatusEffect incomingEffect)
    {
        if (incomingEffect.CancelsTags == null || incomingEffect.CancelsTags.Length == 0) return false;

        bool hasCancelled = false;

        foreach (var kvp in activeEffects)
        {
            StatusEffect activeEffect = kvp.Value;
            if (activeEffect.Tags == null) continue;

            foreach (string cancelTag in incomingEffect.CancelsTags)
            {
                if (activeEffect.Tags.Contains(cancelTag))
                {
                    effectsToRemove.Add(kvp.Key); 
                    hasCancelled = true;
                }
            }
        }

        if (hasCancelled)
        {
            foreach (string id in effectsToRemove) RemoveStatus(id);
            effectsToRemove.Clear();
            return true;
        }

        return false;
    }

    // NOUVEAU : Permet aux Gemmes de demander "Est-ce que cet ennemi est gelé/ralenti/brûlé ?"
    public bool HasTag(string tagToCheck)
    {
        foreach (var effect in activeEffects.Values)
        {
            if (effect.Tags != null && effect.Tags.Contains(tagToCheck)) return true;
        }
        return false;
    }
}