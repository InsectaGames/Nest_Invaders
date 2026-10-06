using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "TroopDefinition", menuName = "Scriptable Objects/TroopDefinition")]
public class TroopDefinition : ScriptableObject
{
    public int ID { get; private set; }
    public string Name { get; private set; }
    public Faction faction;
    public TroopType type;

    public int maxHealth;
    public float speed;
    public int strength;
    public int attackReach;


    public ResourceSlot[] recursos;  // <Resource,int>  ==== ResourceInventory   Cost[] cost;
    public ResourceSlot[] capacity; // <Resource , int>

    public GameObject _prefab;
    public Sprite[] _sprites;
    public Animator _animator;
}
