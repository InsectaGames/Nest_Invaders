
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class CameraController : MonoBehaviour
{
    [Header("Límites")]
    [SerializeField] private Vector2 limiteInferior = new Vector2(-18.5f, -10f);
    [SerializeField] private Vector2 limiteSuperior = new Vector2(18.5f, 10f);

    [Header("Configuración de Velocidad")]
    [SerializeField] private float keyboardSpeed = 6f;

    [Header("Configuración del Zoom 2D")]
    [SerializeField] private float zoomMinimo = 2f;
    [SerializeField] private float zoomMaximo = 7f;
    [SerializeField] private float zoomSpeed = 0.15f;
    [SerializeField] private float pinchZoomSpeed = 0.01f;

    private Camera camara;

    private Vector2 lastPointerPosition;
    private float lastPinchDistance;

    private bool isDragging;
    private int previousTouchCount;

    private void Awake() => camara = GetComponent<Camera>();

    private void OnEnable() => EnhancedTouchSupport.Enable();

    private void OnDisable() => EnhancedTouchSupport.Disable();

    private void Update()
    {
        if (camara == null)
            return;

        HandleKeyboardMovement();

        bool touchHandled = HandleTouchInput();

        if (!touchHandled)
        {
            HandleMouseDrag();
            HandleMouseZoom();
        }

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

    private bool HandleTouchInput()
    {
        if (Touchscreen.current == null)
            return false;

        var touches = Touch.activeTouches;
        int touchCount = touches.Count;

        if (touchCount == 0)
        {
            isDragging = false;
            previousTouchCount = 0;
            return true;
        }

        // Dos dedos: zoom mediante gesto pinch.
        if (touchCount >= 2)
        {
            isDragging = false;

            Vector2 firstPosition = touches[0].screenPosition;
            Vector2 secondPosition = touches[1].screenPosition;

            float currentDistance = Vector2.Distance(firstPosition, secondPosition);
            Vector2 pinchCenter = (firstPosition + secondPosition) * 0.5f;

            if (previousTouchCount < 2)
            {
                lastPinchDistance = currentDistance;
            }
            else
            {
                float pinchDelta = currentDistance - lastPinchDistance;
                ZoomAtScreenPosition(pinchCenter, pinchDelta * pinchZoomSpeed);

                lastPinchDistance = currentDistance;
            }

            previousTouchCount = touchCount;
            return true;
        }

        // Un dedo: arrastrar la cámara.
        Touch touch = touches[0];
        Vector2 currentPosition = touch.screenPosition;

        if (previousTouchCount != 1 || touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
        {
            lastPointerPosition = currentPosition;
            isDragging = true;
        }
        else if (isDragging)
        {
            MoveCameraByPointer(lastPointerPosition, currentPosition);
            lastPointerPosition = currentPosition;
        }

        previousTouchCount = touchCount;
        return true;
    }

    private void HandleMouseDrag()
    {
        if (Mouse.current == null)
            return;

        Vector2 currentPosition = Mouse.current.position.ReadValue();

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            isDragging = true;
            lastPointerPosition = currentPosition;
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            isDragging = false;
        }

        if (!isDragging || !Mouse.current.leftButton.isPressed)
            return;

        MoveCameraByPointer(lastPointerPosition, currentPosition);
        lastPointerPosition = currentPosition;
    }

    private void MoveCameraByPointer(Vector2 previousPosition, Vector2 currentPosition)
    {
        Vector3 previousWorldPosition = ScreenToWorld(previousPosition);
        Vector3 currentWorldPosition = ScreenToWorld(currentPosition);

        transform.position += previousWorldPosition - currentWorldPosition;
    }

    private void HandleMouseZoom()
    {
        if (Mouse.current == null || !camara.orthographic)
            return;

        float scroll = Mouse.current.scroll.ReadValue().y;

        if (Mathf.Approximately(scroll, 0f))
            return;

        Vector2 cursorPosition = Mouse.current.position.ReadValue();
        ZoomAtScreenPosition(cursorPosition, scroll * zoomSpeed);
    }

    // delta positivo = acercar cámara.
    private void ZoomAtScreenPosition(Vector2 screenPosition, float delta)
    {
        if (!camara.orthographic || Mathf.Approximately(delta, 0f))
            return;

        Vector3 worldBefore = ScreenToWorld(screenPosition);

        float newSize = camara.orthographicSize - delta;
        camara.orthographicSize = Mathf.Clamp(newSize, zoomMinimo, zoomMaximo);

        Vector3 worldAfter = ScreenToWorld(screenPosition);

        transform.position += worldBefore - worldAfter;
    }

    private Vector3 ScreenToWorld(Vector2 screenPosition)
    {
        float distance = -camara.transform.position.z / camara.transform.forward.z;

        return camara.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, distance));
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