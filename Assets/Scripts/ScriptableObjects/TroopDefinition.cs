using BehaviourAPI.UnityToolkit;
using UnityEngine;

[CreateAssetMenu(fileName = "TroopDefinition", menuName = "Scriptable Objects/TroopDefinition")]
public class TroopDefinition : ScriptableObject
{
    [Header("General")]
    public int ID;
    public string Name;
    public TroopType type;
    public BehaviourRunner behaviour;

    [Header("Stats")]
    public int maxHealth;
    public float speed;
    public int strength;
    public int attackReach;
    public int chanceToDropSticks = 0;

    [Header("Recursos")]
    public ResourceSlot[] recursos;  // <Resource, int>  ==== ResourceInventory   Cost[] cost;
    public ResourceSlot[] capacity;  // <Resource, int>

    [Header("GameObject y Propiedades")]
    public GameObject _prefab;
    public Color FactionColor;
    public Sprite[] _sprites;
    public Animator _animator;
}
