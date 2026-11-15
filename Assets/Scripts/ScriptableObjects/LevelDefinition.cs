using UnityEngine;

public enum LevelDifficulty
{
    EASY, NORMAL, HARD, EXPERT
}

[System.Serializable]
public struct EnemySummons
{
    public TroopDefinition enemyTroop;
    public int chance;
}

[CreateAssetMenu(fileName = "LevelDefinition", menuName = "Scriptable Objects/LevelDefinition")]
public class LevelDefinition : ScriptableObject
{
    [Header("Identificación")]
    public int ID;
    public string Name;
    public LevelDifficulty difficulty;

    [Header("Progresión")]
    public LevelDefinition nextLevel;

    [Header("Duración")]
    public int dayLength;
    public int nightLength;

    [Header("Recursos")]
    public ResourceSlot[] initialResources;

    [Header("Mapa")]
    public Sprite Map;

    [Header("Base Aliada")]
    public GameObject allyBase;
    public int allyBaseHealth = 10;

    [Header("Base Enemiga")]
    public GameObject enemyBase;
    public int enemyBaseHealth = 10;

    [Header("Obstáculos")]
    public int obstacleNumber;

    [Header("Generación enemiga")]
    [Min(0.1f)] public float enemySpawnInterval = 10f;
    public EnemySummons[] enemyTroops;
    public TroopDefinition defaultTroop;
}
