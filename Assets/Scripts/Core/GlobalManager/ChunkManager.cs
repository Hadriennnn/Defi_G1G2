using System.Collections.Generic;
using UnityEngine;

public class GlobalSocket
{
    public Vector2Int position;
    public SocketDirection direction;
    public SocketType type;
    public int chunkId;
}

public class ChunkManager : MonoBehaviour
{
    public static ChunkManager Instance { get; private set; }

    [Header("Bibliothèque de Chunks")]
    public ChunkData chunkDeBase;
    public List<ChunkData> chunksDisponibles;

    [Header("Paramètres du Monde")]
    public int globalGridSize = 50;
    
    [Header("Nombre de chunk à générer")]
    [Tooltip("Combien de Chunks supplémentaires l'algorithme doit-il coller à la base pour créer cet Acte ?")]
    public int nombreDeChunksAGenerer = 4;

    // --- LES DONNÉES CUITES (Baking) ---
    public int[,] bakedElevation;
    public int[,] bakedChunkIds;
    
    // --- LES LISTES DYNAMIQUES ---
    public List<Vector2Int> activeSpawns = new List<Vector2Int>();
    public Vector2Int baseNoyauCoords;
    private List<GlobalSocket> openDoors = new List<GlobalSocket>();

    private int currentChunkIdCounter = 1;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void GenererMondeInitial()
    {
        bakedElevation = new int[globalGridSize, globalGridSize];
        bakedChunkIds = new int[globalGridSize, globalGridSize];

        for (int x = 0; x < globalGridSize; x++)
        {
            for (int y = 0; y < globalGridSize; y++)
            {
                bakedElevation[x, y] = -1; 
                bakedChunkIds[x, y] = 0;   
            }
        }

        if (chunkDeBase != null)
        {
            int offsetX = (globalGridSize / 2) - (chunkDeBase.largeur / 2);
            int offsetY = (globalGridSize / 2) - (chunkDeBase.hauteur / 2);
            
            // Le Chunk de base reçoit toujours l'ID 1
            PlacerChunk(chunkDeBase, new Vector2Int(offsetX, offsetY));
            
            // NOUVEAU : On génère immédiatement le reste de l'île (qui restera invisible)
            for (int i = 0; i < nombreDeChunksAGenerer; i++)
            {
                GenererNouveauChunk();
            }
        }
        else
        {
            Debug.LogError("[ChunkManager] ERREUR : Aucun Chunk de Base assigné !");
        }
    }

    public bool GenererNouveauChunk()
    {
        if (openDoors.Count == 0 || chunksDisponibles.Count == 0) return false;

        GlobalSocket porteCible = openDoors[Random.Range(0, openDoors.Count)];

        for (int i = 0; i < 10; i++)
        {
            ChunkData chunkCandidat = chunksDisponibles[Random.Range(0, chunksDisponibles.Count)];
            SocketDirection directionOpposee = GetOppositeDirection(porteCible.direction);
            
            foreach (ChunkSocket socketCandidat in chunkCandidat.sockets)
            {
                if (socketCandidat.type == SocketType.Porte_Sortie && socketCandidat.direction == directionOpposee)
                {
                    Vector2Int directionVector = GetVectorFromDirection(porteCible.direction);
                    Vector2Int caseDeContactGlobal = porteCible.position + directionVector;
                    Vector2Int offset = caseDeContactGlobal - socketCandidat.localPosition;

                    if (CheckCollision(chunkCandidat, offset))
                    {
                        PlacerChunk(chunkCandidat, offset);
                        openDoors.Remove(porteCible);
                        return true;
                    }
                }
            }
        }
        
        Debug.LogWarning("[ChunkManager] Impossible de placer un Chunk ici après 10 essais.");
        return false; 
    }

    private bool CheckCollision(ChunkData chunk, Vector2Int offset)
    {
        for (int y = 0; y < chunk.hauteur; y++)
        {
            for (int x = 0; x < chunk.largeur; x++)
            {
                int globalX = offset.x + x;
                int globalY = offset.y + y;

                if (globalX < 0 || globalX >= globalGridSize || globalY < 0 || globalY >= globalGridSize)
                    return false;

                int elevationLocale = chunk.elevationMap[y].cells[x];

                if (elevationLocale != -1)
                {
                    if (bakedChunkIds[globalX, globalY] != 0)
                    {
                        return false; 
                    }
                }
            }
        }
        return true; 
    }

    private void PlacerChunk(ChunkData chunk, Vector2Int offset)
    {
        int chunkIdAssigne = currentChunkIdCounter;
        currentChunkIdCounter++;

        for (int y = 0; y < chunk.hauteur; y++)
        {
            for (int x = 0; x < chunk.largeur; x++)
            {
                int elevationLocale = chunk.elevationMap[y].cells[x];

                if (elevationLocale != -1)
                {
                    int globalX = offset.x + x;
                    int globalY = offset.y + y;

                    bakedElevation[globalX, globalY] = elevationLocale;
                    bakedChunkIds[globalX, globalY] = chunkIdAssigne;
                }
            }
        }

        foreach (ChunkSocket socket in chunk.sockets)
        {
            Vector2Int globalPos = offset + socket.localPosition;

            if (socket.type == SocketType.Porte_Entree)
            {
                openDoors.Add(new GlobalSocket { position = globalPos, direction = socket.direction, type = socket.type, chunkId = chunkIdAssigne });
            }
            else if (socket.type == SocketType.Portail_Spawn)
            {
                activeSpawns.Add(globalPos);
            }
            else if (socket.type == SocketType.Noyau_Base)
            {
                baseNoyauCoords = globalPos;
            }
        }

        Debug.Log($"[ChunkManager] Chunk {chunk.name} (ID:{chunkIdAssigne}) placé avec succès en {offset}");
    }

    private SocketDirection GetOppositeDirection(SocketDirection dir)
    {
        switch (dir)
        {
            case SocketDirection.Nord: return SocketDirection.Sud;
            case SocketDirection.Sud: return SocketDirection.Nord;
            case SocketDirection.Est: return SocketDirection.Ouest;
            case SocketDirection.Ouest: return SocketDirection.Est;
            default: return SocketDirection.Aucune;
        }
    }

    private Vector2Int GetVectorFromDirection(SocketDirection dir)
    {
        switch (dir)
        {
            case SocketDirection.Nord: return new Vector2Int(0, 1);
            case SocketDirection.Sud: return new Vector2Int(0, -1);
            case SocketDirection.Est: return new Vector2Int(1, 0);
            case SocketDirection.Ouest: return new Vector2Int(-1, 0);
            default: return Vector2Int.zero;
        }
    }
}