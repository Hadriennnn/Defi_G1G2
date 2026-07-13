using UnityEngine;

// Le colis qui voyagera dans le projectile. 
public struct HitPayload
{
    public float rawDamage;
    // Plus tard, on ajoutera ici : public StatusEffectData[] statusEffects;
}

// Le contrat : tout élément du jeu qui peut subir des dégâts doit implémenter ceci.
public interface IStatusReceiver
{
    void ReceivePayload(HitPayload payload);
}