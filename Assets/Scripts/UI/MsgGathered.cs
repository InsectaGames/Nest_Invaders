
using TMPro;
using UnityEngine;

public class MsgGathered : MonoBehaviour, IPoolable
{
    [SerializeField] private TMP_Text countText;
    [SerializeField] private Animator animator;

    public void RecieveQuantity(int quantity = 0)
    {
        countText.text = "+ " + quantity;

        if (animator == null)
            return;

        int stateHash = Animator.StringToHash("msgGatheredFungus");

        if (animator.HasState(0, stateHash))
            animator.Play(stateHash, 0, 0f);
        else
            Debug.LogError($"No existe el estado 'Gathered' en el Animator de {name}.", animator);
    }

    private System.Action<MsgGathered> onFinished;

    public void Initialize(System.Action<MsgGathered> callback)
    {
        onFinished = callback;
    }

    public void AnimationFinished()
    {
        onFinished?.Invoke(this);
    }
    public void OnSpawn()
    {
        gameObject.SetActive(true);
    }

    public void OnDespawn()
    {
        gameObject.SetActive(false);
    }

    public void ResetState()
    {
        countText.text = "";
    }
}