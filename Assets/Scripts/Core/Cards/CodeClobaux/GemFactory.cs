using UnityEngine;


public static class GemFactory
{
    public static IGemEffect CreateGemLogic(GemLogicType type)
    {
        switch (type)
        {
            case GemLogicType.AttaqueRapide:
                return new Gem_AttaqueRapide();
            case GemLogicType.NoyauIgnifere:
                return new Gem_NoyauIgnifere();
            
            // Insérer les nvx 
            
            
            
            
            
            
            
            
            
            
            default:
                Debug.LogError($"[GemFactory] Type de gemme inconnu : {type}");
                return null;
        }
    }
}