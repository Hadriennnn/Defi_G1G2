using System;
using System.Collections.Generic;
using UnityEngine;


public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance { get; private set; }

    [Header("Configuration du Deck")]
    [Tooltip("La liste des cartes (ScriptableObjects) avec lesquelles le joueur commence la partie.")]
    public List<CardData> startingDeck; 
    
    [Tooltip("La taille maximale de la main du joueur.")]
    public int maxHandSize = 6; 

    private List<CardData> drawPile = new List<CardData>();
    private List<CardData> hand = new List<CardData>();
    private List<CardData> discardPile = new List<CardData>();

    // --- HOOKS POUR L'UI ---
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
        InitializeDeck();
        
        DrawHand();
    }

    void OnEnable()
    {
        GameEvents.OnGameStateChanged += HandleGameStateChanged;
    }

    void OnDisable()
    {
        GameEvents.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void HandleGameStateChanged(GameState state)
    {
        if (state == GameState.PreparationPhase)
        {
            // Début du tour de construction on refait sa main 
            DrawHand();
        }
        else if (state == GameState.ActionPhase)
        {
            // Début du combat on défausse toutes les cartes qu'on n'a pas pu ou voulu jouer
            DiscardEntireHand();
        }
    }


    public void InitializeDeck()
    {
        drawPile.Clear();
        hand.Clear();
        discardPile.Clear();

        // On clone le deck de départ dans la pioche pour ne pas altérer le deck de base
        drawPile.AddRange(startingDeck);
        Shuffle(drawPile);
    }

    public void DrawHand()
    {
        int cardsToDraw = maxHandSize - hand.Count;
        for (int i = 0; i < cardsToDraw; i++)
        {
            DrawSingleCard();
        }
        Debug.Log($"[DeckManager] Nouvelle Main piochée. Cartes en main : {hand.Count}, Pioche : {drawPile.Count}");
    }

    public void DrawSingleCard()
    {
        if (hand.Count >= maxHandSize)
        {
            return;
        }

        // Si la pioche est vide on re mélange la défausse
        if (drawPile.Count == 0)
        {
            if (discardPile.Count == 0) return; 
            ReshuffleDiscardIntoDraw();
        }

        // On prend la première carte 
        CardData drawnCard = drawPile[0];
        drawPile.RemoveAt(0);
        hand.Add(drawnCard);

        // On prévient l'UI de dessiner cette carte à l'écran
        OnCardDrawn?.Invoke(drawnCard);
    }

    public void DiscardCard(CardData card)
    {
        if (hand.Contains(card))
        {
            hand.Remove(card);
            discardPile.Add(card);
            
            // On prévient l'UI de détruire le visuel de cette carte
            OnCardDiscarded?.Invoke(card);
        }
    }

    public void DiscardEntireHand()
    {
        // On crée une copie temporaire pour pouvoir supprimer des éléments dans la boucle
        List<CardData> tempHand = new List<CardData>(hand);
        foreach (CardData card in tempHand)
        {
            DiscardCard(card);
        }
        Debug.Log($"[DeckManager] Main défaussée. Défausse totale : {discardPile.Count}");
    }

    private void ReshuffleDiscardIntoDraw()
    {
        drawPile.AddRange(discardPile);
        discardPile.Clear();
        Shuffle(drawPile);
        OnDeckReshuffled?.Invoke();
        Debug.Log("Pioche vide La défausse a été mélangée");
    }

    // L'algorithme standard pour mélanger une liste (Fisher-Yates Shuffle) choppé sur Internet
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
    
    public List<CardData> GetHand()
    {
        return hand;
    }
    
}