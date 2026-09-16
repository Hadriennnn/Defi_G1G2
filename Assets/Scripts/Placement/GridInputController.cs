using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class GridInputController : MonoBehaviour
{
    [Header("Configuration du Survol (Hover 3D)")]  
    public GameObject defaultHoverIndicator; 
    public GameObject cardHoverIndicator; 
    public LayerMask groundLayer; 

    private Vector2Int derniereCaseSurvolee = new Vector2Int(-1, -1);
    private PlacementManager placementManager;

    void Start()
    {
        // On récupère la référence du cerveau logique
        placementManager = PlacementManager.Instance;
        MasquerHover();
    }

    void Update()
    {
        if (placementManager == null) return;

        // DEBUG KEY 
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && placementManager.debugCardToTest != null) 
        {
            // Juste pour tes tests, on simule l'activation de la carte debug
            GameEvents.TriggerCardSelected(new CardInstance(placementManager.debugCardToTest));
        }

        // Si on a la souris sur l'interface UI (les cartes, les boutons), on masque l'hologramme 3D
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            MasquerHover();
            return; 
        }

        GererHover();

        // Si le joueur ne tient aucune carte, on s'arrête là (pas de clic pour jouer)
        if (placementManager.ActiveCard == null) return;

        // LIC GAUCHE 
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)  
        {  
            if (derniereCaseSurvolee.x != -1) 
            {
                placementManager.TryPlayCardAt(derniereCaseSurvolee);
            }
        }
        
        //CLIC DROIT : Annuler la sélection 
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)  
        {  
            placementManager.DeselectCard();
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
           
            int gridX = Mathf.FloorToInt(localX / placementManager.GridData.cellSize);  
            int gridY = Mathf.FloorToInt(localZ / placementManager.GridData.cellSize); 
           
            Vector2Int caseActuelle = new Vector2Int(gridX, gridY);  

            if (gridX >= 0 && gridX < placementManager.GridData.largeur && gridY >= 0 && gridY < placementManager.GridData.hauteur)  
            {  
                // On choisit l'hologramme selon si le joueur tient une carte ou non
                bool hasCard = (placementManager.ActiveCard != null);
                GameObject hoverAActiver = hasCard ? cardHoverIndicator : defaultHoverIndicator;
                GameObject hoverADesactiver = hasCard ? defaultHoverIndicator : cardHoverIndicator;

                if (caseActuelle != derniereCaseSurvolee)  
                {  
                    derniereCaseSurvolee = caseActuelle;  
                }

                if (hoverADesactiver != null) hoverADesactiver.SetActive(false);
                
                if (hoverAActiver != null)
                {
                    hoverAActiver.SetActive(true);
                    
                    // On récupère la hauteur de la case pour poser l'hologramme dessus
                    float hauteurPhysique = placementManager.GridData.Grid[gridX, gridY].elevation * placementManager.GridVisualizer.hauteurNiveau;
                    Vector3 worldPos = placementManager.GetWorldPosition(caseActuelle);
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
}