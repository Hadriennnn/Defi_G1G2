using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    private TextMeshPro textMesh;
    private PooledItem pooledItem; 

    [Header("Paramètres d'Animation")]
    public float lifetime = 1f;
    public float floatSpeed = 2f;
    
    [Tooltip("Dessine la courbe de taille du texte sur sa durée de vie (ex: petit -> gros -> normal)")]
    public AnimationCurve scaleCurve;

    private float currentLifeTime;
    private float baseScale; 
    private Color textColor;

    void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    public void Setup(float damageAmount, ElementType element, bool isCritical)
    {
        if (pooledItem == null)
        {
            pooledItem = GetComponent<PooledItem>();
        }

        textMesh.text = Mathf.RoundToInt(damageAmount).ToString();
        currentLifeTime = 0f;

        // 1. GESTION DES COULEURS
        switch (element)
        {
            case ElementType.Normal: textColor = Color.white; break;
            case ElementType.Feu: textColor = new Color(1f, 0.4f, 0f); break; 
            case ElementType.Glace: textColor = Color.cyan; break;
            case ElementType.Foudre: textColor = Color.yellow; break;
        }
        
        // 2. GESTION DE LA TAILLE DE BASE
        baseScale = Mathf.Clamp(1f + (damageAmount / 100f), 0.8f, 2.5f);
        
        if (isCritical)
        {
            baseScale *= 1.5f; 
            textColor = Color.red; 
            textMesh.text += "!";
        }

        textMesh.color = textColor;
        transform.localScale = Vector3.one * (baseScale * scaleCurve.Evaluate(0));
    }

    void Update()
    {
        currentLifeTime += Time.deltaTime;
        float lifePercent = currentLifeTime / lifetime;

        // Mouvement vers le haut
        transform.position += new Vector3(0, floatSpeed * Time.deltaTime, 0);
        
        // Fait face à la caméra
        transform.rotation = Camera.main.transform.rotation;

        // Animation de la taille selon la courbe
        float curveValue = scaleCurve.Evaluate(lifePercent);
        transform.localScale = Vector3.one * (baseScale * curveValue);

        // Fondu (Alpha) sur la fin de la vie
        if (lifePercent > 0.5f)
        {
            float fadeAmount = (lifePercent - 0.5f) * 2f; 
            textColor.a = 1f - fadeAmount;
            textMesh.color = textColor;
        }

        // Fin de vie : retour au Pool
        if (currentLifeTime >= lifetime)
        {
            // Sécurité supplémentaire au cas où
            if (pooledItem != null)
            {
                pooledItem.ReleaseToPool();
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}