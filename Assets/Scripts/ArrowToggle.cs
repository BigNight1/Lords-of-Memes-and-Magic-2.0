using UnityEngine;
using UnityEngine.UI;

public class ArrowToggle : MonoBehaviour
{
    public GameObject optionsPanel; // Asigna el Panel de Opciones desde el Inspector
    private bool isPanelVisible = false; // Estado del panel

    public AudioSource audioSource; // Asigna el AudioSource desde el Inspector

    void Start()
    {
        // Asegúrate de que el panel esté oculto al inicio
        optionsPanel.SetActive(false);
    }

    public void ToggleOptionsPanel()
    {
        isPanelVisible = !isPanelVisible; // Cambiar el estado
        optionsPanel.SetActive(isPanelVisible); // Mostrar u ocultar el panel
        // Reproducir el sonido al hacer clic
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }
}