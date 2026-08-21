using UnityEngine;

public enum StatModType
{
    Flat,       // Addition pure (ex: +10 dégâts)
    PercentMult // Multiplicateur (ex: +0.5f pour +50%, ou -0.2f pour -20%)
}

public class StatModifier
{
    public float Value { get; private set; }
    public StatModType Type { get; private set; }
    
    // On garde une trace de qui a posé ce modificateur (la Gemme), 
    public object Source { get; private set; } 

    public StatModifier(float value, StatModType type, object source = null)
    {
        Value = value;
        Type = type;
        Source = source;
    }
}