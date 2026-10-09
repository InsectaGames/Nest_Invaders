
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
    [Header("Configuración")]
    [SerializeField] private float timeScale = 1f;

    public DayTime _dayTime { get; private set; } = DayTime.DAY;

    private List<DayTime> phaseOrder = new List<DayTime>()
    {
        DayTime.DAY,
        DayTime.NIGHT
    };

    [SerializeField] private LevelDefinition levelInfo;

    private SpriteRenderer gameBG;
    private Color defaultColour;

    [SerializeField] private Color nightColour;

    public float _timer { get; private set; }

    [SerializeField] private Image fillTime;
    [SerializeField] private Image fillTimeBG;
    public Color imageFillBGColor;
   public Color imageFillColor;

    private void Start()
    {

        imageFillBGColor = fillTimeBG.color;
        imageFillColor = fillTime.color;
        gameBG = GameObject.Find("BG").GetComponent<SpriteRenderer>();
        defaultColour = gameBG.color;

        StartDay();
        UpdateFillTime();
    }

    private void Update()
    {
        _timer -= timeScale * Time.deltaTime;

        if (_timer <= 0f)
        {
            UpdateCycle();
        }

        UpdateFillTime();
    }


 private void UpdateFillTime() { 
        
        float phaseLength = _dayTime == DayTime.DAY ? levelInfo.dayLength : levelInfo.nightLength; 
        if (phaseLength > 0f) { 
            fillTime.fillAmount = Mathf.Clamp01(_timer / phaseLength); 
        } else { 
            fillTime.fillAmount = 0f; 
        }
    }

    public void StartDay()
    {
        gameBG.color = defaultColour;
        fillTime.color = imageFillColor;
        fillTimeBG.color = imageFillBGColor;
        _timer = levelInfo.dayLength;
    }

    public void StartNight()
    {
        gameBG.color = nightColour;
        fillTime.color = imageFillBGColor;
        fillTimeBG.color = imageFillColor;
        _timer = levelInfo.nightLength;
    }

    public void UpdateCycle()
    {
        if (_timer <= 0f)
        {
            int currentIndex = phaseOrder.IndexOf(_dayTime);

            _dayTime = phaseOrder[
                (currentIndex + 1) % phaseOrder.Count
            ];

            if (_dayTime == DayTime.DAY)
                StartDay();
            else
                StartNight();
        }
    }

    public void ForcePhase(DayTime phase)
    {
        _dayTime = phase;

        if (_dayTime == DayTime.DAY)
            StartDay();
        else
            StartNight();

        UpdateFillTime();
    }
}