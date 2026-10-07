
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[System.Serializable]
public struct GhostwithID
{
    public GameObject ghost;
    public GameObject prefab;
    public int id;
}


public class UITroopsGenerator : MonoBehaviour
{
    [Header("Entities")]
    public GhostwithID[] ghostPrefabs;


    private GameObject currentGhost;
    private int currentID = -1;

    private bool isDragging = false;

    private Camera mainCamera;


    private void Start()
    {
        mainCamera = Camera.main;
    }


    private void Update()
    {
        if (!isDragging || currentGhost == null)
            return;


        // El Ghost sigue al ratón
        DragEntity();


        // Click izquierdo
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            // No colocar si estamos sobre la UI
            if (EventSystem.current.IsPointerOverGameObject())
                return;


            DropEntity();
        }
    }


    // =========================================================
    // GENERATE
    // =========================================================

    public void UIGenerateEntity(int id)
    {
        GenerateEntity(id);
    }


    private void GenerateEntity(int id)
    {
        // Eliminar Ghost anterior si existe
        if (currentGhost != null)
        {
            Destroy(currentGhost);
        }


        // Buscar entidad por ID
        for (int i = 0; i < ghostPrefabs.Length; i++)
        {
            if (ghostPrefabs[i].id == id)
            {
                currentID = id;


                // Crear Ghost
                currentGhost =
                    Instantiate(
                        ghostPrefabs[i].ghost
                    );


                isDragging = true;

                return;
            }
        }


        Debug.LogWarning(
            "No existe una entidad con ID: " + id
        );
    }


    // =========================================================
    // DRAG
    // =========================================================

    private void DragEntity()
    {
        if (Mouse.current == null)
            return;


        Vector3 mouseWorldPosition =
            GetMouseWorldPosition();


        // El Ghost simplemente sigue al ratón
        currentGhost.transform.position =
            mouseWorldPosition;
    }


    // =========================================================
    // DROP
    // =========================================================

    private void DropEntity()
    {
        if (currentGhost == null)
            return;

        Vector3 spawnPosition = currentGhost.transform.position;

        GameObject realPrefab = GetPrefabByID(currentID);

        if (realPrefab == null)
        {
            Debug.LogWarning(
                "No existe prefab para ID: " + currentID
            );

            return;
        }

        GameObject newTroop = Instantiate(
            realPrefab,
            spawnPosition,
            Quaternion.identity
        );

        // Inicializar la tropa
        Troop troop = newTroop.GetComponent<Troop>();

        if (troop != null)
        {
            // Aquí necesitamos pasarle su TroopDefinition
        }

        Destroy(currentGhost);

        currentGhost = null;
        currentID = -1;
        isDragging = false;
    }


    // =========================================================
    // GET PREFAB
    // =========================================================

    private GameObject GetPrefabByID(int id)
    {
        for (int i = 0; i < ghostPrefabs.Length; i++)
        {
            if (ghostPrefabs[i].id == id)
            {
                return ghostPrefabs[i].prefab;
            }
        }


        return null;
    }


    // =========================================================
    // MOUSE to WORLD
    // =========================================================

    private Vector3 GetMouseWorldPosition()
    {
        if (Mouse.current == null)
            return Vector3.zero;


        Vector2 mousePosition =
            Mouse.current.position.ReadValue();


        Vector3 screenPosition = new Vector3(
            mousePosition.x,
            mousePosition.y,
            Mathf.Abs(
                mainCamera.transform.position.z
            )
        );


        Vector3 worldPosition =
            mainCamera.ScreenToWorldPoint(
                screenPosition
            );


        // Juego 2D to mantenemos Z = 0
        worldPosition.z = 0f;


        return worldPosition;
    }
}

