using UnityEngine;

[CreateAssetMenu(fileName = "New_Gem_AttaqueLourde", menuName = "TowerDefense/Cards/Gems/Normal/Attaque Lourde")]
public class GemCard_AttaqueLourde : GemCardData
{
    public override IGemEffect CreateGemEffect()
    {
        return new Gem_AttaqueLourde();
    }
}