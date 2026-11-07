using UnityEngine;

public class LevelSelectionAnimator : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.enabled = false;
    }

    public void PlayUnlockAnimation(string stateName)
    {
        if (animator == null || string.IsNullOrEmpty(stateName))
            return;

        animator.enabled = true;

        animator.Rebind();
        animator.Update(0f);

        animator.Play(stateName, 0, 0f);
        animator.Update(0f);
    }
}