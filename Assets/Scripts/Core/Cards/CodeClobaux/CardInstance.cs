using UnityEngine;

// Sert lors de la création du deck pour éviter les memory leaks
public class CardInstance
{
    // Référence au ScriptableObject 
    public CardData Data { get; private set; }
    
    // Donnée volatile 
    public int CurrentCharges { get; set; }

    public CardInstance(CardData data)
    {
        Data = data;
        CurrentCharges = data.maxCharges;
    }
}