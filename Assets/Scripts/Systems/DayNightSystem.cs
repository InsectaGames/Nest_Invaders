using System.Collections;
using UnityEngine;

public enum DayTime
{
    DAY,
    NIGHT
}

public class DayNightSystem : ASubject<Color>
{
    [SerializeField] private LevelDefinition levelInfo;

    private SpriteRenderer gameBG;
    [SerializeField] private Color nightColour;
    [SerializeField] private Animator animator;

    public bool _toggle;

    public float _timer { get; private set; }

    public LevelDefinition GetLvlDef() => this.levelInfo;

    private void Awake()
    {
        animator = this.GetComponent<Animator>();
    }

    private void Start()
    {
        gameBG = GameObject.Find("BG").GetComponent<SpriteRenderer>();
        StartDay();
        
        StartCoroutine(DNChange());
    }
    private void Update()
    {
        // DEBUG ONLY.
        if (Utils.KeyPressed(UnityEngine.InputSystem.Key.N))
        {
            _toggle = !_toggle;
        }
    }

    private void SetVelocityAnim(float velocity) => animator.speed = velocity;

    // La state machine es todo lambda, asi que no hacen falta percepciones, todo ciclico.
    public void StartDay()
    {
        SetVelocityAnim((float)1 / levelInfo.dayLength);
    }

    public void StartNight()
    { 
        SetVelocityAnim((float) 1 / levelInfo.nightLength); 
    }

    private IEnumerator DNChange()
    {
        // yield return null;

        while (true)
        { 
            DebugToggleNightDay(_toggle);
            yield return new WaitForSeconds(1f);
        }
    }

    public void DebugToggleNightDay(bool _toggleDN)
    {
        if (!_toggleDN)
        {
            Debug.Log("De dia");
            UpdateObservers(new Color(1,1,1));
        }
        else
        {
            Debug.Log("De noche");
            UpdateObservers(new Color(
                77f / 255f,
                122f / 255f,
                143f / 255f,
                1f
            ));
        }
    }
}