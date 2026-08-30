using UnityEngine;

public class TowerVisuals : MonoBehaviour
{
    [Header("Matériaux de la Tour (Mat_Gem)")]
    [Tooltip("Le MeshRenderer principal de la tour qui contient le Mat_Gem")]
    public MeshRenderer gemRenderer;
    [Tooltip("L'index (0, 1, 2...) du Mat_Gem dans la liste des matériaux de la tour")]
    public int gemMaterialIndex = 1;
    [ColorUsage(true, true)] 
    public Color colorOff = Color.white;

    [Header("Objet Actif (Le Cristal / Cylindre)")]
    [Tooltip("Le GameObject qui apparaît quand la tour est allumée")]
    public GameObject activeIndicator;
    [Tooltip("Le MeshRenderer de cet Active Indicator (Optionnel, permet de le colorer)")]
    public MeshRenderer indicatorRenderer;
    [Tooltip("L'index du matériau à colorer sur l'indicateur (0 par défaut)")]
    public int indicatorMaterialIndex = 0;

    private void Start()
    {
        TurnOff();
    }

    public void TurnOn(ElementType elementType)
    {
        Color glowColor = GetColorForElement(elementType);

        //On allume l'objet 
        if (activeIndicator != null) activeIndicator.SetActive(true);

        //On colore le matériau de la tour
        if (gemRenderer != null && gemRenderer.materials.Length > gemMaterialIndex)
        {
            Material[] mats = gemRenderer.materials;
            mats[gemMaterialIndex].color = glowColor;
            mats[gemMaterialIndex].SetColor("_EmissionColor", glowColor * 2f); 
            gemRenderer.materials = mats;
        }

        //n colore aussi l'indicateur s'il a un Renderer 
        if (indicatorRenderer != null && indicatorRenderer.materials.Length > indicatorMaterialIndex)
        {
            Material[] indMats = indicatorRenderer.materials;
            indMats[indicatorMaterialIndex].color = glowColor;
            
            // On met une forte émission pour que le cristal brille bien !
            indMats[indicatorMaterialIndex].SetColor("_EmissionColor", glowColor * 3f); 
            indicatorRenderer.materials = indMats;
        }
    }

    public void TurnOff()
    {
        //On éteint l'objet indicateur
        if (activeIndicator != null) activeIndicator.SetActive(false);

        //On remet la couleur "Off" sur la tour
        if (gemRenderer != null && gemRenderer.materials.Length > gemMaterialIndex)
        {
            Material[] mats = gemRenderer.materials;
            mats[gemMaterialIndex].color = colorOff;
            mats[gemMaterialIndex].SetColor("_EmissionColor", colorOff * 0.1f); 
            gemRenderer.materials = mats;
        }
    }

    private Color GetColorForElement(ElementType element)
    {
        switch (element)
        {
            case ElementType.Normal: return Color.white;
            case ElementType.Feu: return new Color(1f, 0.4f, 0f); // Orange/Rouge
            case ElementType.Glace: return Color.cyan;
            case ElementType.Foudre: return Color.yellow;
            default: return Color.white;
        }
    }
}