using UnityEngine;

/*
 * RÉSUMÉ : Point d'entrée de la scène, fait l'ordre strict d'initialisation de tous les systèmes au démarrage du jeux.
 */

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
        Debug.Log("[GameInitiator] 1/5 : Création de la Toile procédurale (ChunkManager)...");
        ChunkManager.Instance.GenererMondeInitial();

        Debug.Log("[GameInitiator] 2/5 : Génération des données physiques de la grille...");
        gridData.Init();

        Debug.Log("[GameInitiator] 3/5 : Création des décors et visuels 3D...");
        gridVisualizer.Init();
        
        Debug.Log("[GameInitiator] 4/5 : Initialisation de l'IA et du Pathfinding...");
        pathfinding.Init();

        Debug.Log("[GameInitiator] 5/5 : Démarrage de la boucle de jeu...");
        gameManager.Init();
        
        Debug.Log("[GameInitiator] --- LE JEU EST PRÊT ! ---");
    }
}