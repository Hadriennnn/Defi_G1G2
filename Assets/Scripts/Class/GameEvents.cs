using System;
using UnityEngine;

public static class GameEvents
{
    // L'événement classique (Action = ne retourne rien)
    public static event Action OnGridChanged;

    // L'événement de requête (Func = prend 2 Vector2Int en paramètres, et retourne un booléen)
    public static event Func<Vector2Int, Vector2Int, bool> RequestPathCheck;

    // Méthode pour annoncer que la grille a changé
    public static void TriggerGridChanged()
    {
        OnGridChanged?.Invoke();
    }

    // Méthode pour poser la question "Y a-t-il un chemin possible ?"
    public static bool CheckPathPossible(Vector2Int start, Vector2Int end)
    {
        // Si quelqu'un écoute cette question, on lui demande et on renvoie sa réponse
        if (RequestPathCheck != null)
        {
            return RequestPathCheck.Invoke(start, end);
        }
        
        // Sécurité au cas où aucun script ne réponde
        return true; 
    }
}