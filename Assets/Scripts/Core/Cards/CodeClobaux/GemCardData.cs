using UnityEngine;


public enum GemLogicType
{
    AttaqueRapide,
    NoyauIgnifere,
    // On ajoutera les autres ici au fur et à mesure (Sniper, OndePolaire, etc.)
}

[CreateAssetMenu(fileName = "NewGemCard", menuName = "TowerDefense/Cards/Gem")]
public class GemCardData : CardData
{
    [Header("Données de Gemme")]
    public GemLogicType logicType; 
}