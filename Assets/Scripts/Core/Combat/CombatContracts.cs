using UnityEngine;
using System.Collections.Generic;

public enum ElementType
{
    Normal,
    Glace,
    Feu,
    Foudre
}

public struct HitPayload
{
    public float rawDamage;
    public ElementType element;
    public GameObject instigator; 
    public List<StatusEffect> effectsToApply; 
}

public interface IStatusReceiver
{
    void ReceivePayload(HitPayload payload);
}