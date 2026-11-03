
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public enum DayTime
{
    DAY,
    NIGHT
}

public class DayNightSystem : ASubject<Color>
{
    [SerializeField] private LevelDefinition levelInfo;
    [SerializeField] private GameObject fade;

    private DayTime daytime;

    private SpriteRenderer gameBG;

    [SerializeField] private Animator animator;
    [SerializeField] private Animator animatorFade;

    public bool _toggle;

    [Header("Global Volume")]
    [SerializeField] private Volume volume;

    [Header("Colores")]
    [SerializeField] private Color colorDia = Color.white;

    [SerializeField]
    private Color colorAtardecer =
        new Color(1f, 0.65f, 0.4f, 1f);

    [SerializeField]
    private Color colorNoche =
        new Color(0.3f, 0.35f, 0.65f, 1f);

    [Header("Amanecer")]
    [Range(0f, 1f)]
    [SerializeField] private float inicioAmanecer = 0.75f;

    private ColorAdjustments colorAdjustments;

    public float _timer { get; private set; }

    public LevelDefinition GetLvlDef() => this.levelInfo;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animatorFade = fade.GetComponent<Animator>();

        if (volume == null ||
            !volume.profile.TryGet(out colorAdjustments))
        {
            Debug.LogError("No se encontró Color Adjustments en el Volume.");
            enabled = false;
            return;
        }

        colorAdjustments.colorFilter.overrideState = true;
        colorAdjustments.colorFilter.value = colorDia;
    }

    private void Start()
    {
        gameBG = GameObject.Find("BG").GetComponent<SpriteRenderer>();

        daytime = DayTime.DAY;
        StartDay();
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        UpdateVolumeColor();

        // DEBUG ONLY.
        if (Utils.KeyPressed(UnityEngine.InputSystem.Key.N))
        {
            _toggle = !_toggle;
        }
    }

    private void UpdateVolumeColor()
    {
        if (daytime == DayTime.DAY)
        {
            float t = Mathf.Clamp01(
                _timer / levelInfo.dayLength
            );

            colorAdjustments.colorFilter.value =
                Color.Lerp(colorDia, colorAtardecer, t);
        }
        else
        {
            float t = Mathf.Clamp01(
                _timer / levelInfo.nightLength
            );

            if (t < inicioAmanecer)
            {
                colorAdjustments.colorFilter.value = colorNoche;
            }
            else
            {
                float amanecerT = Mathf.InverseLerp(
                    inicioAmanecer, 1f, t
                );

                colorAdjustments.colorFilter.value =
                    Color.Lerp(colorNoche, colorDia, amanecerT);
            }
        }
    }

    private void SetVelocityAnim(float velocity)
    {
        animator.speed = velocity;
    }

    public void StartDay()
    {
        daytime = DayTime.DAY;
        _timer = 0f;

        colorAdjustments.colorFilter.value = colorDia;

        SetVelocityAnim(1f / levelInfo.dayLength);
    }

    public void StartNight()
    {
        daytime = DayTime.NIGHT;
        _timer = 0f;

        colorAdjustments.colorFilter.value = colorNoche;

        SetVelocityAnim(1f / levelInfo.nightLength);
    }

    public void CallFade()
    {
        animatorFade.SetTrigger("trans");
    }

    public void ResetVel()
    {
        SetVelocityAnim(1f);
    }

    public void AnimUpdateDay()
    {
        UpdateObservers(new Color(1, 1, 1));
    }

    public void AnimUpdateNight()
    {
        UpdateObservers(new Color(
            77f / 255f,
            122f / 255f,
            143f / 255f,
            1f
        ));
    }

    public void DebugToggleNightDay(bool _toggleDN)
    {
        if (!_toggleDN)
        {
            Debug.Log("De dia");
            UpdateObservers(Color.white);
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
