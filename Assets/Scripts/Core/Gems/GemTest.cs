using UnityEngine;

public class Gem_AttaqueRapide : IGemEffect
{
    public string GemId => "AttaqueRapide_01";
    
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

    public void Equip(BasicTower tower)
    {
        tower.OnBeforeShoot += ModifierColis;
    }

    public void Unequip(BasicTower tower)
    {
        tower.OnBeforeShoot -= ModifierColis;
    }

    // NOUVEAU : On ajoute "ref" ici pour modifier le struct original
    private void ModifierColis(ref HitPayload payload)
    {
        payload.element = ElementType.Feu;
        
        // Exemple de comment tu feras plus tard pour ajouter un effet sans crasher :
        // if (payload.effectsToApply == null) payload.effectsToApply = new System.Collections.Generic.List<StatusEffect>();
        // payload.effectsToApply.Add(new BurnEffect(...));
    }
}