using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Límites")]
    [SerializeField] private Vector2 limiteInferior = new Vector2(-18.5f, -10f);
    [SerializeField] private Vector2 limiteSuperior = new Vector2(18.5f, 10f);

    [Header("Configuración de Velocidad")]
    [SerializeField] private float keyboardSpeed = 6f;
    [SerializeField] private float mouseDragSpeed = 0.02f;

    [Header("Configuración del Zoom 2D")]
    [SerializeField] private float zoomMinimo = 2f;
    [SerializeField] private float zoomMaximo = 7f;
    [SerializeField] private float zoomSpeed = 0.15f;

    private Camera camara;

    private Vector3 lastMousePosition;
    private bool isDragging = false;

    private void Awake()
    {
        camara = GetComponent<Camera>();
    }

    private void Update()
    {
        HandleKeyboardMovement();
        HandleMouseDrag();
        HandleMouseZoom();
        ApplyCameraLimits();
    }

    private void HandleKeyboardMovement()
    {
        float horizontalInput = 0f;
        float verticalInput = 0f;

        if (Utils.KeyHeld(Key.UpArrow)) verticalInput += 1f;
        if (Utils.KeyHeld(Key.DownArrow)) verticalInput -= 1f;
        if (Utils.KeyHeld(Key.LeftArrow)) horizontalInput -= 1f;
        if (Utils.KeyHeld(Key.RightArrow)) horizontalInput += 1f;

        Vector3 direction = new Vector3(horizontalInput, verticalInput, 0f);

        direction = Vector3.ClampMagnitude(direction, 1f);

        transform.Translate(direction * keyboardSpeed * Time.deltaTime, Space.World);
    }

    private void HandleMouseDrag()
    {
        if (!Utils.MouseActive())
            return;

        if (Utils.MouseClicked(Utils.LeftClick))
        {
            isDragging = true;
            lastMousePosition = Utils.GetMousePos();
        }

        if (Utils.MouseReleased(Utils.LeftClick))
        {
            isDragging = false;
        }

        if (!isDragging)
            return;

        Vector2 currentMousePosition = Utils.GetMousePos();
        Vector2 deltaMouse = currentMousePosition - new Vector2(lastMousePosition.x, lastMousePosition.y);

        // Efecto "agarre" (invertir).
        Vector3 mouseDirection = new Vector3(-deltaMouse.x, -deltaMouse.y, 0f);

        transform.Translate(mouseDirection * mouseDragSpeed, Space.World);

        lastMousePosition = currentMousePosition;
    }

    private void HandleMouseZoom()
    {
        if (!Utils.MouseActive() || camara == null || !camara.orthographic)
            return;

        float scroll = Mouse.current.scroll.ReadValue().y;

        if (Mathf.Approximately(scroll, 0f))
            return;

        Vector2 posicionCursor = Utils.GetMousePos();
        float distancia = -camara.transform.position.z / camara.transform.forward.z;
        Vector3 puntoAntes = new Vector3(posicionCursor.x, posicionCursor.y, distancia);
        Vector3 mundoAntes = camara.ScreenToWorldPoint(puntoAntes);

        float nuevoTamaño = camara.orthographicSize - scroll * zoomSpeed;
        camara.orthographicSize = Mathf.Clamp(nuevoTamaño, zoomMinimo, zoomMaximo);

        Vector3 mundoDespues = camara.ScreenToWorldPoint(puntoAntes);
        transform.position += mundoAntes - mundoDespues;
    }

    private void ApplyCameraLimits()
    {
        Vector3 position = transform.position;

        float mitadAlto = camara.orthographicSize;
        float mitadAncho = mitadAlto * camara.aspect;

        float minX = limiteInferior.x + mitadAncho;
        float maxX = limiteSuperior.x - mitadAncho;

        float minY = limiteInferior.y + mitadAlto;
        float maxY = limiteSuperior.y - mitadAlto;

        if (minX > maxX)
            position.x = (limiteInferior.x + limiteSuperior.x) / 2f;
        else
            position.x = Mathf.Clamp(position.x, minX, maxX);

        if (minY > maxY)
            position.y = (limiteInferior.y + limiteSuperior.y) / 2f;
        else
            position.y = Mathf.Clamp(position.y, minY, maxY);

        transform.position = position;
    }
}