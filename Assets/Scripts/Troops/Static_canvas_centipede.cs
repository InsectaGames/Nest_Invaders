using UnityEngine;

public class Static_canvas : MonoBehaviour
{
    [SerializeField] private Transform target;

    private Vector3 offset = new Vector3(-0.44f, 1.67f, 0f);

    private void LateUpdate()
    {
        if (target == null) return;

        transform.position = target.position + offset;
    }
}
