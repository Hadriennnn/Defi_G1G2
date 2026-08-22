using UnityEngine;

[CreateAssetMenu(fileName = "New_Gem_NoyauIgnifere", menuName = "TowerDefense/Cards/Gems/NoyauIgnifere")]
public class GemCard_NoyauIgnifere : GemCardData
{
    public override IGemEffect CreateGemEffect()
    {
        // On retourne l'effet codé dans ton fichier GemTest.cs
        return new Gem_NoyauIgnifere(); 
    }
}