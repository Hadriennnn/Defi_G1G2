using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Stat
{
    public float baseValue; // La valeur de base 
    
    private float lastBaseValue = float.MinValue;
    private float finalValue;
    private bool isDirty = true; // Permet de savoir si on doit recalculer
    
    private readonly List<StatModifier> statModifiers;

    public Stat(float baseValue)
    {
        this.baseValue = baseValue;
        statModifiers = new List<StatModifier>();
    }

    // propriété qu'on appellera pour avoir la valeur finale
    public float Value
    {
        get
        {
            // Si on modifie la valeur de base en plein jeu dans l'inspecteur, on force le recalcul
            if (isDirty || baseValue != lastBaseValue)
            {
                lastBaseValue = baseValue;
                finalValue = CalculateFinalValue();
                isDirty = false;
            }
            return finalValue;
        }
    }

    public void AddModifier(StatModifier mod)
    {
        isDirty = true;
        statModifiers.Add(mod);
    }

    public bool RemoveModifier(StatModifier mod)
    {
        if (statModifiers.Remove(mod))
        {
            isDirty = true;
            return true;
        }
        return false;
    }

    public bool RemoveAllModifiersFromSource(object source)
    {
        bool didRemove = false;
        for (int i = statModifiers.Count - 1; i >= 0; i--)
        {
            if (statModifiers[i].Source == source)
            {
                isDirty = true;
                didRemove = true;
                statModifiers.RemoveAt(i);
            }
        }
        return didRemove;
    }

    private float CalculateFinalValue()
    {
        float finalCalc = baseValue;
        float percentMult = 1f; // 1 = 100%

        foreach (StatModifier mod in statModifiers)
        {
            if (mod.Type == StatModType.Flat)
            {
                finalCalc += mod.Value;
            }
            else if (mod.Type == StatModType.PercentMult)
            {
                percentMult *= (1 + mod.Value);
            }
        }

        // On applique toujours les pourcentages en dernier
        return (float)Math.Round(finalCalc * percentMult, 4);
    }
}