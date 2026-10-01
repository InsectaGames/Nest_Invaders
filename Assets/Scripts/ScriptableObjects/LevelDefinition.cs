using UnityEngine;

[CreateAssetMenu(fileName = "LevelDefinition", menuName = "Scriptable Objects/LevelDefinition")]
public class LevelDefinition : ScriptableObject
{
    [SerializeField] private int ID;
    [SerializeField] private string Name;
    [SerializeField] private Sprite Map;
    [SerializeField] private GameObject allyBase;
    [SerializeField] private GameObject enemyBase;
    // [SerializeField] private Resource[] initialResources;
    // [SerializeField] private Troops[] initialTroops;
    // [SerializeField] private Troops[] initialEnemies;
    [SerializeField] private int dayLength;
    [SerializeField] private int nightLength;
}
