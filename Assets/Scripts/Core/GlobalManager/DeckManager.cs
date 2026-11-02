using System;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance { get; private set; }

    [Header("Configuration du Deck")]
    public List<CardData> startingDeck; 
    public int maxHandSize = 6; 
    
    [Header("Coûts Mulligan")]
    public int mulliganEnergyCost = 1;

    private List<CardInstance> drawPile = new List<CardInstance>();
    private List<CardInstance> hand = new List<CardInstance>();
    private List<CardInstance> discardPile = new List<CardInstance>();
    private List<CardInstance> exhaustedPile = new List<CardInstance>(); 
    public List<CardData> armedSpells = new List<CardData>();   
    

    // UI
    public event Action<CardInstance> OnCardDrawn;
    public event Action<CardInstance> OnCardDiscarded;
    public event Action OnDeckReshuffled;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Init()
    {
        InitializeDeckForNewAct();
    }

    void OnEnable() { GameEvents.OnGameStateChanged += HandleGameStateChanged; }
    void OnDisable() { GameEvents.OnGameStateChanged -= HandleGameStateChanged; }

    private void HandleGameStateChanged(GameState state)
    {
        if (state == GameState.PreparationPhase)
        {
            hand.Clear(); 
            DrawHand();
        }
        else if (state == GameState.ActionPhase) DiscardEntireHand();
    }

    public void InitializeDeckForNewAct()
    {
        drawPile.Clear();
        hand.Clear();
        discardPile.Clear();
        exhaustedPile.Clear();
        armedSpells.Clear();

        foreach (CardData baseCard in startingDeck)
        {
            
            if (baseCard != null)
            {
                drawPile.Add(new CardInstance(baseCard));
            }
            else
            {
                Debug.LogWarning("[DeckManager] Attention : Une case vide a été trouvée dans le Starting Deck de l'inspecteur ! Elle a été ignorée.");
            }
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

        CardInstance drawnCard = drawPile[0];
        drawPile.RemoveAt(0);
        hand.Add(drawnCard);

        OnCardDrawn?.Invoke(drawnCard);
    }
    
    //  GESTION DES SORTS ARMÉS 

    public bool TryArmSpell(CardInstance card)
    {
        if (card.Data is SpellCardData && armedSpells.Count < 3) 
        {
            if (EnergyManager.Instance.TrySpendEnergy(card.Data.cost))
            {
                card.CurrentCharges--;
                hand.Remove(card);

                if (card.CurrentCharges <= 0) exhaustedPile.Add(card);
                else discardPile.Add(card);
                
                OnCardDiscarded?.Invoke(card); 
                armedSpells.Add(card.Data);
                
                // MODIFIÉ ICI
                GameEvents.TriggerArmedSpellsChanged(); 
                return true;
            }
        }
        return false;
    }


    public void ConsumeArmedSpell(CardData spellData)
    {
        if (armedSpells.Contains(spellData))
        {
            armedSpells.Remove(spellData);
        }
    }
    
    public void UnarmSpell(CardData spellData)
    {
        if (armedSpells.Contains(spellData) && hand.Count < maxHandSize)
        {
            armedSpells.Remove(spellData);
            
            CardInstance returnedCard = new CardInstance(spellData);
            hand.Add(returnedCard);
            OnCardDrawn?.Invoke(returnedCard); 
            
            GameEvents.TriggerArmedSpellsChanged(); 
        }
    }
    
    public void UseCardAndDiscard(CardInstance card)
    {
        if (!hand.Contains(card)) return;

        card.CurrentCharges--;
        Debug.Log($"[DeckManager] Carte jouée : {card.Data.nomCarte}. Charges restantes : {card.CurrentCharges}");

        hand.Remove(card);
        OnCardDiscarded?.Invoke(card); 

        if (card.CurrentCharges <= 0)
        {
            Debug.Log($"[DeckManager] {card.Data.nomCarte} ÉPUISÉE (Détruite pour l'Acte) !");
            exhaustedPile.Add(card);
        }
        else
        {
            discardPile.Add(card);
        }
    }

    public void DiscardEntireHand()
    {
        List<CardInstance> tempHand = new List<CardInstance>(hand);
        foreach (CardInstance card in tempHand)
        {
            hand.Remove(card);
            discardPile.Add(card); 
            OnCardDiscarded?.Invoke(card);
        }
        Debug.Log($"[DeckManager] Main défaussée. Défausse : {discardPile.Count}");
    }

    public bool TryMulligan(List<CardInstance> selectedCards)
    {
        if (selectedCards.Count == 0) return false;

        if (!EnergyManager.Instance.TrySpendEnergy(mulliganEnergyCost))
        {
            return false;
        }

        Debug.Log($"[DeckManager] Mulligan effectué pour {mulliganEnergyCost} énergie. {selectedCards.Count} cartes changées.");

        foreach (CardInstance card in selectedCards)
        {
            if (hand.Contains(card))
            {
                hand.Remove(card);
                discardPile.Add(card);
                OnCardDiscarded?.Invoke(card);
            }
        }

        for (int i = 0; i < selectedCards.Count; i++)
        {
            DrawSingleCard();
        }
        return true;
    }

    private void ReshuffleDiscardIntoDraw()
    {
        drawPile.AddRange(discardPile);
        discardPile.Clear();
        Shuffle(drawPile);
        OnDeckReshuffled?.Invoke();
    }

    private void Shuffle(List<CardInstance> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            CardInstance temp = list[i];
            int randomIndex = UnityEngine.Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
    
    public List<CardInstance> GetHand() => hand;
}