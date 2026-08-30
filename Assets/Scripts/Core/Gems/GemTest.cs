using UnityEngine;

public class Gem_AttaqueRapide : IGemEffect
{
    public string GemId => "AttaqueRapide_01";
    public GemCategory Category => GemCategory.Amplificateur;
    public ElementType Element => ElementType.Normal; // Un amplificateur n'a pas vraiment d'élément
    
    private StatModifier speedMod;

    public void Equip(BasicTower tower)
    {
        speedMod = new StatModifier(1f, StatModType.PercentMult, this);
        tower.FireRate.AddModifier(speedMod);
    }

    public void Unequip(BasicTower tower)
    {
        tower.FireRate.RemoveModifier(speedMod);
    }
}

public class Gem_NoyauIgnifere : IGemEffect
{
    public string GemId => "NoyauIgnifere_01";
    public GemCategory Category => GemCategory.Noyau;
    public ElementType Element => ElementType.Feu; 

    public void Equip(BasicTower tower)
    {
        tower.OnBeforeShoot += ModifierColis;
    }

    public void Unequip(BasicTower tower)
    {
        tower.OnBeforeShoot -= ModifierColis;
    }

    private void ModifierColis(ref HitPayload payload)
    {
        payload.element = ElementType.Feu;
    }
}