using UnityEngine;
using UnityEngine.UI;

public class TimeBar : MonoBehaviour
{
    public Image foreground; // Referencia al Image del Foreground
    public bool debugMode = true; // Activar/desactivar mensajes de debug
    public Image background; // Referencia al Background
    
    private float fillAmount = 0f; // Cantidad de llenado actual
    private float totalTime; // Tiempo total para la recolección
    private float elapsedTime = 0f; // Tiempo transcurrido
    private bool isTimerActive = false; // Para controlar si el temporizador está activo
    private Canvas canvas; // Referencia al componente Canvas
    private RectTransform rectTransform;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
        }
        
        if (background == null)
        {
            background = transform.Find("Background")?.GetComponent<Image>();
        }

        if (background != null)
        {
            background.gameObject.SetActive(true);
        }

        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogError("No se encontró RectTransform en el TimeBar");
        }
    }

    private void Start()
    {
        CheckComponents();
        
        // Asegurarse de que la barra esté oculta al inicio
        HideTimeBar();
    }

    private void CheckComponents()
    {
        if (background == null)
        {
            Debug.LogError("El Image del Background no está asignado en el TimeBar");
        }

        if (foreground == null)
        {
            Debug.LogError("El Image del Foreground no está asignado en el TimeBar");
        }

        if (canvas == null)
        {
            Debug.LogError("No se encontró un Canvas para el TimeBar");
        }
    }

    public void StartTimer(float time)
    {
        if (time <= 0)
        {
            Debug.LogError("¡ATENCIÓN! Tiempo inválido para el temporizador");
            return;
        }
        
        ShowTimeBar();
        
        totalTime = time;
        elapsedTime = 0f;
        fillAmount = 0f;
        
        if (foreground != null)
        {
            foreground.gameObject.SetActive(true);
            foreground.fillAmount = fillAmount;
        }
        
        isTimerActive = true;
    }

    private void Update()
    {
        if (isTimerActive && elapsedTime < totalTime)
        {
            elapsedTime += Time.deltaTime;
            fillAmount = elapsedTime / totalTime;
            
            if (foreground != null)
            {
                if (!foreground.gameObject.activeSelf)
                {
                    foreground.gameObject.SetActive(true);
                }
                foreground.fillAmount = fillAmount;
            }

            if (fillAmount >= 1f)
            {
                isTimerActive = false;
                HideTimeBar();
            }
        }
    }
    
    public void StopTimer()
    {
        isTimerActive = false;
        HideTimeBar();
    }

    public void ShowTimeBar()
    {
        if (background != null)
        {
            background.gameObject.SetActive(true);
        }

        if (foreground != null)
        {
            foreground.gameObject.SetActive(true);
            foreground.enabled = true;
        }

        if (canvas != null)
        {
            canvas.enabled = true;
        }
    }

    public void HideTimeBar()
    {
        if (foreground != null)
        {
            foreground.gameObject.SetActive(false);
        }

        if (canvas != null)
        {
            canvas.enabled = true;
        }
    }

    public bool IsVisible()
    {
        bool isVisible = gameObject.activeSelf && (canvas == null || canvas.enabled);
        return isVisible;
    }

    // Método para forzar la activación del TimeBar (útil para debugging)
    public void ForceActivate()
    {
        gameObject.SetActive(true);
        if (canvas != null) canvas.enabled = true;
        if (foreground != null) foreground.enabled = true;
        CheckComponents();
    }
}