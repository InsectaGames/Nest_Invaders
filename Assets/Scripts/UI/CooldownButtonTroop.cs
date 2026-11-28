using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ButtonCooldown : MonoBehaviour, IObserver<GameEvent<int>> , ISubject<GameEvent<int>>
{
    [Header("Configuración")]
    [SerializeField] private Button button;
    [SerializeField] private Image cooldownImage;
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

            cooldownImage.fillAmount = 1f - (elapsedTime / cooldownTime);

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

        if (LevelSession.SelectedLevel._tutorial && data.data == 1)
         UpdateObservers(new GameEvent<int>(LogicEvent.TUTORIAL_INCREMENT, 0));


        if (data.logicEvent == LogicEvent.NOT_ENOUGH_RESOURCES)
        {
            Debug.Log("Error");
            cooldownImage.fillAmount = 0f;
            _anim.ResetTrigger("pressed");
            _anim.SetTrigger("not_enough_resources");
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

            _anim.ResetTrigger("not_enough_resources");
            _anim.SetTrigger("pressed");  
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