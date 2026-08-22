using UnityEngine;

[CreateAssetMenu(fileName = "New_Gem_AttaqueRapide", menuName = "TowerDefense/Cards/Gems/AttaqueRapide")]
public class GemCard_AttaqueRapide : GemCardData
{
    public override IGemEffect CreateGemEffect()
    {
        // On retourne l'effet codé dans ton fichier GemTest.cs
        return new Gem_AttaqueRapide(); 
    }
}