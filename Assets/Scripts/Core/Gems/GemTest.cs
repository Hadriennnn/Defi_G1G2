using UnityEngine;

// "Attaque rapide : Augmente la vitesse de tir de la tour."
public class Gem_AttaqueRapide : IGemEffect
{
    public string GemId => "AttaqueRapide_01";
    
    private StatModifier speedMod;

    public void Equip(BasicTower tower)
    {
        // On crée un modificateur : +100% de cadence de tir 
        speedMod = new StatModifier(1f, StatModType.PercentMult, this);
        tower.FireRate.AddModifier(speedMod);
    }

    public void Unequip(BasicTower tower)
    {
        // Si on retire la gemme, la tour retrouve sa cadence de base instantanément
        tower.FireRate.RemoveModifier(speedMod);
    }
}

// --- GEMME 2 : INTERCEPTION D'ÉVÉNEMENT ---
// "Noyau Ignifère : Applique l'élément Feu."
public class Gem_NoyauIgnifere : IGemEffect
{
    public string GemId => "NoyauIgnifere_01";

    public void Equip(BasicTower tower)
    {
        // On s'abonne discrètement au moment du tir de la tour
        tower.OnBeforeShoot += ModifierColis;
    }

    public void Unequip(BasicTower tower)
    {
        // On se désabonne
        tower.OnBeforeShoot -= ModifierColis;
    }

    // Cette fonction sera appelée automatiquement par la tour JUSTE AVANT que le projectile ne parte
    private void ModifierColis(HitPayload payload)
    {
        // On change l'élément du tir en Feu
        payload.element = ElementType.Feu;
        
        // Plus tard, on fera ici : payload.effectsToApply.Add(new BurnEffect(...));
    }
}