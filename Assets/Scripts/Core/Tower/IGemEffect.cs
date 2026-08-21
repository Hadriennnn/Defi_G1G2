using UnityEngine;


 // Le contrat universel pour toutes les gemmes du jeu.
 
public interface IGemEffect
{
    // ID de la gemme 
    string GemId { get; }

    // Appelé au moment où le joueur glisse la gemme dans la tour.
    void Equip(BasicTower tower);

    // Appelé si la gemme est remplacée ou détruite.
    void Unequip(BasicTower tower);
}