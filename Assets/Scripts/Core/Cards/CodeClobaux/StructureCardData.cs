using UnityEngine;

/*
 * RÉSUMÉ : Les cartes qui posent un bâtiment sur la grille.
 */
[CreateAssetMenu(fileName = "NewStructureCard", menuName = "TowerDefense/Cards/Structure")]
public class StructureCardData : CardData
{
    [Header("Données de Structure")]
    public GameObject chassisPrefab; 
}