using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class ButtonCooldown : MonoBehaviour, IObserver<GameEvent<int>> , ISubject<GameEvent<int>>
{
    [Header("Configuración")]
    [SerializeField] private Button button;
    [SerializeField] private Image cooldownImage;

    [SerializeField] private Image troopImg;
    [SerializeField] private TMP_Text cost_text;

    [SerializeField] private float cooldownTime = 3f;
    [SerializeField] private int buttonID;

    private bool isOnCooldown = false;
    public bool OnCoolDown => isOnCooldown;

    private Animator _anim;

    private List<IObserver<GameEvent<int>>> _observers = new List<IObserver<GameEvent<int>>>();

    private void Start()
    {
        _anim = GetComponent<Animator>();
        button = this.GetComponent<Button>();

        GameObject.Find("UI_Controller").GetComponent<UITroopsGenerator>().AddObserver(this);

        if (LevelSession.SelectedLevel._tutorial)
            AddObserver(GameObject.FindGameObjectWithTag("tutorial").GetComponent<Tutorial_canvas>());

        GameObject[] _go = GameObject.FindGameObjectsWithTag("Base");

        foreach (var go in _go)
        {
           if(go.GetComponent<BaseSystem>().GetFaction() == Faction.ALLY)
            {
                go.GetComponent<BaseSystem>().AddObserver(this);
                Debug.Log("Me suscribi a: " + go.name);
            }
        }

        cooldownImage.fillAmount = 0f;
        cooldownImage.gameObject.SetActive(false);
    }

    public void OnButtonPressed()
    {
        Debug.Log("ANIMATION EVENT EJECUTADO");

        if (isOnCooldown)
        {
            Debug.Log("BLOQUEADO POR COOLDOWN");
            return;
        }

        StartCoroutine(CooldownCoroutine());
    }

    private IEnumerator CooldownCoroutine()
    {

        Debug.Log("INICIO COOLDOWN");
        isOnCooldown = true;
        button.interactable = false;

        cooldownImage.gameObject.SetActive(true);

        float elapsedTime = 0f;

        while (elapsedTime < cooldownTime)
        {
            elapsedTime += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsedTime / cooldownTime);

            cooldownImage.fillAmount = 1f - progress;

            yield return null;
        }

        cooldownImage.fillAmount = 0f;
        cooldownImage.gameObject.SetActive(false);

        isOnCooldown = false;
        button.interactable = true;


        Debug.Log("FIN COOLDOWN");
    }


    private IEnumerator NotEnoughButtonAnim()
    {
        Color text_color = new Color(1f, (float)(155 / 255), (float)(155 / 255));


        troopImg.color = Color.red;
        button.image.color = Color.red;
        cost_text.color = text_color;

        float duration = 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            troopImg.color = Color.Lerp(Color.red, Color.white, elapsedTime / duration);
            button.image.color = Color.Lerp(Color.red, Color.white, elapsedTime / duration);
            cost_text.color = Color.Lerp(text_color, Color.white, elapsedTime / duration);

            yield return null;
        }

        troopImg.color = Color.white;
        button.image.color = Color.white;
        cost_text.color =    Color.white;
        yield return null;
    }

    public void PlayPressedAnimation()
    {
        if (isOnCooldown)
            return;

        _anim.SetTrigger("pressed");
    }


    public void UpdateObserver(GameEvent<int> data)
    {

        Debug.Log("EVENTO RECIBIDO: " + data.logicEvent);
        // Solo reaccionamos si el ID coincide con el botón.
        if (data.data != buttonID)
            return;

        if (LevelSession.SelectedLevel._tutorial && data.data == 1)
         UpdateObservers(new GameEvent<int>(LogicEvent.TUTORIAL_INCREMENT, 0));


        if (data.logicEvent == LogicEvent.NOT_ENOUGH_RESOURCES)
        {
            Debug.Log("Error");
            cooldownImage.fillAmount = 0f;
            _anim.ResetTrigger("pressed");
            StartCoroutine(NotEnoughButtonAnim());
            return;
        }
        else if(data.logicEvent == LogicEvent.TROOP_PLACED)
        {
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

            //_anim.ResetTrigger("not_enough_resources");
            PlayPressedAnimation(); 
        }
    }

    #region OBSERVER
    
    public void AddObserver(IObserver<GameEvent<int>> obs)
    {
        _observers.Add(obs);
    }

    public void RemoveObserver(IObserver<GameEvent<int>> obs)
    {
        _observers.Remove(obs);
    }

    public void UpdateObservers(GameEvent<int> data)
    {
        foreach (var obs in _observers)
        {
            obs.UpdateObserver(data);
        }
    }

    #endregion
}