using UnityEngine;

/*
 * RÉSUMÉ : Le contrat de base pour TOUTES les altérations d'état du jeu.
 * Classe abstraite
 * elle sert de moule pour créer les effets.
 */
public abstract class StatusEffect
{
    // Identifiant unique pour éviter d'appliquer 10 fois le même effet séparément
    public string EffectId { get; protected set; } 
    
    
    
    // L'élément applique l'anti-synergie (Feu annule Glace)
    public ElementType Element { get; protected set; }
    // Définit la nature de l'effet (ex: "Froid", "Brulure", "BaisseArmure")
    public string[] Tags { get; protected set; }
    
    // Définit les tags que cet effet détruit au contact (ex: La Brûlure annule le "Froid")
    public string[] CancelsTags { get; protected set; }
    
    
    
    public float Duration { get; set; }
    public bool IsFinished => Duration <= 0f;

    protected GameObject Target;
    protected GameObject Source; // La tour qui a appliqué l'effet

    // Initialisation du statut
    public virtual void Init(GameObject target, GameObject source, float duration)
    {
        Target = target;
        Source = source;
        Duration = duration;
    }

    // Appelé une seule fois au moment où l'effet est appliqué sur l'ennemi
    public abstract void OnApply();

    // Appelé à chaque frame par le StatusManager de l'ennemi.
    // On passe le deltaTime pour que l'effet gère lui-même ses chronomètres.
    public abstract void OnTick(float deltaTime);

    // Appelé quand l'effet se termine naturellement ou est purgé par une anti-synergie.
    public abstract void OnRemove();

    // Appelé si la tour applique le MÊME effet alors qu'il est déjà actif.
    // mécaniques de Stacks
    public abstract void OnStack(StatusEffect newEffect);
}