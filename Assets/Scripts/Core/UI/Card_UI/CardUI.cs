using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using UnityEngine.EventSystems; 

public class CardUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Composants Visuels")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI descriptionText;
    public Image cardIcon; 
    public TextMeshProUGUI chargesText;
    
    public CardInstance CardInstanceData { get; private set; }
   
    private HandUIManager myManager;

    // Variables pour le Drag & Drop
    private Transform originalParent;
    private int originalSiblingIndex;
    private CanvasGroup canvasGroup; // Permet de rendre la carte transparente à la souris pendant le glissement

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    void OnEnable() { GameEvents.OnCardChargesUpdated += HandleChargesUpdate; }
    void OnDisable() { GameEvents.OnCardChargesUpdated -= HandleChargesUpdate; }

    public void Setup(CardInstance instance, HandUIManager manager)
    {
        CardInstanceData = instance;
        myManager = manager;
        
        if (nameText != null) nameText.text = instance.Data.nomCarte;
        if (costText != null) costText.text = instance.Data.cost.ToString();
        if (descriptionText != null) descriptionText.text = instance.Data.description;
        if (cardIcon != null && instance.Data.icon != null) cardIcon.sprite = instance.Data.icon;

        RefreshChargeText();
    }

    private void HandleChargesUpdate(CardInstance updatedCard)
    {
        if (updatedCard == CardInstanceData) RefreshChargeText();
    }

    private void RefreshChargeText()
    {
        if (chargesText != null && CardInstanceData != null)
            chargesText.text = CardInstanceData.CurrentCharges.ToString();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (CardInstanceData == null) return;
        myManager.OnCardClicked(CardInstanceData);
    }

    // LOGIQUE DE DRAG & DROP 

    public void OnBeginDrag(PointerEventData eventData)
    {
        // On sauvegarde sa place dans la main
        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();

        // On le sort du Layout de la main pour qu'il soit libre et on le met tout devant
        transform.SetParent(transform.root); 
        transform.SetAsLastSibling();

        //On désactive les raycasts 
        canvasGroup.blocksRaycasts = false; 
    }

    public void OnDrag(PointerEventData eventData)
    {
        // La carte suit la souris
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // On réactive les raycasts
        canvasGroup.blocksRaycasts = true;

        // Si la carte n'a pas changé de parent 
        if (transform.parent == transform.root)
        {
            // On la remet à sa place dans la main
            transform.SetParent(originalParent);
            transform.SetSiblingIndex(originalSiblingIndex);
        }
    }
}