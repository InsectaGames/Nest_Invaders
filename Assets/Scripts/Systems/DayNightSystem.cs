using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum DayTime
{
    DAY,
    NIGHT
}

public class DayNightSystem : MonoBehaviour
{
    [SerializeField] private LevelDefinition levelInfo;

    private SpriteRenderer gameBG;
    [SerializeField] private Color nightColour;
    [SerializeField] private Animator animator;

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
    }

    private void SetVelocityAnim(float velocity) => animator.speed = velocity;

    // La state machine es todo lambda, asi que no hacen falta percepciones, todo ciclico.
    public void StartDay() => SetVelocityAnim((float) 1 / levelInfo.dayLength);
    
    public void StartNight() => SetVelocityAnim((float) 1 / levelInfo.nightLength);
}