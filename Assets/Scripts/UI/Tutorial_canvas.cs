
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial_canvas : MonoBehaviour, IObserver<GameEvent<int>>
{
    [Header("Tutorial")]
    [SerializeField] public Sprite[] tutorial_but_sprites;
    [SerializeField] public Button but;

    [Header("Canvas")]
    [SerializeField] private GraphicRaycaster graphicRaycaster;

    [Header("Timing")]
    [SerializeField] private float initialDelay = 2f;
    [SerializeField] private float stepDelay = 0.5f;
    [SerializeField] private float fadeDuration = 0.5f;

    private int tutorialSteps;
    private bool canContinue;
    private bool waitingForEvent;

    // Pasos que dependen de otro botón del juego
    private readonly int[] observerSteps = { 4 ,6};
    private int observerEventsReceived;
    private const int MAX_OBSERVER_EVENTS = 2;

    public void UpdateObserver(GameEvent<int> data)
    {
        if (data.logicEvent != LogicEvent.TUTORIAL_INCREMENT)
            return;

        if (!waitingForEvent)
            return;

        if (observerEventsReceived >= MAX_OBSERVER_EVENTS)
            return;

        observerEventsReceived++;

        tutorialSteps++;
        waitingForEvent = false;

        Debug.Log($"Tutorial Observer: {observerEventsReceived}/{MAX_OBSERVER_EVENTS}");
    }

    public void OnTutorialButtonPressed()
    {
        if (!canContinue)
            return;

        canContinue = false;

        // Comprobar si el paso actual depende de un evento externo
        if (System.Array.IndexOf(observerSteps, tutorialSteps) >= 0)
        {
            waitingForEvent = true;
        }
        else
        {
            tutorialSteps++;
        }
    }

    void Start()
    {
        gameObject.SetActive(LevelSession.SelectedLevel._tutorial);

        if (!LevelSession.SelectedLevel._tutorial)
            return;

        Color color = but.image.color;
        color.a = 0f;
        but.image.color = color;

        but.interactable = false;
        graphicRaycaster.enabled = false;

        StartCoroutine(TutorialRoutine());
    }


    IEnumerator TutorialRoutine()
    {
        yield return new WaitForSecondsRealtime(initialDelay);

        while (tutorialSteps < tutorial_but_sprites.Length)
        {
            // El juego se pausa al comenzar cada explicación
            Time.timeScale = 0f;

            yield return new WaitForSecondsRealtime(stepDelay);

            // Preparar nuevo sprite
            but.image.sprite = tutorial_but_sprites[tutorialSteps];

            Debug.Log("Nuevo sprite de turotial" + tutorial_but_sprites[tutorialSteps].name);

            Color color = but.image.color;
            color.a = 0f;
            but.image.color = color;

            // Activar interacción
            graphicRaycaster.enabled = true;
            but.interactable = true;

            // FADE IN
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;

                color.a = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                but.image.color = color;

                yield return null;
            }

            color.a = 1f;
            but.image.color = color;

            canContinue = true;

            // Esperar pulsación
            yield return new WaitUntil(() => !canContinue);

            but.interactable = false;

            // FADE OUT
            elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;

                color.a = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                but.image.color = color;

                yield return null;
            }

            color.a = 0f;
            but.image.color = color;

            // Desactivar interacción
            graphicRaycaster.enabled = false;

            // Reanudar juego entre explicaciones
            Time.timeScale = 1f;

           

            // Si depende de un evento externo, esperar a que ocurra
            if (waitingForEvent)
            {
                yield return new WaitUntil(() => !waitingForEvent);

                yield return new WaitForSeconds(3f);

            }
            else
            {
                yield return new WaitForSeconds(stepDelay);

            }

           
        }

        // Finalizar tutorial
        Time.timeScale = 1f;
        graphicRaycaster.enabled = false;
        gameObject.SetActive(false);
    }


    void OnDisable()
    {
        Time.timeScale = 1f;
    }
}
