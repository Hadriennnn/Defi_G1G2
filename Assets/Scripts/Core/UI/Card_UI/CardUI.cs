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
    public TextMeshProUGUI chargesText; // Le texte des charges
    
    public CardData CardDataAsset { get; private set; }
   
    private CardData myData;
    private HandUIManager myManager;

    void OnEnable()
    {
        // On écoute le changement de charges
        GameEvents.OnCardChargesUpdated += HandleChargesUpdate;
    }

    void OnDisable()
    {
        GameEvents.OnCardChargesUpdated -= HandleChargesUpdate;
    }

    public void Setup(CardData data, HandUIManager manager)
    {
        myData = data;
        CardDataAsset = data;
        myManager = manager;
        
        if (nameText != null) nameText.text = data.nomCarte;
        if (costText != null) costText.text = data.cost.ToString();
        if (descriptionText != null) descriptionText.text = data.description;
        
        if (cardIcon != null && data.icon != null) 
        {
            cardIcon.sprite = data.icon;
        }

        RefreshChargeText();
    }

    // Fonction appelée par l'événement global
    private void HandleChargesUpdate(CardData updatedCard)
    {
        if (updatedCard == myData)
        {
            RefreshChargeText();
        }
    }

    private void RefreshChargeText()
    {
        if (chargesText != null && myData != null)
        {
            // Affiche "1", ou "Charges: 1" selon tes préférences
            chargesText.text = myData.currentCharges.ToString();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (myData == null) return;
        myManager.OnCardClicked(myData);
    }
}