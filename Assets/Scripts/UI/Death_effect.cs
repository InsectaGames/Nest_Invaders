using UnityEngine;

public class Death_effect : MonoBehaviour
{
    private void Start()
    {
        Destroy(this, 1.5f);
    }
}