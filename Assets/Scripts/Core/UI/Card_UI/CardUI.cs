using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using UnityEngine.EventSystems; 

public class CardUI : MonoBehaviour, IPointerClickHandler
{
    [Header("Composants Visuels")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI descriptionText;
    public Image cardIcon; 
    
    [Header("Nouvelle UI")]
    public TextMeshProUGUI chargesText;
    
    public CardInstance CardInstanceData { get; private set; }
   
    private HandUIManager myManager;

    void OnEnable()
    {
        GameEvents.OnCardChargesUpdated += HandleChargesUpdate;
    }

    void OnDisable()
    {
        GameEvents.OnCardChargesUpdated -= HandleChargesUpdate;
    }

    public void Setup(CardInstance instance, HandUIManager manager)
    {
        CardInstanceData = instance;
        myManager = manager;
        
        if (nameText != null) nameText.text = instance.Data.nomCarte;
        if (costText != null) costText.text = instance.Data.cost.ToString();
        if (descriptionText != null) descriptionText.text = instance.Data.description;
        
        if (cardIcon != null && instance.Data.icon != null) 
        {
            cardIcon.sprite = instance.Data.icon;
        }

        RefreshChargeText();
    }

    private void HandleChargesUpdate(CardInstance updatedCard)
    {
        if (updatedCard == CardInstanceData)
        {
            RefreshChargeText();
        }
    }

    private void RefreshChargeText()
    {
        if (chargesText != null && CardInstanceData != null)
        {
            chargesText.text = CardInstanceData.CurrentCharges.ToString();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (CardInstanceData == null) return;
        myManager.OnCardClicked(CardInstanceData);
    }
}