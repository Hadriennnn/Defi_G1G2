using UnityEngine;

public class GameInitiator : MonoBehaviour
{
    [Header("Références des Systèmes Noyau")]
    [SerializeField] private GridData gridData;
    [SerializeField] private Pathfinding pathfinding;
    [SerializeField] private GridVisualizer gridVisualizer;

    [Header("Références Interface & Caméra")]
    [SerializeField] private TopBarUIManager topBarUI;
    [SerializeField] private HandUIManager handUI;
    [SerializeField] private HealthBarManager healthBarManager;
    [SerializeField] private CameraController cameraController;

    void Start()
    {
        LancerLeJeu();
    }

    private void LancerLeJeu()
    {
        Debug.Log("[GameInitiator] 1/7 : Création de la Toile procédurale (ChunkManager)...");
        ChunkManager.Instance.GenererMondeInitial();

        Debug.Log("[GameInitiator] 2/7 : Génération des données physiques de la grille...");
        gridData.Init();

        Debug.Log("[GameInitiator] 3/7 : Création des décors et visuels 3D...");
        gridVisualizer.Init();
        
        Debug.Log("[GameInitiator] 4/7 : Initialisation de l'IA et du Pathfinding...");
        pathfinding.Init();

        Debug.Log("[GameInitiator] 5/7 : Chargement de la Base et du Deck...");
        BaseManager.Instance.Init();
        DeckManager.Instance.Init();

        Debug.Log("[GameInitiator] 6/7 : Connexion de l'Interface UI et Caméra...");
        if (topBarUI != null) topBarUI.Init();
        if (handUI != null) handUI.Init();
        if (healthBarManager != null) healthBarManager.Init();
        if (cameraController != null) cameraController.Init();

        Debug.Log("[GameInitiator] 7/7 : Démarrage de la boucle de jeu...");
        GameManager.Instance.Init(); 
        
        Debug.Log("[GameInitiator] --- LE JEU EST PRÊT ! ---");
    }
}