using UnityEngine;
using System.Collections.Generic;

public class UITroopsGenerator : MonoBehaviour, ISubject<int>
{
    private List<IObserver<int>> _observers = new List<IObserver<int>>();

    // private void Update() => Debug.Log(_observers.Count);
    
    public void UIGenerateEntity(int id) => UpdateObservers(id);

    public void AddObserver(IObserver<int> obs) => _observers.Add(obs);

    public void RemoveObserver(IObserver<int> obs) => _observers.Remove(obs);

    public void UpdateObservers(int data)
    {
        foreach (IObserver<int> obs in _observers)
        { 
            obs?.UpdateObserver(data);
        }
    }
}

