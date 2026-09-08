using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct ColorablePart
{
    public MeshRenderer renderer;
    [Tooltip("L'index (0, 1, 2...) du matériau à colorer sur cet objet")]
    public int materialIndex;
}

public class TowerVisuals : MonoBehaviour
{
    [Header("Éléments à colorer")]
    [Tooltip("Ajoute ici tous les objets (Base, Tourelle, Cylindre) qui doivent changer de couleur.")]
    public List<ColorablePart> partsToColor;

    [ColorUsage(true, true)] 
    public Color colorOff = Color.white;

    [Header("Objet Actif (Le Cristal / Cylindre)")]
    [Tooltip("Le GameObject (l'objet complet) qui doit apparaître/disparaître quand la tour s'allume")]
    public GameObject activeIndicator;

    private void Start()
    {
        TurnOff();
    }

    public void TurnOn(ElementType elementType)
    {
        Color glowColor = GetColorForElement(elementType);

        if (activeIndicator != null) activeIndicator.SetActive(true);

        foreach (ColorablePart part in partsToColor)
        {
            if (part.renderer != null && part.renderer.materials.Length > part.materialIndex)
            {
                Material[] mats = part.renderer.materials;
                mats[part.materialIndex].color = glowColor;
                
                mats[part.materialIndex].SetColor("_EmissionColor", glowColor * 3f); 
                
                part.renderer.materials = mats;
            }
        }
    }

    public void TurnOff()
    {
        if (activeIndicator != null) activeIndicator.SetActive(false);

        foreach (ColorablePart part in partsToColor)
        {
            if (part.renderer != null && part.renderer.materials.Length > part.materialIndex)
            {
                Material[] mats = part.renderer.materials;
                mats[part.materialIndex].color = colorOff;
                
                // On éteint l'émission
                mats[part.materialIndex].SetColor("_EmissionColor", colorOff * 0.1f); 
                
                part.renderer.materials = mats;
            }
        }
    }

    private Color GetColorForElement(ElementType element)
    {
        switch (element)
        {
            case ElementType.Normal: return Color.white;
            case ElementType.Feu: return new Color(1f, 0.4f, 0f); 
            case ElementType.Glace: return Color.cyan;
            case ElementType.Foudre: return Color.yellow;
            default: return Color.white;
        }
    }
}