using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ConstructionBook : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject constructionPanel; // El panel que contiene el background
    [SerializeField] private Transform buildingButtonsContainer; // Donde se crearán los botones
    [SerializeField] private BuildingPlacer buildingPlacer;
    
    private bool isPanelVisible = false;

    [Header("Building Data")]
    [SerializeField] private List<BuildingData> availableBuildings = new List<BuildingData>();

    private Button constructorButton;

    private void Start()
    {
        Debug.Log("ConstructionBook: Iniciando...");
        
        // Obtener el botón del constructor-book (self)
        constructorButton = GetComponent<Button>();
        if (constructorButton == null)
        {
            Debug.LogError("ConstructionBook: ¡No se encontró el componente Button en constructor-book!");
            return;
        }
        
        // Verificar referencias
        if (constructionPanel == null)
        {
            Debug.LogError("ConstructionBook: ¡Construction Panel no está asignado!");
            return;
        }
        
        if (buildingButtonsContainer == null)
        {
            Debug.LogError("ConstructionBook: ¡Building Buttons Container no está asignado!");
            return;
        }
        
        if (buildingPlacer == null)
        {
            Debug.LogError("ConstructionBook: ¡Building Placer no está asignado!");
            return;
        }

        // Verificar componentes del panel
        if (constructionPanel.GetComponent<RectTransform>() == null)
        {
            Debug.LogError("ConstructionBook: ¡El Construction Panel no tiene un RectTransform!");
        }

        if (constructionPanel.GetComponent<CanvasGroup>() == null && 
            constructionPanel.GetComponent<Image>() == null)
        {
            Debug.LogWarning("ConstructionBook: El Construction Panel no tiene ni CanvasGroup ni Image - podría no ser visible");
        }

        // Ocultar el panel al inicio
        constructionPanel.SetActive(false);
        isPanelVisible = false;
        Debug.Log("ConstructionBook: Panel oculto al inicio");
        
        // Añadir listener al botón
        constructorButton.onClick.AddListener(ToggleConstructionBook);
        Debug.Log("ConstructionBook: Listener añadido al botón");
        
        // Crear botones para cada edificio
        CreateBuildingButtons();
    }

    public void ToggleConstructionBook()
    {
        Debug.Log("ConstructionBook: ToggleConstructionBook llamado");
        
        if (constructionPanel == null)
        {
            Debug.LogError("ConstructionBook: ¡Construction Panel es null!");
            return;
        }

        // Cambiar el estado del panel
        isPanelVisible = !isPanelVisible;
        constructionPanel.SetActive(isPanelVisible);
        
        Debug.Log($"ConstructionBook: Panel cambiado a {(isPanelVisible ? "visible" : "oculto")}");
    }

    private void CreateBuildingButtons()
    {
        if (availableBuildings == null || availableBuildings.Count == 0)
        {
            Debug.LogWarning("ConstructionBook: No hay edificios disponibles para crear botones");
            return;
        }

        Debug.Log($"ConstructionBook: Creando {availableBuildings.Count} botones de edificios");
        
        for (int i = 0; i < availableBuildings.Count; i++)
        {
            BuildingData building = availableBuildings[i];
            Debug.Log($"ConstructionBook: Procesando edificio #{i}: {building?.buildingName ?? "NULL DATA"}");

            if (building == null)
            {
                Debug.LogError($"ConstructionBook: ¡BuildingData en índice {i} es null!");
                continue;
            }

            if (building.buttonPrefab == null)
            {
                Debug.LogError($"ConstructionBook: ¡El buttonPrefab para {building.buildingName} (índice {i}) no está asignado en BuildingData! Asegúrate de asignarlo en el Inspector.");
                continue;
            }

            Debug.Log($"ConstructionBook: Instanciando prefab de botón '{building.buttonPrefab.name}' para {building.buildingName}");
            GameObject buttonObj = Instantiate(building.buttonPrefab, buildingButtonsContainer);
            
            if (buttonObj == null)
            {
                Debug.LogError($"ConstructionBook: ¡Error al instanciar el prefab de botón '{building.buttonPrefab.name}' para {building.buildingName}!");
                continue;
            }

            BuildingButtonUI buttonUI = buttonObj.GetComponent<BuildingButtonUI>();
            
            if (buttonUI != null)
            {
                Debug.Log($"ConstructionBook: Encontrado BuildingButtonUI en el prefab instanciado para {building.buildingName}. Inicializando...");
                buttonUI.Initialize(building, buildingPlacer);
                Debug.Log($"ConstructionBook: Botón creado e inicializado para {building.buildingName}");
            }
            else
            {   
                // Log detallado del objeto instanciado y sus componentes
                Debug.LogError($"ConstructionBook: ¡No se encontró BuildingButtonUI en el prefab instanciado '{buttonObj.name}' (original: {building.buttonPrefab.name}) para {building.buildingName}! Verificando componentes...");
                Component[] components = buttonObj.GetComponents<Component>();
                foreach (Component comp in components)
                {
                    Debug.LogError($"  - Componente encontrado: {comp.GetType().Name}");
                }
            }
        }
    }
} 