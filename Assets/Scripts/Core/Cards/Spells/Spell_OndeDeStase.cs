using UnityEngine;

// --- 1. ONDE DE STASE ---

public class SpellLogic_OndeDeStase : ISpellEffect
{
    private float aoeRadius;
    private float duration;
    private GameObject vfxPrefab; 

    public SpellLogic_OndeDeStase(float radius, float stunDuration, GameObject vfx)
    {
        aoeRadius = radius;
        duration = stunDuration;
        vfxPrefab = vfx;
    }

    public void Execute(Vector3 targetPosition)
    {
        Debug.Log($"[Sort] Onde de Stase explosée ! Rayon: {aoeRadius}");
        
        if (vfxPrefab != null)
        {
            GameObject vfx = PoolManager.Instance.Spawn(vfxPrefab, targetPosition, Quaternion.identity);
            ExplosionVFX vfxScript = vfx.GetComponent<ExplosionVFX>();
            // La taille du VFX s'adapte à la taille du sort 
            if (vfxScript != null) vfxScript.Setup(aoeRadius);
        }

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

[CreateAssetMenu(fileName = "New_Spell_OndeStase", menuName = "TowerDefense/Cards/Spells/Onde de Stase")]
public class SpellCard_OndeDeStase : SpellCardData
{
    [Header("Paramètres du Sort")]
    public float radius = 0.5f;
    public float freezeDuration = 2f;
    
    [Header("Visuel")]
    public GameObject vfxPrefab; 

    public override ISpellEffect CreateSpellEffect()
    {
        return new SpellLogic_OndeDeStase(radius, freezeDuration, vfxPrefab);
    }
}


// --- 2. FUMIGÈNE ---

public class SpellLogic_Fumigene : ISpellEffect
{
    private float aoeRadius;
    private float duration;
    private GameObject vfxPrefab; 

    public SpellLogic_Fumigene(float radius, float smokeDuration, GameObject vfx)
    {
        aoeRadius = radius;
        duration = smokeDuration;
        vfxPrefab = vfx;
    }

    public void Execute(Vector3 targetPosition)
    {
        Debug.Log($"[Sort] Fumigène lancé ! Rayon: {aoeRadius}");
        
        if (vfxPrefab != null)
        {
            GameObject vfx = PoolManager.Instance.Spawn(vfxPrefab, targetPosition, Quaternion.identity);
            ExplosionVFX vfxScript = vfx.GetComponent<ExplosionVFX>();
            if (vfxScript != null) vfxScript.Setup(aoeRadius);
        }

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

[CreateAssetMenu(fileName = "New_Spell_Fumigene", menuName = "TowerDefense/Cards/Spells/Fumigene")]
public class SpellCard_Fumigene : SpellCardData
{
    [Header("Paramètres du Sort")]
    public float radius = 1f;
    public float slowDuration = 2f;
    
    [Header("Visuel")]
    public GameObject vfxPrefab; 

    public override ISpellEffect CreateSpellEffect()
    {
        return new SpellLogic_Fumigene(radius, slowDuration, vfxPrefab);
    }
}