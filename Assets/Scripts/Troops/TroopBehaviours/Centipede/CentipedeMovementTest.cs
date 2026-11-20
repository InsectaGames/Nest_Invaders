using UnityEngine;

public class CentipedeMovementTest : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform head;

    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 2f;

    [SerializeField] private float turnSpeed = 90f;

    [Header("Giros")]
    [SerializeField] private float turnDuration = 1.5f;

    private float turnTimer;
    private int turnDirection = 1;

    private void Update()
    {
        if (head == null) return;

        MoveHead();
        HandleTurning();
    }

    private void MoveHead()
    {
        head.position += head.right * moveSpeed * Time.deltaTime;
    }

    private void HandleTurning()
    {
        turnTimer += Time.deltaTime;

        if (turnTimer >= turnDuration)
        {
            turnTimer = 0f;
            turnDirection *= -1;
        }

        head.Rotate(0f, 0f, turnSpeed * turnDirection * Time.deltaTime);
    }
}