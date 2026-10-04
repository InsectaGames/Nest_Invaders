using UnityEngine;
using UnityEngine.InputSystem;

public class TowerPlacement : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private GameObject towerPrefab;
    
    [Tooltip("Tecla para activar el modo de colocación.")]
    [SerializeField] private Key placementKey = Key.T;
    [SerializeField] private Key cancelKey = Key.Escape;
    
    [Tooltip("Capa del suelo o terreno donde se puede colocar la torre.")]
    [SerializeField] private LayerMask groundLayerMask;

    private GameObject currentPreview;
    private bool isPlacing = false;

    private void Update()
    {
        if (InputUtils.KeyPressed(placementKey) && !isPlacing)
        {
            StartPlacement();
        }

        if (isPlacing)
        {
            UpdatePreviewPosition();

            // Click izquierdo para confirmar
            if (InputUtils.MouseClicked())
            {
                ConfirmPlacement();
            }
            else if (InputUtils.KeyPressed(cancelKey))
            {
                CancelPlacement();
            }
        }
    }

    private void StartPlacement()
    {
        Debug.Log("Colocando torre...");

        if (towerPrefab == null)
        {
            Debug.LogWarning("No hay un prefab de torre asignado para colocar.");
            return;
        }

        isPlacing = true;
        currentPreview = Instantiate(towerPrefab);
        
        Collider col = currentPreview.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        UpdatePreviewPosition();
    }

    private void UpdatePreviewPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(InputUtils.GetMousePos());
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayerMask))
        {
            currentPreview.transform.position = hit.point;
        }
        else
        {
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            if (groundPlane.Raycast(ray, out float distance))
            {
                currentPreview.transform.position = ray.GetPoint(distance);
            }
        }
    }

    private void ConfirmPlacement()
    {
        Collider col = currentPreview.GetComponent<Collider>();
        if (col != null) col.enabled = true;

        Debug.Log("Torre colocada.");

        isPlacing = false;
        currentPreview = null;
    }

    private void CancelPlacement()
    {
        if (currentPreview != null)
        {
            Destroy(currentPreview);
        }
        isPlacing = false;
    }
}