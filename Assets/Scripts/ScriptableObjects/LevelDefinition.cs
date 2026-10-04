using UnityEngine;

[CreateAssetMenu(fileName = "LevelDefinition", menuName = "Scriptable Objects/LevelDefinition")]
public class LevelDefinition : ScriptableObject
{
    public int ID;
    public string Name;
    public int dayLength;
    public int nightLength;
    public Resource[] initialResources;
    // [SerializeField] private Troops[] initialTroops;
    // [SerializeField] private Troops[] initialEnemies;
    public Sprite Map;
    public GameObject allyBase;
    public GameObject enemyBase;
}