using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Límites")]
    [SerializeField] private Vector2 limiteInferior = new Vector2(-6.8f, -3.7f);
    [SerializeField] private Vector2 limiteSuperior = new Vector2(6.7f, 3.8f);

    [Header("Configuración de Velocidad")]
    [SerializeField] private float keyboardSpeed = 6f;
    [SerializeField] private float mouseDragSpeed = 0.01f;

    private Vector3 lastMousePosition;
    private bool isDragging = false;

    private void Update()
    {
        HandleKeyboardMovement();
        HandleMouseDrag();
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

        if (Utils.MouseClicked(Utils.MiddleClick))
        {
            isDragging = true;
            lastMousePosition = Utils.GetMousePos();
        }

        if (Utils.MouseReleased(Utils.MiddleClick))
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

    private void ApplyCameraLimits()
    {
        Vector3 position = transform.position;

        position.x = Utils.Clamp(position.x, limiteInferior.x, limiteSuperior.x);
        position.y = Mathf.Clamp(position.y, limiteInferior.y, limiteSuperior.y);

        transform.position = position;
    }
}