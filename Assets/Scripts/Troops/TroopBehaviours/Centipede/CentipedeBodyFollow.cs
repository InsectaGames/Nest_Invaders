using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[DefaultExecutionOrder(-1000)]
public class CentipedeBodyFollow : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform head;
    [SerializeField] private List<Transform> segments = new List<Transform>();

    [Header("Generación de segmentos")]
    [SerializeField] private bool generateSegmentsOnStart;
    [SerializeField] private Transform bodySegmentPrefab;
    [SerializeField] private Transform tailPrefab;
    [SerializeField, Min(0)] private int bodySegmentCount = 4;
    [SerializeField] private Transform segmentsParent;

    private int activeBodySegmentCount = -1;
    public int BodySegmentCount => Mathf.Max(0, segments.Count - 1);

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

    [Header("NavmeshAgent")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private TroopDefinition definition;

    [Header("Animación")]
    [SerializeField] private Animator headAnimator;

    private Animator[] segmentAnimators;
    private bool isMoving;

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

    private void Awake()
    {
        if (head == null)
            return;

        agent = head.gameObject.GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
        }
    }

    private void Start()
    {
        if (head == null)
        {
            Debug.LogError("CentipedeBodyFollow: No se ha asignado Head.", this);
            enabled = false;
            return;
        }

        CreateSegments();
        InitializeAnimations();

        CacheSpriteAxis();
        InitializeHistory();

        if (definition == null && head.TryGetComponent<ATroop>(out ATroop headTroop))
            definition = headTroop.GetDefinition();

        if (agent != null)
        {
            if (!agent.enabled)
                agent.enabled = true;

            agent.speed = definition.speed;
            agent.isStopped = false;

            if (!agent.isOnNavMesh)
            {
                Debug.LogWarning($"{name}: el agente no está situado sobre el NavMesh.");
                return;
            }

            // agent.ResetPath();
        }

    }

    private void LateUpdate()
    {
        RecordHeadMovement();
        UpdateSegments();
        CleanHistory();

        FixHeadRotation();
        UpdateAnimationState();
    }

    private void FixHeadRotation()
    {
        UpdateSegmentRotation(head, distanceTravelled);
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
        if (segments.Count == 0)
            return;

        int bodyCount = activeBodySegmentCount < 0 ? BodySegmentCount : activeBodySegmentCount;

        float accumulatedSpacing = 0f;

        // Actualizar los segmentos intermedios activos.
        for (int i = 0; i < bodyCount; i++)
        {
            Transform segment = segments[i];

            if (segment == null)
                continue;

            accumulatedSpacing += i == 0 ? headToFirstSegmentSpacing : segmentSpacing;

            float targetDistance = distanceTravelled - accumulatedSpacing;

            segment.position = GetPositionAtDistance(targetDistance);
            UpdateSegmentRotation(segment, targetDistance);
        }

        // Actualizar la cola, que siempre es el último elemento.
        Transform tail = segments[segments.Count - 1];

        if (tail == null)
            return;

        float tailSpacing = bodyCount > 0 ? lastSegmentSpacing : headToFirstSegmentSpacing;

        accumulatedSpacing += tailSpacing;

        float tailDistance = distanceTravelled - accumulatedSpacing;

        tail.position = GetPositionAtDistance(tailDistance);
        UpdateSegmentRotation(tail, tailDistance);
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

    // Para la animación de los segmentos.
    private void UpdateAnimationState()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        bool moving = agent.velocity.sqrMagnitude > 0.01f;

        if (moving == isMoving)
            return;

        isMoving = moving;

        if (headAnimator != null)
            headAnimator.speed = isMoving ? 1f : 0f;

        if (segmentAnimators == null)
            return;

        foreach (Animator animator in segmentAnimators)
        {
            if (animator != null)
                animator.speed = isMoving ? 1f : 0f;
        }
    }

    // Elimina puntos antiguos sin desplazar la lista cada frame.
    private void CleanHistory()
    {
        int bodyCount = activeBodySegmentCount < 0 ? BodySegmentCount : activeBodySegmentCount;
        
        float maxDistance = bodyCount > 0 ? headToFirstSegmentSpacing + (bodyCount - 1) * segmentSpacing + lastSegmentSpacing : headToFirstSegmentSpacing;
        maxDistance += rotationLookDistance;
        
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

    // Crear segmentos de forma dinámica.
    private void CreateSegments()
    {
        if (!generateSegmentsOnStart)
            return;

        if (bodySegmentPrefab == null || tailPrefab == null)
        {
            Debug.LogError("CentipedeBodyFollow: Asigna el prefab del cuerpo y el de la cola.", this);
            return;
        }

        segments.Clear();

        Transform parent = segmentsParent != null ? segmentsParent : transform;

        // Crear los segmentos del cuerpo.
        for (int i = 0; i < bodySegmentCount; i++)
        {
            Transform segment = Instantiate(bodySegmentPrefab, head.position, head.rotation, parent);

            segments.Add(segment);
        }

        // La cola siempre es el último elemento de la lista.
        Transform tail = Instantiate(tailPrefab, head.position, head.rotation, parent);

        segments.Add(tail);
    }

    // Inicializar los Animator.
    private void InitializeAnimations()
    {
        // Obtener el Animator de la cabeza si no está asignado.
        if (headAnimator == null && head != null)
            headAnimator = head.GetComponent<Animator>();

        segmentAnimators = new Animator[segments.Count];

        for (int i = 0; i < segments.Count; i++)
        {
            if (segments[i] == null)
                continue;

            Animator animator = segments[i].GetComponent<Animator>();
            segmentAnimators[i] = animator;

            if (animator == null)
                continue;

            // La cola siempre ocupa la última posición.
            bool isTail = i == segments.Count - 1;

            // Alternar la fase de los segmentos del cuerpo.
            if (!isTail && i % 2 == 1)
                animator.Play(0, 0, 0.5f);
            else
                animator.Play(0, 0, 0f);
        }

        if (headAnimator != null)
            headAnimator.Play(0, 0, 0f);
    }

    // Activar y desactivar segmentos según la vida del mini jefe.
    public void SetActiveBodySegmentCount(int count)
    {
        int newCount = Mathf.Clamp(count, 0, BodySegmentCount);

        if (newCount == activeBodySegmentCount)
            return;

        activeBodySegmentCount = newCount;

        // Activar los segmentos necesarios y ocultar los restantes.
        for (int i = 0; i < BodySegmentCount; i++)
        {
            if (segments[i] != null)
                segments[i].gameObject.SetActive(i < activeBodySegmentCount);
        }

        // La cola siempre permanece activa.
        Transform tail = segments[segments.Count - 1];

        if (tail != null)
            tail.gameObject.SetActive(true);

        UpdateSegments();
    }
}