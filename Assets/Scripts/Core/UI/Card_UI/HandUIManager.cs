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

    void Start()
    {
        DeckManager.Instance.OnCardDrawn += AddCardVisual;
        DeckManager.Instance.OnCardDiscarded += RemoveCardVisual;
        GameEvents.OnMulliganModeToggled += ToggleMulliganMode;

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
    }

    private void ToggleMulliganMode(bool enabled)
    {
        isMulliganMode = enabled;
        cardsSelectedForMulligan.Clear();
        
        foreach (var kvp in visualCards)
        {
            kvp.Value.GetComponent<Image>().color = Color.white;
        }

        if (!enabled && mulliganButtonText != null)
        {
            mulliganButtonText.text = "Mulligan";
        }
    }

    public void OnCardClicked(CardInstance cardClicked)
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
            GameEvents.TriggerMulliganModeToggled(true);
            if (mulliganButtonText != null) mulliganButtonText.text = "Annuler";
        }
        else
        {
            GameEvents.TriggerMulliganModeToggled(false);
        }
    }


    private void AddCardVisual(CardInstance data)
    {
        GameObject newCard = Instantiate(cardUIPrefab, handContainer);
        CardUI cardScript = newCard.GetComponent<CardUI>();
        
        if (cardScript != null)
        {
            cardScript.Setup(data, this); 
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
        }
    }
}