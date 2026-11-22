using UnityEngine;

public class CentipedeHeadAnimationEvents : MonoBehaviour
{
    private SpringMiniBossTroop boss;

    private void Awake()
    {
        boss = GetComponentInParent<SpringMiniBossTroop>();
    }

    public void OnAreaAttackFinished()
    {
        if (boss != null)
            boss.FinishAreaAttackAnimation();
    }
}