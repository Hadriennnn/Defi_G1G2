using UnityEngine;


public abstract class GemCardData : CardData
{
    
    [Tooltip("Cette méthode sera définie par chaque gemme spécifique.")]

    // On est obligé de créer l'effet de la gemme 
    public abstract IGemEffect CreateGemEffect(); 
}