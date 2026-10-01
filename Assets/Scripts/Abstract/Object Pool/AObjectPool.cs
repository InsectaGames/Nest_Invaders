using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AObjectPool<T> : IObjectPool<T> where T : MonoBehaviour, IPoolable
{
    // Una cola para los disponibles, un conjunto (HashSet) para los activos.
    private readonly Queue<T> available = new();
    private readonly HashSet<T> active = new();
    public IEnumerable<T> ActiveObjects => active;

    // Prefab y padre, para los Manager.
    private readonly T prefab;
    private readonly Transform parent;

    // Constructor que inicializa la cola.
    public AObjectPool(T prefab, int initialSize, Transform parent)
    {
        this.prefab = prefab;
        this.parent = parent;

        // Inicializa el pool con la cantidad de objetos necesaria.
        for (int i = 0; i < initialSize; i++)
        {
            T obj = Object.Instantiate(prefab, parent);
            obj.gameObject.SetActive(false);
            available.Enqueue(obj);
        }
    }

    // Obtener un objeto del pool.
    public T Get()
    {
        if (available.Count == 0)
        {
            Debug.Log($"¡POOL de ({typeof(T).Name}) agotado! No puedo instanciar más.");
            return null;
        }

        /* 
         * Solo da un objeto si está disponible,
         * lo quita de la cola y lo pasa a active.
        */
        
        T obj = available.Dequeue();
        active.Add(obj);
        obj.OnSpawn();
        return obj;
    }

    // Libera un objeto, lo quita de los active y lo devuelve a la cola.
    public void Release(T obj)
    {
        active.Remove(obj);
        obj.ResetState();
        obj.OnDespawn();
        available.Enqueue(obj);
    }

    // Libera TODOS los objetos activos.
    public void ReleaseAll()
    {
        var toRelease = active.ToArray();

        foreach(var obj in toRelease)
        {
            Release(obj);
        }
    }
}