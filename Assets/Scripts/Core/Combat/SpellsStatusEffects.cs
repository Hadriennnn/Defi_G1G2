using UnityEngine;

// Paralyse complètement l'ennemi. Onde de stase
public class Status_GelTotal : StatusEffect
{
    public Status_GelTotal()
    {
        EffectId = "Status_GelTotal";
        Element = ElementType.Glace;
        Tags = new string[] { "Glace", "GelTotal" };
        CancelsTags = new string[] { "Feu", "Brulure" };
    }

    private StatModifier stunMod;

    public override void OnApply()
    {
        EnemyCore enemy = Target.GetComponent<EnemyCore>();
        if (enemy != null)
        {
            // Mettre -1 = -100% de vitesse de déplaceement
            stunMod = new StatModifier(-1f, StatModType.PercentMult, this);
            enemy.Speed.AddModifier(stunMod);
            
            // Plus tard : geler le modèle 3D ou couper l'animation ici
        }
    }

    public override void OnTick(float deltaTime)
    {
        // La stase ne fait pas de dégâts, elle dure juste son temps
    }

    public override void OnRemove()
    {
        EnemyCore enemy = Target.GetComponent<EnemyCore>();
        if (enemy != null && stunMod != null)
        {
            enemy.Speed.RemoveModifier(stunMod);
        }
    }

    public override void OnStack(StatusEffect newEffect)
    {
        // Si on relance une Onde de Stase sur un ennemi déjà gelé, on rafraîchit la durée
        if (newEffect.Duration > this.Duration)
        {
            this.Duration = newEffect.Duration;
        }
    }
}

//FUMIGÈNE (Ralentissement sévère, sans élément)
public class Status_Fumigene : StatusEffect
{
    public Status_Fumigene()
    {
        EffectId = "Status_Fumigene";
        Element = ElementType.Normal; 
        Tags = new string[] { "Ralentissement", "Fumigene" };
        CancelsTags = new string[] { }; // Le fumigène n'annule rien
    }

    private StatModifier slowMod;

    public override void OnApply()
    {
        EnemyCore enemy = Target.GetComponent<EnemyCore>();
        if (enemy != null)
        {
            // -70% de vitesse
            slowMod = new StatModifier(-0.7f, StatModType.PercentMult, this);
            enemy.Speed.AddModifier(slowMod);
        }
    }

    public override void OnTick(float deltaTime) { }

    public override void OnRemove()
    {
        EnemyCore enemy = Target.GetComponent<EnemyCore>();
        if (enemy != null && slowMod != null)
        {
            enemy.Speed.RemoveModifier(slowMod);
        }
    }

    public override void OnStack(StatusEffect newEffect)
    {
        if (newEffect.Duration > this.Duration) this.Duration = newEffect.Duration;
    }
}