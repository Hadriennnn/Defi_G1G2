using System.Threading.Tasks;
using UnityEngine;

public abstract class CardData : ScriptableObject
{
    [Header("Informations de Base")]
    public string idCarte; 
    public string nomCarte;
    [TextArea] public string description;
    
    public int cost; 
    public Sprite icon; 

    [Header("Système de Charges")]
    [Tooltip("Nombre d'utilisations avant que la carte ne soit détruite pour le reste de l'Acte.")]
    public int maxCharges = 1;

    public abstract Task<bool> TryPlayCardAsync(Vector2Int targetCoords, PlacementManager context);
}