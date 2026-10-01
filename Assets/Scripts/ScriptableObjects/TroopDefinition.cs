using System;
using UnityEngine;

[CreateAssetMenu(fileName = "TroopDefinition", menuName = "Scriptable Objects/TroopDefinition")]
public class TroopDefinition : ScriptableObject
{
    [SerializeField] private int ID;
    [SerializeField] private string Name;
    // [SerializeField] private Type role;
    // [SerializeField] private Faction faction;

    [SerializeField] private int maxHealth;
    [SerializeField] private float speed;
    [SerializeField] private int strength;
    [SerializeField] private int attackReach;
    // [SerializeField] private Resource[] cost;
    // [SerializeField] private Resoruce[] capacity;

    [SerializeField] private GameObject _prefab;
    [SerializeField] private Sprite[] _sprites;
    [SerializeField] private Animator _animator;
}
