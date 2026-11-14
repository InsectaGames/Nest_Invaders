using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ButtonCooldown : MonoBehaviour, IObserver<GameEvent<int>>
{
    [Header("Configuración")]
    [SerializeField] private Button button;
    [SerializeField] private Image cooldownImage;
    [SerializeField] private float cooldownTime = 3f;
    [SerializeField] private int buttonID;

    private bool isOnCooldown = false;
    public bool OnCoolDown => isOnCooldown;

    private Animator _anim;

    private void Start()
    {
        _anim = GetComponent<Animator>();

        GameObject.Find("UI_Controller")
            .GetComponent<UITroopsGenerator>()
            .AddObserver(this);

        cooldownImage.fillAmount = 0f;
        cooldownImage.gameObject.SetActive(false);
    }

    public void OnButtonPressed()
    {
        if (!isOnCooldown)
        {
            StartCoroutine(CooldownCoroutine());
        }
    }

    private IEnumerator CooldownCoroutine()
    {
        isOnCooldown = true;

        button.interactable = false;
        cooldownImage.gameObject.SetActive(true);

        float elapsedTime = 0f;

        while (elapsedTime < cooldownTime)
        {
            elapsedTime += Time.deltaTime;

            cooldownImage.fillAmount =
                1f - (elapsedTime / cooldownTime);

            yield return null;
        }

        cooldownImage.fillAmount = 0f;
        cooldownImage.gameObject.SetActive(false);

        button.interactable = true;
        isOnCooldown = false;
    }

    public void UpdateObserver(GameEvent<int> data)
    {
        // Solo reaccionamos si el ID coincide con el botón.
        if (data.data != buttonID)
            return;

        switch (data.data)
        {
            case 0:
                cooldownTime = 2f;
                break;

            case 1:
                cooldownTime = 3.5f;
                break;

            case 2:
                cooldownTime = 2.5f;
                break;

            case 3:
                cooldownTime = 4f;
                break;
        }
        _anim.SetTrigger("pressed");
    }
}