using UnityEngine;

public interface IGemEffect
{
    string GemId { get; }
    
    GemCategory Category { get; }
    ElementType Element { get; }

    void Equip(BasicTower tower);
    void Unequip(BasicTower tower);
}