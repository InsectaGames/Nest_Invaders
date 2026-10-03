using System.Collections;
using UnityEngine;

public class SpawnSystem
{
    public int RemainingCooldown { get; private set; }
    [SerializeField] private SpawnController spawnController;

    public bool RequestSpawn(string name)
    {
        bool result = spawnController.CanSpawn();
        if(result) { SpawnUnit(name); }
        return result;
    }

    private void SpawnUnit(string name)
    {
        
    }
}