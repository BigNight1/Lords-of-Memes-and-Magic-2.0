using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Referencias de Texto")]
    public TextMeshProUGUI woodText; // Asigna el Text para madera desde el Inspector
    public TextMeshProUGUI  stoneText; // Asigna el Text para piedra desde el Inspector
    public TextMeshProUGUI foodText; // Asigna el Text para comida desde el Inspector
    public TextMeshProUGUI populationText; // Nuevo texto para población

    [Header("Configuración de Actualización")]
    public float intervaloActualizacion = 0.5f; // Actualizar cada medio segundo
    private float tiempoUltimaActualizacion;

    void Start()
    {
        UpdateResourceUI(); // Actualiza la UI al inicio
        tiempoUltimaActualizacion = Time.time;
    }

    void Update()
    {
        // Actualizar la UI cada intervaloActualizacion segundos
        if (Time.time - tiempoUltimaActualizacion >= intervaloActualizacion)
        {
            UpdateResourceUI();
            tiempoUltimaActualizacion = Time.time;
        }
    }

    public void UpdateResourceUI()
    {
        if (Inventory.Instance != null)
        {
            woodText.text = Inventory.Instance.wood.ToString();
            stoneText.text = Inventory.Instance.stone.ToString();
            foodText.text = Inventory.Instance.food.ToString();
        }

        if (PopulationManager.Instance != null)
        {
            populationText.text = PopulationManager.Instance.GetPoblacionAll();
        }
    }
}
