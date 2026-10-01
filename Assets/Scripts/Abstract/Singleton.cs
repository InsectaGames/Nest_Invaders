using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    // Instancia del Singleton.
    protected static T _instance;

    // Propiedad que maneja la instancia del Singleton.
    public static T Instance
    {
        get
        {
            // Si no hay una instancia, busca una ya existente o crea una nueva.
            if (_instance == null)
            {
                // Busca en la escena un objeto del tipo T.
                _instance = FindAnyObjectByType<T>();

                if (_instance == null)
                {
                    Debug.LogError($"[Singleton<{typeof(T)}>] No se encontró ninguna instancia en escena.");
                }
            }

            return _instance;
        }
    }

    // Evita que se cree una nueva instancia mediante el constructor.
    protected Singleton() { }

    // Si ya existe una instancia, destruye el nuevo objeto.
    protected virtual void Awake()
    {
        // Establece la instancia si no ha sido configurada.
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            // Si ya existe una instancia diferente, destruye el objeto duplicado.
            Destroy(gameObject);
        }
    }
}