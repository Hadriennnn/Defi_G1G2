using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class HandUIManager : MonoBehaviour
{
    [Header("Configuration UI")]
    public GameObject cardUIPrefab; 
    public Transform handContainer; 
    
    public TextMeshProUGUI mulliganButtonText; 

    private bool isMulliganMode = false;
    private List<CardInstance> cardsSelectedForMulligan = new List<CardInstance>();
    private Dictionary<CardInstance, GameObject> visualCards = new Dictionary<CardInstance, GameObject>();
    
    private Dictionary<CardInstance, Color> originalColors = new Dictionary<CardInstance, Color>();

    private CardInstance currentlySelectedCard = null;

    public void Init()
    {
        DeckManager.Instance.OnCardDrawn += AddCardVisual;
        DeckManager.Instance.OnCardDiscarded += RemoveCardVisual;
        
        GameEvents.OnMulliganModeToggled += ToggleMulliganMode;
        GameEvents.OnCardSelected += HandleCardSelected;
        GameEvents.OnCardDeselected += HandleCardDeselected;

        foreach (CardInstance card in DeckManager.Instance.GetHand())
        {
            AddCardVisual(card);
        }
    }

    void OnDestroy()
    {
        if (DeckManager.Instance != null)
        {
            DeckManager.Instance.OnCardDrawn -= AddCardVisual;
            DeckManager.Instance.OnCardDiscarded -= RemoveCardVisual;
        }
        GameEvents.OnMulliganModeToggled -= ToggleMulliganMode;
        GameEvents.OnCardSelected -= HandleCardSelected;
        GameEvents.OnCardDeselected -= HandleCardDeselected;
    }

    private void ToggleMulliganMode(bool enabled)
    {
        isMulliganMode = enabled;
        cardsSelectedForMulligan.Clear();
        
        if (enabled && currentlySelectedCard != null)
        {
            PlacementManager.Instance.DeselectCard();
        }
        
        ResetAllCardColors();

        if (mulliganButtonText != null)
        {
            mulliganButtonText.text = enabled ? "Annuler" : "Mulligan";
        }
    }

    private void HandleCardSelected(CardInstance card)
    {
        ResetAllCardColors(); 
        currentlySelectedCard = card;

        if (visualCards.TryGetValue(card, out GameObject cardObj))
        {
            // On teinte la carte en vert clair pour montrer qu'elle est sélectionnée
            cardObj.GetComponent<Image>().color = new Color(0.7f, 1f, 0.7f); 
        }
    }

    private void HandleCardDeselected()
    {
        currentlySelectedCard = null;
        if (!isMulliganMode) ResetAllCardColors();
    }

    private void ResetAllCardColors()
    {
        foreach (var kvp in visualCards)
        {
            CardInstance card = kvp.Key;
            GameObject cardObj = kvp.Value;

            // On restaure TA couleur d'origine
            if (originalColors.TryGetValue(card, out Color baseColor))
            {
                cardObj.GetComponent<Image>().color = baseColor;
            }
        }
    }

    public void OnCardClicked(CardInstance cardClicked)
    {
        if (isMulliganMode)
        {
            if (cardsSelectedForMulligan.Contains(cardClicked))
            {
                cardsSelectedForMulligan.Remove(cardClicked);
                visualCards[cardClicked].GetComponent<Image>().color = originalColors[cardClicked]; 
            }
            else
            {
                cardsSelectedForMulligan.Add(cardClicked);
                visualCards[cardClicked].GetComponent<Image>().color = Color.gray; // Gris pour marquer le Mulligan
            }
        }
        else
        {
            GameEvents.TriggerCardSelected(cardClicked);
        }
    }

    public void ConfirmMulligan()
    {
        if (isMulliganMode && cardsSelectedForMulligan.Count > 0)
        {
            if (DeckManager.Instance.TryMulligan(cardsSelectedForMulligan))
            {
                ToggleMulliganMode(false); 
            }
        }
    }

    public void OnMulliganButtonClicked()
    {
        GameEvents.TriggerMulliganModeToggled(!isMulliganMode);
    }

    private void AddCardVisual(CardInstance data)
    {
        GameObject newCard = Instantiate(cardUIPrefab, handContainer);
        CardUI cardScript = newCard.GetComponent<CardUI>();
        
        if (cardScript != null)
        {
            cardScript.Setup(data, this); 
        }

        Image cardImage = newCard.GetComponent<Image>();
        if (cardImage != null)
        {
            originalColors.Add(data, cardImage.color);
        }
        else
        {
            originalColors.Add(data, Color.white); // Sécurité au cas où il n'y a pas d'Image
        }
        
        visualCards.Add(data, newCard);
    }

    private void RemoveCardVisual(CardInstance data)
    {
        if (visualCards.TryGetValue(data, out GameObject cardObj))
        {
            cardObj.transform.SetParent(null);
            Destroy(cardObj);
            
            visualCards.Remove(data);
            originalColors.Remove(data); // On nettoie la mémoire
        }
    }
}