using UnityEngine;

[CreateAssetMenu(fileName = "LevelDefinition", menuName = "Scriptable Objects/LevelDefinition")]
public class LevelDefinition : ScriptableObject
{
    public int ID;
    public string Name;
    public int dayLength;
    public int nightLength;
    public ResourceSlot[] initialResources;
    // [SerializeField] private Troop[] initialTroops;
    // [SerializeField] private Troop[] initialEnemies;
    public Sprite Map;
    public GameObject allyBase;
    public GameObject enemyBase;
}