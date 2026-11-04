using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DayNightCycleVolume : MonoBehaviour
{
    [Header("Global Volume")]
    [SerializeField] private Volume volume;

    [Header("Colores")]
    [SerializeField] private Color dia = Color.white;
    [SerializeField] private Color atardecer = new Color(1f, 0.65f, 0.4f);
    [SerializeField] private Color noche = new Color(0.3f, 0.35f, 0.65f);

    [Header("Duración de cada fase")]
    [SerializeField] private float duracionFase = 20f;

    private ColorAdjustments colorAdjustments;
    private float tiempo;

    private void Awake()
    {
        volume =this.GetComponent<Volume>();

        if (volume == null || !volume.profile.TryGet(out colorAdjustments))
        {
            Debug.LogError("No se encontró Color Adjustments.");
            enabled = false;
            return;
        }

        colorAdjustments.colorFilter.overrideState = true;
    }

    private void Update()
    {
        tiempo += Time.deltaTime;

        float ciclo = tiempo / duracionFase;
        float fase = Mathf.PingPong(ciclo, 2f);

        Color colorActual;

        if (fase < 1f)
        {
            colorActual = Color.Lerp(dia, atardecer, fase);
        }
        else
        {
            colorActual = Color.Lerp(atardecer, noche, fase - 1f);
        }

        colorAdjustments.colorFilter.value = colorActual;
    }
}