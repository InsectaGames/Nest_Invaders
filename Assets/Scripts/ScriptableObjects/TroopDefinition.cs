using System;
using UnityEngine;

[CreateAssetMenu(fileName = "TroopDefinition", menuName = "Scriptable Objects/TroopDefinition")]
public class TroopDefinition : ScriptableObject
{
    public int ID { get; private set; }
    public string Name { get; private set; }
    // [SerializeField] private Type role;
    // [SerializeField] private Faction faction;

    [SerializeField] private int maxHealth;
    [SerializeField] private float speed;
    [SerializeField] private int strength;
    [SerializeField] private int attackReach;
    // [SerializeField] private Resource[] cost;
    // [SerializeField] private Resource[] capacity;

    [SerializeField] private GameObject _prefab;
    [SerializeField] private Sprite[] _sprites;
    [SerializeField] private Animator _animator;
}
