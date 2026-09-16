using UnityEngine;

// --- 1. ONDE DE STASE ---

// La Logique C# pure
public class SpellLogic_OndeDeStase : ISpellEffect
{
    private float aoeRadius;
    private float duration;

    public SpellLogic_OndeDeStase(float radius, float stunDuration)
    {
        aoeRadius = radius;
        duration = stunDuration;
    }

    public void Execute(Vector3 targetPosition)
    {
        Debug.Log($"[Sort] Onde de Stase explosée ! Rayon: {aoeRadius}");
        
        Collider[] hits = Physics.OverlapSphere(targetPosition, aoeRadius);
        foreach (Collider c in hits)
        {
            EnemyStatusManager statusManager = c.GetComponent<EnemyStatusManager>();
            if (statusManager != null)
            {
                Status_GelTotal gel = new Status_GelTotal();
                gel.Init(c.gameObject, null, duration);
                statusManager.ApplyStatus(gel);
            }
        }
    }
}

// Le ScriptableObject (La Carte dans l'éditeur)
[CreateAssetMenu(fileName = "New_Spell_OndeStase", menuName = "TowerDefense/Cards/Spells/Onde de Stase")]
public class SpellCard_OndeDeStase : SpellCardData
{
    [Header("Paramètres du Sort")]
    public float radius = 5f;
    public float freezeDuration = 3f;

    public override ISpellEffect CreateSpellEffect()
    {
        return new SpellLogic_OndeDeStase(radius, freezeDuration);
    }
}


// --- 2. FUMIGÈNE ---

// La Logique C# pure
public class SpellLogic_Fumigene : ISpellEffect
{
    private float aoeRadius;
    private float duration;

    public SpellLogic_Fumigene(float radius, float smokeDuration)
    {
        aoeRadius = radius;
        duration = smokeDuration;
    }

    public void Execute(Vector3 targetPosition)
    {
        Debug.Log($"[Sort] Fumigène lancé ! Rayon: {aoeRadius}");
        
        Collider[] hits = Physics.OverlapSphere(targetPosition, aoeRadius);
        foreach (Collider c in hits)
        {
            EnemyStatusManager statusManager = c.GetComponent<EnemyStatusManager>();
            if (statusManager != null)
            {
                Status_Fumigene fumi = new Status_Fumigene();
                fumi.Init(c.gameObject, null, duration);
                statusManager.ApplyStatus(fumi);
            }
        }
    }
}

// Le ScriptableObject (La Carte dans l'éditeur)
[CreateAssetMenu(fileName = "New_Spell_Fumigene", menuName = "TowerDefense/Cards/Spells/Fumigene")]
public class SpellCard_Fumigene : SpellCardData
{
    [Header("Paramètres du Sort")]
    public float radius = 8f;
    public float slowDuration = 6f;

    public override ISpellEffect CreateSpellEffect()
    {
        return new SpellLogic_Fumigene(radius, slowDuration);
    }
}