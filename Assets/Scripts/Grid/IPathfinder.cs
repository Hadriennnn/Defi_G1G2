using UnityEngine;

public interface IPathfinder
{
    bool FindPath(Vector2Int startPos, Vector2Int targetPos);
}