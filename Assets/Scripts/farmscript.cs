using UnityEngine;
using System.Collections;

public class farmscript : MonoBehaviour
{
    public int cantidadComida = 10; // Cantidad de comida que genera cada vez
    public float intervalo = 10f; // Segundos entre generación de comida
    private bool isConstructed = false;
    private Coroutine foodGenerationCoroutine;
    
    private void Awake()
    {
        Debug.Log($"[DEBUG-FLOW] Farm {gameObject.name}: Awake llamado");
        // Desactivar el script al inicio - será activado por BuildingPlacer
        // cuando se coloque definitivamente
        enabled = false;
    }
    
    private void Start()
    {
        Debug.Log($"[DEBUG-FLOW] Farm {gameObject.name}: Start llamado - Verificando si es modo previsualización");
        // No realizar ninguna acción en Start, todo se maneja en OnEnable
    }
    
    private void OnEnable()
    {
        Debug.Log($"[DEBUG-FLOW] Farm {gameObject.name}: OnEnable llamado");
        
        // Solo ejecutar la lógica si no estamos en modo previsualización
        // (BuildingPlacer activará este script solo cuando se coloque definitivamente)
        if (!isConstructed)
        {
            isConstructed = true;
            Debug.Log($"[DEBUG-FLOW] Farm {gameObject.name}: Ahora está construido definitivamente");
            
            // Solo registrar y comenzar producción cuando se activa después de la colocación
            if (PopulationManager.Instance != null)
            {
                Debug.Log($"[DEBUG-FLOW] Farm {gameObject.name}: Registrando en PopulationManager");
                PopulationManager.Instance.AgregarGranja();
            }
            
            Debug.Log($"[DEBUG-FLOW] Farm {gameObject.name}: Iniciando generación de comida");
            foodGenerationCoroutine = StartCoroutine(GenerarComidaRutina());
        }
    }
    
    private void OnDisable()
    {
        Debug.Log($"[DEBUG-FLOW] Farm {gameObject.name}: OnDisable llamado - isConstructed: {isConstructed}");
        // Solo detener la coroutine si estamos construidos (no en previsualización)
        if (isConstructed && foodGenerationCoroutine != null)
        {
            StopCoroutine(foodGenerationCoroutine);
            foodGenerationCoroutine = null;
            Debug.Log($"[DEBUG-FLOW] Farm {gameObject.name}: Generación de comida detenida");
        }
    }
    
    private void OnDestroy()
    {
        Debug.Log($"[DEBUG-FLOW] Farm {gameObject.name}: OnDestroy llamado - isConstructed: {isConstructed}");
        // Solo notificar al PopulationManager si estábamos realmente construidos
        if (isConstructed && PopulationManager.Instance != null)
        {
            Debug.Log($"[DEBUG-FLOW] Farm {gameObject.name}: Notificando eliminación a PopulationManager");
            PopulationManager.Instance.RemoverGranja();
        }
    }
    
    private IEnumerator GenerarComidaRutina()
    {
        Debug.Log($"[DEBUG-FLOW] Farm {gameObject.name}: Iniciando rutina de generación");
        while (true)
        {
            // Esperar el intervalo especificado
            yield return new WaitForSeconds(intervalo);
            
            // Agregar comida al inventario
            if (Inventory.Instance != null)
            {
                Inventory.Instance.AddResource(cantidadComida, "food");
                Debug.Log($"[DEBUG-FLOW] Farm {gameObject.name}: Generados {cantidadComida} de comida");
                
                // Actualizar UI si hay un UIManager
                UIManager uiManager = FindAnyObjectByType<UIManager>();
                if (uiManager != null)
                {
                    uiManager.UpdateResourceUI();
                }
            }
            else
            {
                Debug.LogError($"[DEBUG-FLOW] Farm {gameObject.name}: No se pudo añadir comida - Inventory.Instance no encontrado");
            }
        }
    }
}
