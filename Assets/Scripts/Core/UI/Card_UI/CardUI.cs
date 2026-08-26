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
    
    public CardData CardDataAsset { get; private set; }
   
    private CardData myData;

    public void Setup(CardData data)
    {
        myData = data;
        CardDataAsset = data;
        
        if (nameText != null) nameText.text = data.nomCarte;
        if (costText != null) costText.text = data.cost.ToString();
        if (descriptionText != null) descriptionText.text = data.description;
        
        if (cardIcon != null && data.icon != null) 
        {
            cardIcon.sprite = data.icon;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (myData == null) return;
        GameEvents.TriggerCardSelected(myData);
    }
}