using UnityEngine;

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
    public float radius = 2f;
    public float slowDuration = 3f;
    
    [Header("Visuel")]
    public GameObject vfxPrefab; 

    public override ISpellEffect CreateSpellEffect()
    {
        return new SpellLogic_Fumigene(radius, slowDuration, vfxPrefab);
    }
}