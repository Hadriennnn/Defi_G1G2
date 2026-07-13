using UnityEngine;

public class GameInitiator : MonoBehaviour
{
    [Header("Références des Systèmes")]
    [SerializeField] private GridData gridData;
    [SerializeField] private Pathfinding pathfinding;
    [SerializeField] private GridVisualizer gridVisualizer;
    [SerializeField] private GameManager gameManager;

    void Start()
    {
        LancerLeJeu();
    }

    private void LancerLeJeu()
    {
        Debug.Log("[GameInitiator] 1/4 : Génération des données de la grille...");
        gridData.Init();

        

        Debug.Log("[GameInitiator] 3/4 : Création des décors et visuels...");
        // Le visuel a besoin de la grille et du chemin initial !
        gridVisualizer.Init();
        
        Debug.Log("[GameInitiator] 2/4 : Initialisation de l'IA et du Pathfinding...");
        // Le Pathfinding a besoin que la grille soit générée en premier !
        pathfinding.Init();

        Debug.Log("[GameInitiator] 4/4 : Démarrage de la boucle de jeu...");
        // Tout est prêt, on lance la phase de préparation !
        gameManager.Init();
        
        Debug.Log("[GameInitiator] --- LE JEU EST PRÊT ! ---");
    }
}