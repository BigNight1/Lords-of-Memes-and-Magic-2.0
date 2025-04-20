using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class BuildingButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Image buildingIcon;
    [SerializeField] private TextMeshProUGUI buildingNameText;
    [SerializeField] private TextMeshProUGUI woodCostText;
    [SerializeField] private TextMeshProUGUI stoneCostText;
    
    private BuildingData buildingData;
    private BuildingPlacer buildingPlacer;
    private Button button;
    private bool isPlacing = false;

    private void Awake()
    {
        Debug.Log($"[BUTTON-DEBUG] {gameObject.name}: Iniciando Awake");
        SetupButtonComponent();
    }

    private void Start()
    {
        Debug.Log($"[BUTTON-DEBUG] {gameObject.name}: Iniciando Start");
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnBuildingSelected);
            Debug.Log($"[BUTTON-DEBUG] {gameObject.name}: Listener configurado en Start");
        }
        else
        {
            Debug.LogError($"[BUTTON-DEBUG] {gameObject.name}: ¡Button es null en Start!");
        }
    }

    private void Update()
    {
        // Si estamos en modo de colocación, verificar el estado
        if (isPlacing && buildingPlacer != null)
        {
            if (!buildingPlacer.IsPlacing())
            {
                // Si el BuildingPlacer ya no está en modo de colocación, resetear nuestro estado
                isPlacing = false;
                Debug.Log($"[BUTTON-DEBUG] {gameObject.name}: Colocación finalizada");
            }
        }
    }

    private void SetupButtonComponent()
    {
        Debug.Log($"[BUTTON-DEBUG] {gameObject.name}: Configurando componente Button");
        button = GetComponent<Button>();
        
        if (button == null)
        {
            Debug.Log($"[BUTTON-DEBUG] {gameObject.name}: Añadiendo componente Button");
            button = gameObject.AddComponent<Button>();
        }

        Debug.Log($"[BUTTON-DEBUG] {gameObject.name}: Estado del botón - Interactuable: {button.interactable}");
    }

    public void Initialize(BuildingData data, BuildingPlacer placer)
    {
        Debug.Log($"[BUTTON-DEBUG] {gameObject.name}: Iniciando inicialización");
        
        if (data == null || placer == null)
        {
            Debug.LogError($"[BUTTON-DEBUG] {gameObject.name}: ¡Error! BuildingData: {(data == null ? "NULL" : "OK")}, BuildingPlacer: {(placer == null ? "NULL" : "OK")}");
            return;
        }

        buildingData = data;
        buildingPlacer = placer;

        UpdateUIElements();
        Debug.Log($"[BUTTON-DEBUG] {gameObject.name}: Inicialización completada para {data.buildingName}");
    }

    private void UpdateUIElements()
    {
        if (buildingIcon != null && buildingData.buildingIcon != null)
        {
            buildingIcon.sprite = buildingData.buildingIcon;
            Debug.Log($"[BUTTON-DEBUG] {gameObject.name}: Icono actualizado");
        }
        else
        {
            Debug.LogWarning($"[BUTTON-DEBUG] {gameObject.name}: Problema con el icono - Icon Component: {(buildingIcon == null ? "NULL" : "OK")}, Data Icon: {(buildingData.buildingIcon == null ? "NULL" : "OK")}");
        }

        if (buildingNameText != null)
        {
            buildingNameText.text = buildingData.buildingName;
            Debug.Log($"[BUTTON-DEBUG] {gameObject.name}: Nombre actualizado: {buildingData.buildingName}");
        }

        if (woodCostText != null)
        {
            woodCostText.text = buildingData.woodCost.ToString();
            Debug.Log($"[BUTTON-DEBUG] {gameObject.name}: Costo de madera actualizado: {buildingData.woodCost}");
        }

        if (stoneCostText != null)
        {
            stoneCostText.text = buildingData.stoneCost.ToString();
            Debug.Log($"[BUTTON-DEBUG] {gameObject.name}: Costo de piedra actualizado: {buildingData.stoneCost}");
        }
    }

    private void OnBuildingSelected()
    {
        Debug.Log($"[BUTTON-DEBUG] {gameObject.name}: ¡CLICK DETECTADO en {buildingData.buildingName}!");
        
        // Si ya estamos en modo de colocación, no hacer nada
        if (isPlacing)
        {
            Debug.Log($"[BUTTON-DEBUG] {gameObject.name}: Ya está en modo de colocación");
            return;
        }

        if (!ValidateComponents())
        {
            Debug.LogError($"[BUTTON-DEBUG] {gameObject.name}: Falló la validación de componentes");
            return;
        }

        if (HasEnoughResources())
        {
            Debug.Log($"[BUTTON-DEBUG] {gameObject.name}: Recursos suficientes, procediendo con la construcción");
            AttemptConstruction();
        }
        else
        {
            Debug.LogWarning($"[BUTTON-DEBUG] {gameObject.name}: Recursos insuficientes para construir {buildingData.buildingName}");
            Debug.LogWarning($"[BUTTON-DEBUG] {gameObject.name}: Necesita - Madera: {buildingData.woodCost}, Piedra: {buildingData.stoneCost}");
        }
    }

    private bool ValidateComponents()
    {
        if (buildingPlacer == null)
        {
            Debug.LogError($"[BUTTON-DEBUG] {gameObject.name}: BuildingPlacer es NULL");
            return false;
        }
        if (buildingData == null)
        {
            Debug.LogError($"[BUTTON-DEBUG] {gameObject.name}: BuildingData es NULL");
            return false;
        }
        return true;
    }

    private void AttemptConstruction()
    {
        Debug.Log($"[BUTTON-DEBUG] {gameObject.name}: Intentando craftear {buildingData.buildingName}");

        // Verificar que no estemos ya en modo de colocación
        if (buildingPlacer.IsPlacing())
        {
            Debug.LogWarning($"[BUTTON-DEBUG] {gameObject.name}: BuildingPlacer ya está en modo de colocación");
            return;
        }

        if (Inventory.Instance != null && 
            Inventory.Instance.SpendResources(buildingData.woodCost, buildingData.stoneCost, 0, 0))
        {
            Debug.Log($"[BUTTON-DEBUG] {gameObject.name}: ¡Recursos gastados correctamente!");
            
            // Verificar que el prefab exista
            if (buildingData.buildingPrefab == null)
            {
                Debug.LogError($"[BUTTON-DEBUG] {gameObject.name}: ¡El prefab del edificio es NULL!");
                return;
            }
            
            // Iniciar modo de colocación
            Debug.Log($"[BUTTON-DEBUG] {gameObject.name}: Llamando a StartPlacing con prefab {buildingData.buildingPrefab.name}");
            buildingPlacer.StartPlacing(buildingData);
            
            // Marcar que estamos en modo de colocación
            isPlacing = true;
            
            // Cerrar el panel de construcción SOLO si la colocación se inició correctamente
            if (buildingPlacer.IsPlacing())
            {
                Transform constructionBook = transform.parent.parent;
                if (constructionBook != null)
                {
                    constructionBook.gameObject.SetActive(false);
                    Debug.Log($"[BUTTON-DEBUG] {gameObject.name}: Panel de construcción cerrado");
                }

                // Mostrar recursos restantes
                if (Inventory.Instance != null)
                {
                    Debug.Log($"[BUTTON-DEBUG] {gameObject.name}: Recursos restantes - Madera: {Inventory.Instance.GetResourceAmount("wood")}, Piedra: {Inventory.Instance.GetResourceAmount("stone")}");
                    Inventory.Instance.ShowInventory();
                }
            }
            else
            {
                Debug.LogError($"[BUTTON-DEBUG] {gameObject.name}: ¡Error al iniciar modo de colocación!");
                // Si falla la colocación, devolver los recursos
                if (Inventory.Instance != null)
                {
                    Inventory.Instance.AddResource(buildingData.woodCost, "wood");
                    Inventory.Instance.AddResource(buildingData.stoneCost, "stone");
                    Debug.Log($"[BUTTON-DEBUG] {gameObject.name}: Recursos devueltos por fallo en colocación");
                }
            }
        }
        else
        {
            Debug.LogWarning($"[BUTTON-DEBUG] {gameObject.name}: No se pudieron gastar los recursos");
        }
    }

    private bool HasEnoughResources()
    {
        if (Inventory.Instance == null)
        {
            Debug.LogError($"[BUTTON-DEBUG] {gameObject.name}: Inventory.Instance es NULL");
            return false;
        }

        int currentWood = Inventory.Instance.GetResourceAmount("wood");
        int currentStone = Inventory.Instance.GetResourceAmount("stone");
        
        Debug.Log($"[BUTTON-DEBUG] {gameObject.name}: Verificando recursos para {buildingData.buildingName}");
        Debug.Log($"[BUTTON-DEBUG] {gameObject.name}: Necesita - Madera: {buildingData.woodCost}, Piedra: {buildingData.stoneCost}");
        Debug.Log($"[BUTTON-DEBUG] {gameObject.name}: Tiene - Madera: {currentWood}, Piedra: {currentStone}");
        
        bool hasResources = Inventory.Instance.HasEnoughResources(
            buildingData.woodCost,
            buildingData.stoneCost,
            0,
            0
        );
        
        if (!hasResources)
        {
            Debug.LogWarning($"[BUTTON-DEBUG] {gameObject.name}: Recursos insuficientes");
            Inventory.Instance.ShowInventory();
        }
        
        return hasResources;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"[BUTTON-DEBUG] {gameObject.name}: Mouse ENTRÓ al botón");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log($"[BUTTON-DEBUG] {gameObject.name}: Mouse SALIÓ del botón");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[BUTTON-DEBUG] {gameObject.name}: Click DETECTADO - Botón: {eventData.button}");
    }
} 