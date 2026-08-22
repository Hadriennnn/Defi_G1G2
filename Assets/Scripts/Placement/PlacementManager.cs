using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlacementManager : MonoBehaviour  
{  
    public static PlacementManager Instance { get; private set; } 
    
    private GridData gridData;  
    private IPathfinder pathfinder; 
    private GridVisualizer gridVisualizer;

    [Header("Configuration du Survol (Hover 3D)")]  
    [Tooltip("Le curseur discret affiché en temps normal.")]
    public GameObject defaultHoverIndicator; 
    [Tooltip("Le curseur bien visible affiché quand une carte est tenue.")]
    public GameObject cardHoverIndicator; 
    public LayerMask groundLayer; 
   
    private Dictionary<Vector2Int, GameObject> placedTowers = new Dictionary<Vector2Int, GameObject>();
    private Vector2Int derniereCaseSurvolee = new Vector2Int(-1, -1);
    private bool peutPlacer = true;

    // --- LE LIEN AVEC LE DECK ---
    private CardData activeCard = null;
    public CardData debugCardToTest; // A SUPPRIMER PLUS TARD

    void Awake()  
    {  
        if (Instance == null) Instance = this;       
        else Destroy(gameObject);
        
        gridData = GetComponent<GridData>();  
        pathfinder = GetComponent<IPathfinder>();  
        gridVisualizer = GetComponent<GridVisualizer>();
       
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

    private void SelectCard(CardData card)
    {
        activeCard = card;
        Debug.Log($"[PlacementManager] Carte sélectionnée : {card.nomCarte} (Coût: {card.cost})");
    }

    private void DeselectCard()
    {
        activeCard = null;
        GameEvents.TriggerCardDeselected(); // On prévient le jeu (utile si l'UI veut retirer une surbrillance plus tard)
    }

    void Update()
    {
        // 1. TEST DEBUG
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) 
        {
            SelectCard(debugCardToTest);
        }

        // 2. Si on est sur l'UI, on masque le Hover et on bloque tout
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            MasquerHover();
            return; 
        }

        // 3. On affiche toujours un Hover sur la grille (il choisira le bon tout seul)
        GererHover();

        // 4. Si on ne peut pas placer ou qu'aucune carte n'est tenue, on arrête ici
        if (!peutPlacer || activeCard == null) 
        {
            return;
        }

        // 5. Clic Gauche : Tenter de jouer la carte
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)  
        {  
            if (derniereCaseSurvolee.x != -1) TryPlayActiveCard();  
        }
        
        // 6. Clic Droit : Annuler la sélection de la carte
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)  
        {  
            DeselectCard();
        }
    }  

    // --- GESTION DU SURVOL (HOVER) ---
    void GererHover()  
    {  
        if (Mouse.current == null || Camera.main == null) return;  

        Vector2 mousePosition = Mouse.current.position.ReadValue();  
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
       
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayer))
        {
            Vector3 origineGrille = transform.position;
            float localX = hit.point.x - origineGrille.x;
            float localZ = hit.point.z - origineGrille.z; 
           
            int gridX = Mathf.FloorToInt(localX / gridData.cellSize);  
            int gridY = Mathf.FloorToInt(localZ / gridData.cellSize); 
           
            Vector2Int caseActuelle = new Vector2Int(gridX, gridY);  

            if (gridX >= 0 && gridX < gridData.largeur && gridY >= 0 && gridY < gridData.hauteur)  
            {  
                // On détermine quel curseur allumer et lequel éteindre
                GameObject hoverAActiver = (activeCard != null) ? cardHoverIndicator : defaultHoverIndicator;
                GameObject hoverADesactiver = (activeCard != null) ? defaultHoverIndicator : cardHoverIndicator;

                if (caseActuelle != derniereCaseSurvolee)  
                {  
                    derniereCaseSurvolee = caseActuelle;  
                }

                // On met à jour l'affichage
                if (hoverADesactiver != null) hoverADesactiver.SetActive(false);
                
                if (hoverAActiver != null)
                {
                    hoverAActiver.SetActive(true);
                    float hauteurPhysique = gridData.Grid[gridX, gridY].elevation * gridVisualizer.hauteurNiveau;
                    float worldX = origineGrille.x + (gridX * gridData.cellSize) + (gridData.cellSize / 2f);
                    float worldZ = origineGrille.z + (gridY * gridData.cellSize) + (gridData.cellSize / 2f);
                    hoverAActiver.transform.position = new Vector3(worldX, origineGrille.y + hauteurPhysique + 0.03f, worldZ);
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

    // JOUER LA CARTE 
    void TryPlayActiveCard()  
    {  
        int gridX = derniereCaseSurvolee.x;  
        int gridY = derniereCaseSurvolee.y;  
        Vector2Int inputCoords = new Vector2Int(gridX, gridY);
        Node clickedNode = gridData.Grid[gridX, gridY];  

        if (LevelManager.Instance != null && LevelManager.Instance.IsStartOrEnd(inputCoords)) return; 

        // CAS 1 : CARTE STRUCTURE (Tour / Barricade)
        if (activeCard is StructureCardData structureCard)
        {
            if (!clickedNode.isWalkable) 
            {
                Debug.LogWarning("Case déjà occupée");
                return;
            }

            // Pathfinding Check
            clickedNode.isWalkable = false;  
            if (!pathfinder.FindPath(LevelManager.Instance.startCoords, LevelManager.Instance.endCoords))  
            {  
                clickedNode.isWalkable = true;  
                pathfinder.FindPath(LevelManager.Instance.startCoords, LevelManager.Instance.endCoords);  
                Debug.LogWarning("Placement impossible : bloque la sortie !");
                return;  
            }

            // Dépense d'Énergie
            if (!EnergyManager.Instance.TrySpendEnergy(structureCard.cost))
            {
                clickedNode.isWalkable = true; 
                return;
            }

            // Placement Physique
            float hauteurPhysique = clickedNode.elevation * gridVisualizer.hauteurNiveau;
            Vector3 origineGrille = transform.position;
            float worldX = origineGrille.x + (gridX * gridData.cellSize) + (gridData.cellSize / 2f);
            float worldZ = origineGrille.z + (gridY * gridData.cellSize) + (gridData.cellSize / 2f);
            Vector3 spawnPos = new Vector3(worldX, origineGrille.y + hauteurPhysique, worldZ);

            GameObject nouvelleTour = Instantiate(structureCard.chassisPrefab, spawnPos, structureCard.chassisPrefab.transform.rotation);
            placedTowers.Add(inputCoords, nouvelleTour);

            GameEvents.TriggerObstacleToggled(new Vector3Int(gridX, gridY, 0), true);  
            GameEvents.TriggerGridChanged(); 
            
            // Défausse la carte jouée
            DeckManager.Instance.DiscardCard(activeCard);
            DeselectCard();
        }
        
        // CAS 2 : CARTE GEMME
        else if (activeCard is GemCardData gemCard)
        {
            if (!placedTowers.ContainsKey(inputCoords))
            {
                Debug.LogWarning("Il n'y a pas de tour sur cette case pour insérer une gemme");
                return;
            }

            GameObject targetTower = placedTowers[inputCoords];
            SocketManager sockets = targetTower.GetComponent<SocketManager>();

            if (sockets == null || !sockets.CanInsertGem())
            {
                Debug.LogWarning("Cette tour n'a plus de place");
                return;
            }

            if (!EnergyManager.Instance.TrySpendEnergy(gemCard.cost)) return;
            
            IGemEffect gemLogic = gemCard.CreateGemEffect();
            
            sockets.InsertGem(gemLogic);

            DeckManager.Instance.DiscardCard(activeCard);
            DeselectCard();
        }
        
        // CAS 3 : CARTE SORT (SPELL)
        else if (activeCard is SpellCardData spellCard)
        {
            // Si le sort nécessite une cible mais qu'on clique hors de la grille, on annule
            if (spellCard.requiresTargeting && !clickedNode.isWalkable && !placedTowers.ContainsKey(inputCoords)) 
            {
                // Note : Tu pourras affiner cette condition selon si tes sorts ciblent le chemin ou les tours
            }

            if (!EnergyManager.Instance.TrySpendEnergy(spellCard.cost)) return;

            // Calcul de la position 3D du clic
            Vector3 origineGrille = transform.position;
            float worldX = origineGrille.x + (gridX * gridData.cellSize) + (gridData.cellSize / 2f);
            float worldZ = origineGrille.z + (gridY * gridData.cellSize) + (gridData.cellSize / 2f);
            Vector3 targetPos = new Vector3(worldX, origineGrille.y, worldZ);

            // Polymorphisme : On exécute le sort !
            ISpellEffect spellLogic = spellCard.CreateSpellEffect();
            spellLogic.Execute(targetPos);

            DeckManager.Instance.DiscardCard(activeCard);
            DeselectCard();
        }

        // CAS 4 : CARTE MODIFICATEUR
        else if (activeCard is ModifierCardData modCard)
        {
            GameObject targetTower = null;

            if (modCard.requiresTowerTarget)
            {
                if (!placedTowers.ContainsKey(inputCoords))
                {
                    Debug.LogWarning("Ce modificateur nécessite de cibler une tour !");
                    return;
                }
                targetTower = placedTowers[inputCoords];
            }

            if (!EnergyManager.Instance.TrySpendEnergy(modCard.cost)) return;

            // Polymorphisme : On exécute le modificateur !
            IModifierEffect modLogic = modCard.CreateModifierEffect();
            modLogic.Execute(targetTower);

            // Si la tour a été détruite par le modificateur (ex: Recyclage), on la retire du dictionnaire
            if (targetTower == null || !targetTower.activeInHierarchy)
            {
                placedTowers.Remove(inputCoords);
                clickedNode.isWalkable = true;
                GameEvents.TriggerObstacleToggled(new Vector3Int(gridX, gridY, 0), false);
                GameEvents.TriggerGridChanged();
            }

            DeckManager.Instance.DiscardCard(activeCard);
            DeselectCard();
        }
        
    }  
}