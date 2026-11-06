using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public enum DayTime
{
    DAY,
    NIGHT
}

public class DayNightSystem : MonoBehaviour//, ASubject<Color>
{
    private LevelDefinition levelInfo;
    private DayTime daytime;

    [SerializeField] private bool _toggle;

    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("Global Volume")]
    [SerializeField] private Volume volume;

    [Header("Colores")]
    [SerializeField] private Color colorDia = Color.white;

    [SerializeField] private Color colorAtardecer = new Color(1f, 0.65f, 0.4f, 1f);

    [SerializeField] private Color colorNoche = new Color(0.3f, 0.35f, 0.65f, 1f);

    [Header("Amanecer")]
    [Range(0f, 1f)]
    [SerializeField] private float inicioAmanecer = 0.75f;

    [Header("Transición día / atardecer / noche")]
    [Range(0f, 1f)] [SerializeField] private float inicioAtardecer = 0.65f;
    [Range(0f, 1f)] [SerializeField] private float inicioNoche = 0.85f;

    private ColorAdjustments colorAdjustments;

    [Header("Particulas y efectos")]
    [SerializeField] private ParticleSystem[] particleSystems;

    public float _timer { get; private set; }

    public LevelDefinition GetLvlDef() => this.levelInfo;

    private void Awake()
    {
        // Obtener la definición seleccionada en SubLevelsScene.
        levelInfo = LevelSession.SelectedLevel;

        if (levelInfo == null)
        {
            Debug.LogError("DayNightSystem: No hay ningún LevelDefinition seleccionado en LevelSession.");
            enabled = false;
            return;
        }

        if (levelInfo.dayLength <= 0 || levelInfo.nightLength <= 0)
        {
            Debug.LogError($"DayNightSystem: La duración del día o de la noche no es válida en {levelInfo.Name}.");
            enabled = false;
            return;
        }

        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("DayNightSystem: No se ha encontrado el Animator.", this);
            enabled = false;
            return;
        }

        if (volume == null || volume.profile == null || !volume.profile.TryGet(out colorAdjustments))
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
            float t = Mathf.Clamp01(_timer / levelInfo.dayLength);

            if (t < inicioAtardecer)
            {
                // Día
                colorAdjustments.colorFilter.value = colorDia;
            }
            else if (t < inicioNoche)
            {
                // Atardecer
                float atardecerT = Mathf.InverseLerp(
                    inicioAtardecer,
                    inicioNoche,
                    t
                );

                colorAdjustments.colorFilter.value =
                    Color.Lerp(colorDia, colorAtardecer, atardecerT);
            }
            else
            {
                // Transición de atardecer a noche
                float nocheT = Mathf.InverseLerp(
                    inicioNoche,
                    1f,
                    t
                );

                colorAdjustments.colorFilter.value =
                    Color.Lerp(colorAtardecer, colorNoche, nocheT);
            }
        }
        else
        {
            // Durante la noche mantenemos el color nocturno
            float t = Mathf.Clamp01(_timer / levelInfo.nightLength);

            if (t < inicioAmanecer)
            {
                colorAdjustments.colorFilter.value = colorNoche;
            }
            else
            {
                float amanecerT = Mathf.InverseLerp(
                    inicioAmanecer,
                    1f,
                    t
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


        foreach(ParticleSystem p in particleSystems)
            p?.Play();
    }

    public void StartNight()
    {
        daytime = DayTime.NIGHT;
        _timer = 0f;

        colorAdjustments.colorFilter.value = colorNoche;

        SetVelocityAnim(1f / levelInfo.nightLength);

        foreach (ParticleSystem p in particleSystems)
            p?.Stop();
    }

    public void ResetVel()
    {
        SetVelocityAnim(1f);
    }
    /*
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
    */
    /*
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
    */
}