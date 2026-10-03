using UnityEngine;

[CreateAssetMenu(fileName = "LevelDefinition", menuName = "Scriptable Objects/LevelDefinition")]
public class LevelDefinition : ScriptableObject
{
    public int ID { get; private set; }
    public string Name { get; private set; }
    public int dayLength { get; private set; }
    public int nightLength { get; private set; }
    // [SerializeField] private Resource[] initialResources;
    // [SerializeField] private Troops[] initialTroops;
    // [SerializeField] private Troops[] initialEnemies;
    // [SerializeField] private Sprite Map;
    public GameObject allyBase { get; private set; }
    public GameObject enemyBase { get; private set; }
}