using UnityEngine;

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
    public float radius = 2f;
    public float freezeDuration = 2f;
    
    [Header("Visuel")]
    public GameObject vfxPrefab; 

    public override ISpellEffect CreateSpellEffect()
    {
        return new SpellLogic_OndeDeStase(radius, freezeDuration, vfxPrefab);
    }
}