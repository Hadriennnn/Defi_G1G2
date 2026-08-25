using UnityEngine;


public class ChunkPreviewer : MonoBehaviour
{
    [Header("Le Chunk à visualiser")]
    public ChunkData chunkToPreview;

    [Header("Paramètres Visuels")]
    public float cellSize = 1f;
    public float hauteurNiveau = 0.7f;
    [Range(0f, 1f)] public float transparenceSol = 0.5f;

    void OnDrawGizmos()
    {
        if (chunkToPreview == null) return;
        if (chunkToPreview.elevationMap == null || chunkToPreview.elevationMap.Length == 0) return;

        Vector3 origine = transform.position;

        //DESSINER LA GRILLE
        for (int y = 0; y < chunkToPreview.hauteur; y++)
        {
            if (chunkToPreview.elevationMap[y].cells == null) continue;

            for (int x = 0; x < chunkToPreview.largeur; x++)
            {
                int elevation = chunkToPreview.elevationMap[y].cells[x];
                
                // Centre de la case (en X et Z)
                float centreX = origine.x + (x * cellSize) + (cellSize / 2f);
                float centreZ = origine.z + (y * cellSize) + (cellSize / 2f);

                // Si c'est du vide (-1), on dessine juste un contour rouge plat
                if (elevation == -1)
                {
                    Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
                    Gizmos.DrawWireCube(new Vector3(centreX, origine.y, centreZ), new Vector3(cellSize, 0.1f, cellSize));
                    continue;
                }

                // Hauteur physique
                float heightPos = origine.y + (elevation * hauteurNiveau);
                Vector3 centreCase = new Vector3(centreX, heightPos, centreZ);
                Vector3 tailleCase = new Vector3(cellSize, hauteurNiveau, cellSize);

                // Choix de la couleur selon la hauteur
                if (elevation == 0)
                    Gizmos.color = new Color(0.2f, 0.8f, 0.2f, transparenceSol); // Vert (Sol)
                else
                    Gizmos.color = new Color(0.5f, 0.5f, 0.5f, transparenceSol); // Gris (Montagne)

                Gizmos.DrawCube(centreCase - new Vector3(0, hauteurNiveau / 2f, 0), tailleCase);
                Gizmos.color = Color.black;
                Gizmos.DrawWireCube(centreCase - new Vector3(0, hauteurNiveau / 2f, 0), tailleCase); // Contour noir pour bien voir les cases
            }
        }

        // DESSINER LES POINTS D'INTÉRÊTS (Sockets)
        if (chunkToPreview.sockets != null)
        {
            foreach (ChunkSocket socket in chunkToPreview.sockets)
            {
                float centreX = origine.x + (socket.localPosition.x * cellSize) + (cellSize / 2f);
                float centreZ = origine.z + (socket.localPosition.y * cellSize) + (cellSize / 2f);
                
                // On récupère l'élévation pour placer l'icône à la bonne hauteur
                int el = chunkToPreview.elevationMap[socket.localPosition.y].cells[socket.localPosition.x];
                float heightPos = origine.y + (Mathf.Max(0, el) * hauteurNiveau) + 0.5f;
                Vector3 positionSocket = new Vector3(centreX, heightPos, centreZ);

                switch (socket.type)
                {
                    case SocketType.Portail_Spawn:
                        Gizmos.color = Color.magenta;
                        Gizmos.DrawSphere(positionSocket, cellSize * 0.3f);
                        break;

                    case SocketType.Noyau_Base:
                        Gizmos.color = Color.red;
                        // On dessine un gros cube de 2x2 pour la base !
                        Vector3 baseCenter = positionSocket + new Vector3(cellSize / 2f, 0f, cellSize / 2f);
                        Gizmos.DrawCube(baseCenter, new Vector3(cellSize * 1.8f, cellSize * 0.5f, cellSize * 1.8f));
                        break;

                    case SocketType.Porte_Entree:
                        Gizmos.color = Color.cyan;
                        Gizmos.DrawCube(positionSocket, new Vector3(cellSize * 0.4f, cellSize * 0.4f, cellSize * 0.4f));
                        DessinerDirection(positionSocket, socket.direction);
                        break;

                    case SocketType.Porte_Sortie:
                        Gizmos.color = new Color(1f, 0.5f, 0f); // Orange
                        Gizmos.DrawCube(positionSocket, new Vector3(cellSize * 0.4f, cellSize * 0.4f, cellSize * 0.4f));
                        DessinerDirection(positionSocket, socket.direction);
                        break;
                }
            }
        }
    }

    // Dessine une petite ligne pour voir vers où pointe la porte
    private void DessinerDirection(Vector3 centre, SocketDirection direction)
    {
        Vector3 directionVector = Vector3.zero;
        switch (direction)
        {
            case SocketDirection.Nord: directionVector = new Vector3(0, 0, 1); break;
            case SocketDirection.Sud: directionVector = new Vector3(0, 0, -1); break;
            case SocketDirection.Est: directionVector = new Vector3(1, 0, 0); break;
            case SocketDirection.Ouest: directionVector = new Vector3(-1, 0, 0); break;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(centre, directionVector * cellSize);
        Gizmos.DrawSphere(centre + (directionVector * cellSize), 0.1f);
    }
}