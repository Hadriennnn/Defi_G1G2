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
    public bool isCritical; // plus tard mais au cas où

    public float aoeRadius; // Pour les AoE : =0 : single target et >0 c'est aoe
}

public interface IStatusReceiver
{
    void ReceivePayload(HitPayload payload);
}