
using UnityEngine;

public enum LevelDifficulty
{
    EASY, NORMAL, HARD, EXPERT
}

public enum ObstacleType
{
    BUSH_A , 
    BUSH_B , 
    BUSH_C ,
    BUSH_D , 
    BUSH_E , 
    FLOWERS, 
    MUSH_A ,
    MUSH_B ,
    ROCK_A ,
    ROCK_B ,
    ROCK_C ,
    ROCK_D ,
    ROCK_E ,
    ROCK_F ,
    STICK_A,
    STICK_B ,
    LOG

}

[System.Serializable]
public struct EnemySummons
{
    public TroopDefinition enemyTroop;
    public int chance;
}


[System.Serializable]
public struct Obstacle
{
    public ObstacleType obsType;
    public Vector2 pos;
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
    public int passiveCooldown = 60;
    public int passiveAmount = 1;
    public ResourceSlot[] initialResources;

    [Header("Mapa")]
    public Sprite Map;

    [Header("Base Aliada")]
    public GameObject allyBase;
    public Vector2[] allyBasePositions;
    public int allyBaseHealth = 10;

    [Header("Zona de recursos")]
    public GameObject resourceArea;
    public Vector2[] resourceAreaPositions;

    [Header("Base Enemiga")]
    public GameObject enemyBase;
    public Vector2[] enemyBasePositions;
    public int enemyBaseHealth = 10;

    [Header("Obstáculos")]
    public Obstacle[] obstacles;

    [Header("Generación enemiga")]
    [Min(0.1f)] public float enemySpawnInterval = 10f;
    [Min(1)] public int maxSpawnableAmount = 1;
    [Min(1)] public int chanceToSpawnMore = 20;
    public EnemySummons[] enemyTroops;
    public TroopDefinition defaultTroop;

    [Header("Generación de criaturas de noche")]
    [Min(0)] public int maxNightCreatureAmount = 3;

    [Header("Generación de Jefe")]
    public bool IsMidBossStage = false;
    [Min(0.1f)] public float midBossSpawnDistance = 3f;

    [Header("Tutorial?")]
    public bool _tutorial;

    public bool IsMainBossStage = false;
    public GameObject bossPrefab;
}
