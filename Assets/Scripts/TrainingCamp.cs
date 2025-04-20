using UnityEngine;

public class TrainingCamp : MonoBehaviour
{
    [SerializeField] private GameObject unitCreationPanel;
    [SerializeField] private GameObject backgroundPanel;
    [SerializeField] private Transform horizontalPanel; // Referencia al panel horizontal
    [SerializeField] private GameObject unitCardPrefab; // Prefab de la carta de unidad
    [SerializeField] private UnitData[] availableUnits; // Array de unidades disponibles

    private void Start()
    {
        // Aseguramos que tanto el panel como el fondo estén ocultos al inicio
        if (unitCreationPanel != null)
        {
            unitCreationPanel.SetActive(false);
        }
        
        if (backgroundPanel != null)
        {
            backgroundPanel.SetActive(false);
        }

        // Crear las cartas de unidades
        CreateUnitCards();
    }

    private void CreateUnitCards()
    {
        if (horizontalPanel == null || unitCardPrefab == null || availableUnits == null)
        {
            Debug.LogWarning("Faltan referencias necesarias para crear las cartas de unidades");
            return;
        }

        // Crear una carta para cada unidad disponible
        foreach (UnitData unitData in availableUnits)
        {
            GameObject cardObject = Instantiate(unitCardPrefab, horizontalPanel);
            UnitCardUI cardUI = cardObject.GetComponent<UnitCardUI>();
            if (cardUI != null)
            {
                cardUI.ConfigureUnitCard(unitData);
            }
        }
    }

    void OnMouseDown()
    {
        ShowUnitPanel();
    }

    private void ShowUnitPanel()
    {
        if (unitCreationPanel != null && backgroundPanel != null)
        {
            unitCreationPanel.SetActive(true);
            backgroundPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("¡El panel de creación de unidades o el background no están asignados en el Inspector!");
        }
    }

    public void ExitUnitPanel()
    {
        if (unitCreationPanel != null && backgroundPanel != null)
        {
            unitCreationPanel.SetActive(false);
            backgroundPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("¡El panel de creación de unidades o el background no están asignados en el Inspector!");
        }
    }
} 