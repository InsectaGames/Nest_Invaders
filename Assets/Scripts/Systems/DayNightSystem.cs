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

    public float _timer { get; private set; }

    [SerializeField] private Animator animator;

    private void Awake()
    {
        animator = this.GetComponent<Animator>();
    }
    private void Start()
    {

     
        gameBG = GameObject.Find("BG").GetComponent<SpriteRenderer>();

        StartDay();
       
    }

    


private void SetVelocityAnim(float velocity)
    {
            animator.speed = velocity;
    }

    public void StartDay()
    {
        SetVelocityAnim((float)1/levelInfo.dayLength);
    }


    // la state machine es todo lambda , asi que no hace falta percepciones , todo ciclico

    public void StartNight()
    {
        SetVelocityAnim(1/levelInfo.nightLength);
    }

  



    public LevelDefinition getLvlDef() { return this.levelInfo; }
}