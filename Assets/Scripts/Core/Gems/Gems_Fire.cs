using UnityEngine;

//NOYAU IGNIFÈRE 
public class Gem_NoyauIgnifere : IGemEffect
{
    public string GemId => "Gem_NoyauIgnifere";
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
        //effet de brulure là plus tard
    }
}