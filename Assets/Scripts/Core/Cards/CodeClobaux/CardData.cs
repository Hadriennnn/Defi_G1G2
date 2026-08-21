using UnityEngine;


 // Elle contient les infos que l'interface utilisateur (UI) va lire (Nom, Coût, Image).
public abstract class CardData : ScriptableObject
{
    [Header("Informations de Base")]
    public string idCarte; // 
    public string nomCarte;
    [TextArea] public string description;
    
    public int cost; // Le coût en Énergie
    public Sprite icon; // L'image qui s'affichera dans la main du joueur
}