using UnityEngine;

// Contient toutes les données de bases d'un ennemi

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "TowerDefense/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Informations")]
    public string enemyName = "DEFAULT_NAME";
    
    [Header("Statistiques de base")]
    public float baseHealth = 100f;
    public float baseArmor = 5f;
    public float baseSpeed = 3f;
    
    [Tooltip("Dégâts infligés au Noyau si l'ennemi atteint la base")]
    public int coreDamage = 1;
}