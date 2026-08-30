using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class HandUIManager : MonoBehaviour
{
    [Header("Configuration UI")]
    public GameObject cardUIPrefab; 
    public Transform handContainer; 
    
    [Tooltip("Le texte enfant du bouton Mulligan")]
    public TextMeshProUGUI mulliganButtonText; 

    //Logique de Mulligan
    private bool isMulliganMode = false;
    private List<CardData> cardsSelectedForMulligan = new List<CardData>();
    private Dictionary<CardData, GameObject> visualCards = new Dictionary<CardData, GameObject>();

    void Start()
    {
        DeckManager.Instance.OnCardDrawn += AddCardVisual;
        DeckManager.Instance.OnCardDiscarded += RemoveCardVisual;
        GameEvents.OnMulliganModeToggled += ToggleMulliganMode;

        foreach (CardData card in DeckManager.Instance.GetHand())
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
    }

    private void ToggleMulliganMode(bool enabled)
    {
        isMulliganMode = enabled;
        cardsSelectedForMulligan.Clear();
        
        // Reset l'aspect visuel de toutes les cartes
        foreach (var kvp in visualCards)
        {
            kvp.Value.GetComponent<Image>().color = Color.white;
        }

        if (!enabled && mulliganButtonText != null)
        {
            mulliganButtonText.text = "Mulligan";
        }
    }

    public void OnCardClicked(CardData cardClicked)
    {
        if (isMulliganMode)
        {
            if (cardsSelectedForMulligan.Contains(cardClicked))
            {
                cardsSelectedForMulligan.Remove(cardClicked);
                visualCards[cardClicked].GetComponent<Image>().color = Color.white; 
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
        if (!isMulliganMode)
        {
            // On ACTIVE le mode Mulligan
            GameEvents.TriggerMulliganModeToggled(true);
            if (mulliganButtonText != null) mulliganButtonText.text = "Annuler";
        }
        else
        {
            // On ANNULE le mode Mulligan
            GameEvents.TriggerMulliganModeToggled(false);
        }
    }


    private void AddCardVisual(CardData data)
    {
        GameObject newCard = Instantiate(cardUIPrefab, handContainer);
        CardUI cardScript = newCard.GetComponent<CardUI>();
        
        if (cardScript != null)
        {
            cardScript.Setup(data, this); 
        }
        
        visualCards.Add(data, newCard);
    }

    private void RemoveCardVisual(CardData data)
    {
        if (visualCards.TryGetValue(data, out GameObject cardObj))
        {
            cardObj.transform.SetParent(null);
            Destroy(cardObj);
            visualCards.Remove(data);
        }
    }
}