using UnityEngine;

// ATTAQUE RAPIDE 

public class Gem_NoyauBasique : IGemEffect
{
    public string GemId => "Gem_NoyauBasique";
    
    // C'est CECI qui dit au SocketManager d'allumer la tour
    public GemCategory Category => GemCategory.Noyau; 
    
    // Élément Normal = Dégâts blancs, pas de statut
    public ElementType Element => ElementType.Normal; 

    public void Equip(BasicTower tower)
    {
        // On ne fait absolument rien ! La tour tirera ses projectiles par défaut.
    }

    public void Unequip(BasicTower tower)
    {
        // Rien à retirer.
    }
}
public class Gem_AttaqueRapide : IGemEffect
{
    public string GemId => "Gem_AttaqueRapide";
    public GemCategory Category => GemCategory.Amplificateur;
    public ElementType Element => ElementType.Normal; 
    
    private StatModifier speedMod;

    public void Equip(BasicTower tower)
    {
        speedMod = new StatModifier(0.3f, StatModType.PercentMult, this); // +30% Vitesse de tir
        tower.FireRate.AddModifier(speedMod);
    }

    public void Unequip(BasicTower tower)
    {
        tower.FireRate.RemoveModifier(speedMod);
    }
}

//ATTAQUE LOURDE
public class Gem_AttaqueLourde : IGemEffect
{
    public string GemId => "Gem_AttaqueLourde";
    public GemCategory Category => GemCategory.Amplificateur;
    public ElementType Element => ElementType.Normal; 
    
    private StatModifier damageMod;

    public void Equip(BasicTower tower)
    {
        damageMod = new StatModifier(0.4f, StatModType.PercentMult, this); // +40% Dégâts
        tower.Damage.AddModifier(damageMod);
    }

    public void Unequip(BasicTower tower)
    {
        tower.Damage.RemoveModifier(damageMod);
    }
}

//SNIPER
public class Gem_Sniper : IGemEffect
{
    public string GemId => "Gem_Sniper";
    public GemCategory Category => GemCategory.Amplificateur;
    public ElementType Element => ElementType.Normal; 
    
    private StatModifier fireRatePenalty;
    private StatModifier massiveDamageMod;

    public void Equip(BasicTower tower)
    {
        fireRatePenalty = new StatModifier(-0.5f, StatModType.PercentMult, this); // -50% Vitesse de tir
        massiveDamageMod = new StatModifier(1.5f, StatModType.PercentMult, this); // +150% Dégâts
        
        tower.FireRate.AddModifier(fireRatePenalty);
        tower.Damage.AddModifier(massiveDamageMod);
    }

    public void Unequip(BasicTower tower)
    {
        tower.FireRate.RemoveModifier(fireRatePenalty);
        tower.Damage.RemoveModifier(massiveDamageMod);
    }
}

//ATTAQUE COMBINÉE (Multiplie les dégâts UNIQUEMENT si la cible est ralentie)
public class Gem_AttaqueCombinee : IGemEffect
{
    public string GemId => "Gem_AttaqueCombinee";
    public GemCategory Category => GemCategory.Amplificateur;
    public ElementType Element => ElementType.Normal; 

    public void Equip(BasicTower tower)
    {
        // On se branche au moment où le projectile touche la cible
        tower.OnHit += VerifierEtBoosterDegats;
    }

    public void Unequip(BasicTower tower)
    {
        tower.OnHit -= VerifierEtBoosterDegats;
    }

    private void VerifierEtBoosterDegats(ref HitPayload payload, GameObject victim)
    {
        EnemyStatusManager status = victim.GetComponent<EnemyStatusManager>();
        
        // Si l'ennemi possède une altération taguée "Ralentissement" ou "Froid"
        if (status != null && (status.HasTag("Ralentissement") || status.HasTag("Froid")))
        {
            // On double les dégâts 
            payload.rawDamage *= 2.0f;
            Debug.Log("[Gemme] Attaque Combinée");
        }
    }
}