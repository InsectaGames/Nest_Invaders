using System.Collections.Generic;
using UnityEngine;

public class CentipedeBodyFollow : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform head;
    [SerializeField] private List<Transform> segments = new List<Transform>();

    [Header("Movimiento")]
    [SerializeField] private float segmentSpacing = 0.59f; // Espacio entre segmentos, es mejor definirlo asi por motivos de rendimiento
    [SerializeField] private float sampleSpacing = 0.1f; // Frecuencia con la que calcula las actualizaciones de la posicion del centipede

    [Header("Distancias de la cabeza y la cola al resto de segmentos")]
    [SerializeField] private float headToFirstSegmentSpacing = 1.36f;
    [SerializeField] private float lastSegmentSpacing = 0.41f;

    [Header("Orientacion")]
    [Tooltip("Direccion hacia la que apunta el sprite originalmente.")]
    [SerializeField] private Vector2 localForwardAxis = Vector2.right;

    [SerializeField] private float rotationLookDistance = 0.15f;

    // Punto del recorrido y distancia acumulada.
    private struct PathPoint
    {
        public Vector3 position;
        public float distance;

        public PathPoint(Vector3 position, float distance)
        {
            this.position = position;
            this.distance = distance;
        }
    }

    private readonly List<PathPoint> path = new List<PathPoint>();
    private int firstValidIndex;
    private Vector3 lastHeadPosition;
    private float distanceTravelled;
    private float distanceToNextSample;

    // Angulo del eje original del sprite, calculado una sola vez.
    private float spriteAxisAngle;

    private void Start()
    {
        if (head == null)
        {
            Debug.LogError("CentipedeBodyFollow: No se ha asignado Head.", this);
            enabled = false;
            return;
        }

        CacheSpriteAxis();
        InitializeHistory();
    }

    private void LateUpdate()
    {
        RecordHeadMovement();
        UpdateSegments();
        CleanHistory();
    }

    // Guarda el angulo original del sprite.
    private void CacheSpriteAxis()
    {
        spriteAxisAngle = Mathf.Atan2(localForwardAxis.y, localForwardAxis.x) * Mathf.Rad2Deg;
    }

    // Inicializa el recorrido detras de la cabeza.
    private void InitializeHistory()
    {
        path.Clear();
        firstValidIndex = 0;

        distanceTravelled = 0f;
        distanceToNextSample = sampleSpacing;

        lastHeadPosition = head.position;

        float maxDistance = headToFirstSegmentSpacing + Mathf.Max(0, segments.Count - 2) * segmentSpacing + (segments.Count > 1 ? lastSegmentSpacing : 0f);
        int initialSamples = Mathf.CeilToInt(maxDistance / sampleSpacing);
        Vector3 backward = GetBackwardDirection();

        for (int i = initialSamples; i >= 0; i--)
        {
            float distance = -i * sampleSpacing;
            Vector3 position = head.position + backward * -distance;
            path.Add(new PathPoint(position, distance));
        }

        UpdateSegments();
    }

    // Registra el recorrido de la cabeza.
    private void RecordHeadMovement()
    {
        Vector3 currentPosition = head.position;
        Vector3 movement = currentPosition - lastHeadPosition;
        float movementDistance = movement.magnitude;

        if (movementDistance < 0.001f) return;

        Vector3 direction = movement / movementDistance;
        float distanceInFrame = 0f;

        // Genera todas las muestras necesarias en el frame actual.
        while (movementDistance - distanceInFrame >= distanceToNextSample)
        {
            distanceInFrame += distanceToNextSample;
            Vector3 samplePosition = lastHeadPosition + direction * distanceInFrame;
            path.Add(new PathPoint(samplePosition, distanceTravelled + distanceInFrame));

            distanceToNextSample = sampleSpacing;
        }

        // Conserva la distancia que falta para la siguiente muestra.
        distanceToNextSample -= movementDistance - distanceInFrame;
        distanceTravelled += movementDistance;
        lastHeadPosition = currentPosition;
    }

    // Actualiza la posicion y rotacion de todos los segmentos.

    private void UpdateSegments()
    {
        float accumulatedSpacing = 0f;

        for (int i = 0; i < segments.Count; i++)
        {
            Transform segment = segments[i];
            if (segment == null) continue;

            // Distancia especial entre cabeza y primer segmento.
            if (i == 0)
            {
                accumulatedSpacing += headToFirstSegmentSpacing;
            }
            // Distancia especial entre los dos últimos segmentos.
            else if (i == segments.Count - 1)
            {
                accumulatedSpacing += lastSegmentSpacing;
            }
            // Distancia normal entre segmentos.
            else
            {
                accumulatedSpacing += segmentSpacing;
            }

            float targetDistance = distanceTravelled - accumulatedSpacing;

            segment.position = GetPositionAtDistance(targetDistance);
            UpdateSegmentRotation(segment, targetDistance);
        }
    }


    // Busca una posicion usando busqueda binaria.
    private Vector3 GetPositionAtDistance(float targetDistance)
    {
        if (path.Count == 0) return head.position;

        PathPoint firstPoint = path[firstValidIndex];
        if (targetDistance <= firstPoint.distance) return firstPoint.position;

        int lastIndex = path.Count - 1;
        PathPoint lastPoint = path[lastIndex];

        // Interpolacion entre el ultimo punto y la cabeza.
        if (targetDistance >= lastPoint.distance)
        {
            float distance = distanceTravelled - lastPoint.distance;
            if (distance <= 0.001f) return head.position;

            float t = (targetDistance - lastPoint.distance) / distance;
            return Vector3.Lerp(lastPoint.position, head.position, Mathf.Clamp01(t));
        }

        // Busqueda binaria entre los puntos del historial.
        int low = firstValidIndex;
        int high = lastIndex;

        while (low <= high)
        {
            int middle = (low + high) / 2;
            if (path[middle].distance < targetDistance)
                low = middle + 1;
            else
                high = middle - 1;
        }

        // high y low son los dos puntos que rodean la distancia.
        PathPoint previous = path[high];
        PathPoint next = path[low];

        float interpolation = Mathf.InverseLerp(previous.distance, next.distance, targetDistance);
        return Vector3.Lerp(previous.position, next.position, interpolation);
    }

    // Orienta el segmento siguiendo la direccion del recorrido.
    private void UpdateSegmentRotation(Transform segment, float targetDistance)
    {
        Vector3 ahead = GetPositionAtDistance(targetDistance + rotationLookDistance);
        Vector3 behind = GetPositionAtDistance(targetDistance - rotationLookDistance);
        Vector3 direction = ahead - behind;

        if (direction.sqrMagnitude < 0.0001f) return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        segment.rotation = Quaternion.Euler(0f, 0f, angle - spriteAxisAngle);
    }

    // Elimina puntos antiguos sin desplazar la lista cada frame.
    private void CleanHistory()
    {
        float maxDistance = headToFirstSegmentSpacing                    + Mathf.Max(0, segments.Count - 2) * segmentSpacing                    + (segments.Count > 1 ? lastSegmentSpacing : 0f)                    + rotationLookDistance;
        float minimumDistance = distanceTravelled - maxDistance - sampleSpacing * 2f;

        while (firstValidIndex + 1 < path.Count && path[firstValidIndex + 1].distance < minimumDistance)
        {
            firstValidIndex++;
        }

        // Compacta la lista solamente cuando sea necesario.
        if (firstValidIndex >= 128 && firstValidIndex * 2 >= path.Count)
        {
            path.RemoveRange(0, firstValidIndex);
            firstValidIndex = 0;
        }
    }

    // Obtiene la direccion contraria a la orientacion de la cabeza.
    private Vector3 GetBackwardDirection()
    {
        Vector3 forward = head.TransformDirection(new Vector3(localForwardAxis.x, localForwardAxis.y, 0f)).normalized;
        return -forward;
    }

    // Reinicia el historial.
    public void ResetFollow()
    {
        if (head == null) return;
        InitializeHistory();
    }

    private void OnValidate()
    {
        segmentSpacing = Mathf.Max(0.01f, segmentSpacing);
        sampleSpacing = Mathf.Max(0.01f, sampleSpacing);
        rotationLookDistance = Mathf.Max(0.01f, rotationLookDistance);
        headToFirstSegmentSpacing = Mathf.Max(0.01f, headToFirstSegmentSpacing);
        lastSegmentSpacing = Mathf.Max(0.01f, lastSegmentSpacing);
    }
}