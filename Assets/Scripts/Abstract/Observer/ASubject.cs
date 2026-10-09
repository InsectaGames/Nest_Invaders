using System.Collections.Generic;
using UnityEngine;

public abstract class ASubject<T> : MonoBehaviour, ISubject<T>
{
    private List<IObserver<T>> _observers = new List<IObserver<T>>();

    public void AddObserver(IObserver<T> obs) => _observers.Add(obs);

    public void RemoveObserver(IObserver<T> obs) => _observers.Remove(obs);

    public void UpdateObservers(T data)
    {
        foreach (IObserver<T> obs in _observers)
        { 
            obs?.UpdateObserver(data);
        }
    }
}