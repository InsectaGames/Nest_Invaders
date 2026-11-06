using UnityEngine;

public enum LevelDifficulty
{
    EASY,
    NORMAL,
    HARD,
    EXPERT
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

    [Header("Bases")]
    public GameObject allyBase;
    public GameObject enemyBase;

    [Header("Obstáculos")]
    public int obstacleNumber;
}
