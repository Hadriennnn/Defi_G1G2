using UnityEngine;

public class HandUIManager : MonoBehaviour
{
    [Header("Configuration UI")]
    public GameObject cardUIPrefab; 
    public Transform handContainer; 

    void Start()
    {
        DeckManager.Instance.OnCardDrawn += AddCardVisual;
        DeckManager.Instance.OnCardDiscarded += RemoveCardVisual;

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
    }

    private void AddCardVisual(CardData data)
    {
        GameObject newCard = Instantiate(cardUIPrefab, handContainer);
        CardUI cardScript = newCard.GetComponent<CardUI>();
        if (cardScript != null)
        {
            cardScript.Setup(data);
        }
    }

    private void RemoveCardVisual(CardData data)
    {
        foreach (Transform child in handContainer)
        {
            CardUI cardUI = child.GetComponent<CardUI>();
            
            if (cardUI != null && cardUI.CardDataAsset == data)
            {

                child.SetParent(null); 
                
                Destroy(child.gameObject);
                break; 
            }
        }
    }
}