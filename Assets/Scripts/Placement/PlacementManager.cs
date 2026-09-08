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

    [Header("Configuration du Survol (Hover 3D)")]  
    public GameObject defaultHoverIndicator; 
    public GameObject cardHoverIndicator; 
    public LayerMask groundLayer; 
    public LayerMask enemyLayer; 
   
    private Dictionary<Vector2Int, GameObject> placedTowers = new Dictionary<Vector2Int, GameObject>();
    private Vector2Int derniereCaseSurvolee = new Vector2Int(-1, -1);
    private bool peutPlacer = true;

    private CardInstance activeCard = null;
    public CardData debugCardToTest; 
    private bool isProcessingCard = false;


    void Awake()  
    {  
        if (Instance == null) Instance = this;       
        else Destroy(gameObject);
        
        GridData = GetComponent<GridData>();  
        Pathfinder = GetComponent<IPathfinder>();  
        GridVisualizer = GetComponent<GridVisualizer>();
       
        MasquerHover();
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
        if (newState == GameState.PreparationPhase) peutPlacer = true;
        else if (newState == GameState.ActionPhase)
        {
            peutPlacer = false;
            DeselectCard();
            MasquerHover();
        }
    }

    private void SelectCard(CardInstance card)
    {
        activeCard = card;
        Debug.Log($"[PlacementManager] Carte sélectionnée : {card.Data.nomCarte} (Coût: {card.Data.cost})");
    }

    private void DeselectCard()
    {
        activeCard = null;
        GameEvents.TriggerCardDeselected(); 
    }

    void Update()
    {
        // En mode debug, on simule la création d'une instance à la volée
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && debugCardToTest != null) 
        {
            SelectCard(new CardInstance(debugCardToTest));
        }

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            MasquerHover();
            return; 
        }

        GererHover();

        if (!peutPlacer || activeCard == null) return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)  
        {  
            if (derniereCaseSurvolee.x != -1) TryPlayActiveCard();  
        }
        
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)  
        {  
            DeselectCard();
        }
    }  

    void GererHover()  
    {  
        if (Mouse.current == null || Camera.main == null) return;  

        Vector2 mousePosition = Mouse.current.position.ReadValue();  
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
       
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayer))
        {
            float localX = hit.point.x - transform.position.x;
            float localZ = hit.point.z - transform.position.z; 
           
            int gridX = Mathf.FloorToInt(localX / GridData.cellSize);  
            int gridY = Mathf.FloorToInt(localZ / GridData.cellSize); 
           
            Vector2Int caseActuelle = new Vector2Int(gridX, gridY);  

            if (gridX >= 0 && gridX < GridData.largeur && gridY >= 0 && gridY < GridData.hauteur)  
            {  
                GameObject hoverAActiver = (activeCard != null) ? cardHoverIndicator : defaultHoverIndicator;
                GameObject hoverADesactiver = (activeCard != null) ? defaultHoverIndicator : cardHoverIndicator;

                if (caseActuelle != derniereCaseSurvolee)  
                {  
                    derniereCaseSurvolee = caseActuelle;  
                }

                if (hoverADesactiver != null) hoverADesactiver.SetActive(false);
                
                if (hoverAActiver != null)
                {
                    hoverAActiver.SetActive(true);
                    float hauteurPhysique = GridData.Grid[gridX, gridY].elevation * GridVisualizer.hauteurNiveau;
                    Vector3 worldPos = GetWorldPosition(caseActuelle);
                    hoverAActiver.transform.position = new Vector3(worldPos.x, worldPos.y + hauteurPhysique + 0.03f, worldPos.z);
                }
            }
            else MasquerHover();
        }
        else MasquerHover();
    }  
   
    void MasquerHover()
    {
        if (defaultHoverIndicator != null) defaultHoverIndicator.SetActive(false);
        if (cardHoverIndicator != null) cardHoverIndicator.SetActive(false);
        derniereCaseSurvolee = new Vector2Int(-1, -1);
    }

    async void TryPlayActiveCard()  
    {  
        if (isProcessingCard) return;

        Vector2Int inputCoords = derniereCaseSurvolee;
        if (LevelManager.Instance != null && LevelManager.Instance.IsStartOrEnd(inputCoords)) return; 

        isProcessingCard = true; // On verrouille l'action

        // ON ATTEND LE RÉSULTAT ASYNCHRONE
        if (await activeCard.Data.TryPlayCardAsync(inputCoords, this))
        {
            bool keepInHand = false;

            if (activeCard.Data is StructureCardData structureCard)
            {
                if (structureCard.isMultiCast && activeCard.CurrentCharges > 1)
                {
                    keepInHand = true;
                    activeCard.CurrentCharges--; 
                    GameEvents.TriggerCardChargesUpdated(activeCard);
                }
            }

            if (!keepInHand)
            {
                DeckManager.Instance.UseCardAndDiscard(activeCard);
                DeselectCard();
            }
        }
        
        isProcessingCard = false; // On déverrouille
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