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
    
    // NOUVEAU : Référence au bouton de validation de la vague
    [Tooltip("Le bouton qui permet de lancer la vague (Confirm / Start)")]
    public GameObject confirmButton; 

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
        
        GameEvents.OnGameStateChanged += HandleGameStateChanged;

        foreach (CardInstance card in DeckManager.Instance.GetHand()) AddCardVisual(card);
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
        GameEvents.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void HandleGameStateChanged(GameState state)
    {
        if (state == GameState.PreparationPhase)
        {
            // On affiche le bouton Mulligan ET le bouton Confirm
            if (mulliganButtonText != null) mulliganButtonText.transform.parent.gameObject.SetActive(true);
            if (confirmButton != null) confirmButton.SetActive(true);
            
            RefreshPrepHand();
        }
        else if (state == GameState.ActionPhase)
        {
            // On cache les deux boutons
            if (mulliganButtonText != null) mulliganButtonText.transform.parent.gameObject.SetActive(false);
            if (confirmButton != null) confirmButton.SetActive(false);
            
            RefreshActionHand();
        }
    }

    private void RefreshPrepHand()
    {
        List<CardInstance> toRemove = new List<CardInstance>(visualCards.Keys);
        foreach (var c in toRemove) RemoveCardVisual(c);

        foreach (CardInstance card in DeckManager.Instance.GetHand())
        {
            AddCardVisual(card);
        }
    }

    public void RefreshActionHand()
    {
        if (GameManager.Instance.CurrentState != GameState.ActionPhase) return;

        List<CardInstance> toRemove = new List<CardInstance>(visualCards.Keys);
        foreach (var c in toRemove) RemoveCardVisual(c);

        foreach (CardData spell in DeckManager.Instance.armedSpells)
        {
            CardInstance dummyInstance = new CardInstance(spell);
            dummyInstance.CurrentCharges = 1; 
            AddCardVisual(dummyInstance);
        }
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
                visualCards[cardClicked].GetComponent<Image>().color = Color.gray; 
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
            originalColors.Add(data, Color.white); 
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
            originalColors.Remove(data); 
        }
    }
}