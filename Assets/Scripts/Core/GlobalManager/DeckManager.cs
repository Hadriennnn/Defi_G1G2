using System;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance { get; private set; }

    [Header("Configuration du Deck")]
    public List<CardData> startingDeck; 
    public int maxHandSize = 6; 
    
    [Header("Coûts (Balatro)")]
    public int mulliganEnergyCost = 1; // Coût de base pour défausser/repiocher

    // Les Piles de Cartes
    private List<CardData> drawPile = new List<CardData>();
    private List<CardData> hand = new List<CardData>();
    private List<CardData> discardPile = new List<CardData>();
    private List<CardData> exhaustedPile = new List<CardData>(); // Cartes détruites pour l'Acte
    private List<CardData> armedSpells = new List<CardData>();   // Sorts en attente

    // UI
    public event Action<CardData> OnCardDrawn;
    public event Action<CardData> OnCardDiscarded;
    public event Action OnDeckReshuffled;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        InitializeDeckForNewAct();
        DrawHand();
    }

    void OnEnable() { GameEvents.OnGameStateChanged += HandleGameStateChanged; }
    void OnDisable() { GameEvents.OnGameStateChanged -= HandleGameStateChanged; }

    private void HandleGameStateChanged(GameState state)
    {
        if (state == GameState.PreparationPhase)
        {
            DrawHand();
        }
        else if (state == GameState.ActionPhase)
        {
            DiscardEntireHand();
        }
    }

    // On clone les cartes pour ne pas détruire les assets de bases que j'ai fait 
    public void InitializeDeckForNewAct()
    {
        drawPile.Clear();
        hand.Clear();
        discardPile.Clear();
        exhaustedPile.Clear();
        armedSpells.Clear();

        foreach (CardData baseCard in startingDeck)
        {
            CardData clonedCard = Instantiate(baseCard); 
            clonedCard.ResetCard(); // Met currentCharges = maxCharges
            drawPile.Add(clonedCard);
        }
        
        Shuffle(drawPile);
    }

    public void DrawHand()
    {
        int cardsToDraw = maxHandSize - hand.Count;
        for (int i = 0; i < cardsToDraw; i++)
        {
            DrawSingleCard();
        }
        Debug.Log($"[DeckManager] Main piochée. Main: {hand.Count}, Pioche: {drawPile.Count}");
    }

    public void DrawSingleCard()
    {
        if (hand.Count >= maxHandSize) return;

        if (drawPile.Count == 0)
        {
            if (discardPile.Count == 0) return; 
            ReshuffleDiscardIntoDraw();
        }

        CardData drawnCard = drawPile[0];
        drawPile.RemoveAt(0);
        hand.Add(drawnCard);

        OnCardDrawn?.Invoke(drawnCard);
    }

    // CHARGES ICI 
    
    // Appelée par le PlacementManager quand une carte est jouée sur le plateau
    public void UseCardAndDiscard(CardData card)
    {
        if (!hand.Contains(card)) return;

        card.currentCharges--;
        Debug.Log($"[DeckManager] Carte jouée : {card.nomCarte}. Charges restantes : {card.currentCharges}");

        hand.Remove(card);
        OnCardDiscarded?.Invoke(card); // UI

        // exhaust ici 
        if (card.currentCharges <= 0)
        {
            Debug.Log($"[DeckManager] {card.nomCarte} ÉPUISÉE (Détruite pour l'Acte) !");
            exhaustedPile.Add(card);
        }
        else
        {
            discardPile.Add(card);
        }
    }

    // Appelée à la fin de la phase de prépa
    public void DiscardEntireHand()
    {
        List<CardData> tempHand = new List<CardData>(hand);
        foreach (CardData card in tempHand)
        {
            hand.Remove(card);
            discardPile.Add(card); // On défausse sans retirer de charge
            OnCardDiscarded?.Invoke(card);
        }
        Debug.Log($"[DeckManager] Main défaussée. Défausse : {discardPile.Count}");
    }

    // MULLIGAN
    
    public bool TryMulligan(List<CardData> selectedCards)
    {
        if (selectedCards.Count == 0) return false;

        // On vérifie si on a l'énergie
        if (!EnergyManager.Instance.TrySpendEnergy(mulliganEnergyCost))
        {
            return false;
        }

        Debug.Log($"[DeckManager] Mulligan effectué pour {mulliganEnergyCost} énergie. {selectedCards.Count} cartes changées.");

        // On défausse les cartes sélectionnées
        foreach (CardData card in selectedCards)
        {
            if (hand.Contains(card))
            {
                hand.Remove(card);
                discardPile.Add(card);
                OnCardDiscarded?.Invoke(card);
            }
        }

        // On repioche le même nombre
        for (int i = 0; i < selectedCards.Count; i++)
        {
            DrawSingleCard();
        }

        // Si je voudrais augmenté le coup à chaque fois 
        // mulliganEnergyCost++; 

        return true;
    }

    private void ReshuffleDiscardIntoDraw()
    {
        drawPile.AddRange(discardPile);
        discardPile.Clear();
        Shuffle(drawPile);
        OnDeckReshuffled?.Invoke();
    }

    private void Shuffle(List<CardData> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            CardData temp = list[i];
            int randomIndex = UnityEngine.Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
    
    public List<CardData> GetHand() => hand;
}