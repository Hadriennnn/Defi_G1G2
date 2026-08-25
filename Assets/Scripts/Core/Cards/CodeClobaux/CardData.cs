using UnityEngine;

public abstract class CardData : ScriptableObject
{
    [Header("Informations de Base")]
    public string idCarte; 
    public string nomCarte;
    [TextArea] public string description;
    
    public int cost; 
    public Sprite icon; 

    // Renvoie 'true' si la carte a bien été jouée (pour la défausser).
    public abstract bool TryPlayCard(Vector2Int targetCoords, PlacementManager context);
}