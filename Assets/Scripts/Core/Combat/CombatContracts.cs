using UnityEngine;
using System.Collections.Generic;

// 1. On définit tous les éléments possibles du jeu
public enum ElementType
{
    Normal,
    Glace,
    Feu,
    Foudre
}

// 2. Le "colis" de dégâts enrichi
public class HitPayload
{
    public float rawDamage;
    public ElementType element;
    
    // La tour qui a généré ce tir. Indispensable pour que la tour sache si elle a tué l'ennemi.
    public GameObject instigator; 
    
    public List<StatusEffect> effectsToApply; 
}

// 3. Le contrat reste le même, mais il recevra ce nouveau colis
public interface IStatusReceiver
{
    void ReceivePayload(HitPayload payload);
}