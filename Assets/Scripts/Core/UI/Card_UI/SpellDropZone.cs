using UnityEngine;
using UnityEngine.EventSystems;

public class SpellDropZone : MonoBehaviour, IDropHandler
{
    public Transform bannerContainer; 
    public GameObject spellBannerPrefab; 
    
    // NOUVEAU : Un CanvasGroup ajouté sur le même objet pour gérer la visibilité
    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    void OnEnable() 
    { 
        GameEvents.OnGameStateChanged += HandlePhase;
        GameEvents.OnArmedSpellsChanged += RefreshBanners; 
    }
    
    void OnDisable() 
    { 
        GameEvents.OnGameStateChanged -= HandlePhase; 
        GameEvents.OnArmedSpellsChanged -= RefreshBanners;
    }

    private void HandlePhase(GameState state)
    {
        if (state == GameState.ActionPhase)
        {
            // On rend l'objet invisible et intouchable, mais le script tourne encore !
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
        else if (state == GameState.PreparationPhase)
        {
            // On le réaffiche
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            RefreshBanners(); 
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        CardUI droppedCard = eventData.pointerDrag.GetComponent<CardUI>();
        if (droppedCard != null && droppedCard.CardInstanceData.Data is SpellCardData)
        {
            if (DeckManager.Instance.TryArmSpell(droppedCard.CardInstanceData))
            {
                RefreshBanners();
            }
        }
    }

    private void RefreshBanners()
    {
        if (GameManager.Instance.CurrentState != GameState.PreparationPhase) return;

        // Nettoie les vieux bandeaux
        foreach (Transform child in bannerContainer) Destroy(child.gameObject);

        // Crée les nouveaux en se basant sur le DeckManager
        foreach (CardData spell in DeckManager.Instance.armedSpells)
        {
            GameObject newBanner = Instantiate(spellBannerPrefab, bannerContainer);
            ArmedSpellBanner bannerScript = newBanner.GetComponent<ArmedSpellBanner>();
            if (bannerScript != null) bannerScript.Setup(spell);
        }
    }
}