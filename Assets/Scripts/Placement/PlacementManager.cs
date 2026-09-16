using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlacementManager : MonoBehaviour  
{  
    public static PlacementManager Instance { get; private set; } 
    
    public GridData GridData { get; private set; }
    public IPathfinder Pathfinder { get; private set; }
    public GridVisualizer GridVisualizer { get; private set; }

    private Dictionary<Vector2Int, GameObject> placedTowers = new Dictionary<Vector2Int, GameObject>();
    
    public CardInstance ActiveCard { get; private set; }
    public CardData debugCardToTest; 
    
    private bool isProcessingCard = false;
    private bool peutPlacer = true;


    void Awake()  
    {  
        if (Instance == null) Instance = this;       
        else Destroy(gameObject);
        
        GridData = GetComponent<GridData>();  
        Pathfinder = GetComponent<IPathfinder>();  
        GridVisualizer = GetComponent<GridVisualizer>();
    }

    void OnEnable() 
    { 
        GameEvents.OnGameStateChanged += GererChangementDePhase;
        GameEvents.OnCardSelected += SelectCard;
    }
    
    void OnDisable() 
    { 
        GameEvents.OnGameStateChanged -= GererChangementDePhase;
        GameEvents.OnCardSelected -= SelectCard;
    }

    private void GererChangementDePhase(GameState newState)
    {
        peutPlacer = true;
        DeselectCard();
    }

    private void SelectCard(CardInstance card)
    {
        ActiveCard = card;
        Debug.Log($"[PlacementManager] Carte sélectionnée : {card.Data.nomCarte}");
    }

    public void DeselectCard()
    {
        ActiveCard = null;
        GameEvents.TriggerCardDeselected(); 
    }

     public async void TryPlayCardAt(Vector2Int targetCoords)  
    {  
        if (!peutPlacer || ActiveCard == null || isProcessingCard) return;
        if (LevelManager.Instance != null && LevelManager.Instance.IsStartOrEnd(targetCoords)) return; 

        // RÈGLES DE PHASES 
        GameState currentState = GameManager.Instance.CurrentState;
        
        if (currentState == GameState.PreparationPhase && ActiveCard.Data is SpellCardData)
        {
            Debug.LogWarning("Les sorts doivent être glissés dans la Zone d'Armement");
            return;
        }
        
        if (currentState == GameState.ActionPhase && !(ActiveCard.Data is SpellCardData))
        {
            Debug.LogWarning("Impossible de construire pendant le combat !");
            return;
        }
        // -------------------------

        isProcessingCard = true; 

        if (await ActiveCard.Data.TryPlayCardAsync(targetCoords, this))
        {
            if (currentState == GameState.PreparationPhase)
            {
                // Logique normale pour les tours
                bool keepInHand = false;
                if (ActiveCard.Data is StructureCardData structureCard && structureCard.isMultiCast && ActiveCard.CurrentCharges > 1)
                {
                    keepInHand = true;
                    ActiveCard.CurrentCharges--; 
                    GameEvents.TriggerCardChargesUpdated(ActiveCard);
                }

                if (!keepInHand)
                {
                    DeckManager.Instance.UseCardAndDiscard(ActiveCard);
                    DeselectCard();
                }
            }
            else if (currentState == GameState.ActionPhase)
            {
                // En phase d'action, le sort a fait effet, on le supprime des sorts armés
                DeckManager.Instance.ConsumeArmedSpell(ActiveCard.Data);
                DeselectCard();
                
                FindObjectOfType<HandUIManager>().RefreshActionHand();
            }
        }
        
        isProcessingCard = false; 
    }

    public Vector3 GetWorldPosition(Vector2Int coords)
    {
        float worldX = transform.position.x + (coords.x * GridData.cellSize) + (GridData.cellSize / 2f);
        float worldZ = transform.position.z + (coords.y * GridData.cellSize) + (GridData.cellSize / 2f);
        return new Vector3(worldX, transform.position.y, worldZ);
    }

    public bool IsEnemyOnCell(Vector3 cellCenter)
    {
        Vector3 halfExtents = new Vector3(GridData.cellSize * 0.45f, 0.5f, GridData.cellSize * 0.45f); 
        return Physics.CheckBox(cellCenter, halfExtents, Quaternion.identity, enemyLayer);
    }

    public LayerMask enemyLayer; // Gardé pour IsEnemyOnCell

    public bool TryGetTowerAt(Vector2Int coords, out GameObject tower)
    {
        return placedTowers.TryGetValue(coords, out tower);
    }

    public void AddTower(Vector2Int coords, GameObject tower)
    {
        placedTowers[coords] = tower;
    }

    public void RemoveTower(Vector2Int coords)
    {
        placedTowers.Remove(coords);
    }
}