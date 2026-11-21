
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
    public int obstacleNumber;

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

    public bool IsMainBossStage = false;
    public GameObject bossPrefab;
}
