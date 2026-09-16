using UnityEngine;

[CreateAssetMenu(fileName = "New_Gem_AttaqueCombinee", menuName = "TowerDefense/Cards/Gems/Normal/Attaque Combinee")]
public class GemCard_AttaqueCombinee : GemCardData
{
    public override IGemEffect CreateGemEffect()
    {
        return new Gem_AttaqueCombinee();
    }
}