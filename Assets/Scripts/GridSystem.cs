using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class GridSystem : MonoBehaviour
{
    [Header("Grid Settings")]
    public float cellSize = 2f;  // Cambiado a 2f para coincidir con el Grid
    public Vector2 gridSize = new Vector2(100, 100);
    public Tilemap groundTilemap;
    public bool mostrarGrid = true;
    
    [Header("Building Preview")]
    // IMPORTANTE: No asignar nada aquí. La previsualización ahora es manejada por BuildingPlacer
    // Dejar este campo vacío en el Inspector
    [Tooltip("DEJAR VACÍO - La previsualización ahora es manejada por BuildingPlacer")]
    public GameObject buildingPreview;
    public Color validPlacementColor = Color.green;
    public Color invalidPlacementColor = Color.red;
    
    [Header("Debug")]
    public bool enableGridSystemPreview = false; // IMPORTANTE: Mantener esto en FALSE para evitar conflictos con BuildingPlacer
    public bool showDebugInfo = true; // Nuevo: para mostrar información de debug
    
    [Header("Visual Grid")]
    public GameObject gridLinesPrefab; // Prefab para las líneas del grid (puede ser un GameObject simple con LineRenderer)
    public Color gridColor = new Color(0, 1, 0, 0.3f); // Color para las líneas del grid
    private GameObject gridContainer; // Contenedor para todas las líneas del grid
    private bool gridCreated = false;
    
    // NUEVO: Diccionario para rastrear qué celdas están ocupadas
    private Dictionary<Vector3Int, GameObject> ocupiedCells = new Dictionary<Vector3Int, GameObject>();
    
    private Vector3 mousePosition;
    private Vector3Int gridPosition;
    private bool canPlace;
    private SpriteRenderer previewRenderer;
    
    // Diccionario para almacenar las celdas del grid
    private Dictionary<Vector3Int, GameObject> cellObjects;
    
    private void Start()
    {
        if (showDebugInfo)
        {
            Debug.Log($"GridSystem: Iniciando con cellSize={cellSize}, gridSize={gridSize}");
            Debug.Log($"GridSystem: Posición del objeto: {transform.position}");
        }

        // Verificar y configurar el Tilemap
        if (groundTilemap == null)
        {
            groundTilemap = GetComponentInChildren<Tilemap>();
            if (groundTilemap != null && showDebugInfo)
            {
                Debug.Log($"GridSystem: Tilemap encontrado automáticamente");
                Debug.Log($"GridSystem: Tilemap cellSize={groundTilemap.cellSize}");
                Debug.Log($"GridSystem: Tilemap origin={groundTilemap.origin}");
                Debug.Log($"GridSystem: Tilemap tileAnchor={groundTilemap.tileAnchor}");
            }
        }
        
        // IMPORTANTE: Verificar la configuración de previsualización
        if (enableGridSystemPreview)
        {
            if (buildingPreview == null)
            {
                enableGridSystemPreview = false;
            }
            else
            {
                previewRenderer = buildingPreview.GetComponent<SpriteRenderer>();
                buildingPreview.SetActive(false);
            }
        }
        else
        {
            // Si buildingPreview está asignado pero no se va a usar, mostrar advertencia
            if (buildingPreview != null)
            {
                buildingPreview.SetActive(false);
            }
        }
        
        // Inicializar el grid visual
        InitializeGridVisual();
    }
    
    private void Update()
    {
        // IMPORTANTE: Este sistema de previsualización está desactivado por defecto
        // Se recomienda usar BuildingPlacer para la previsualización en su lugar
        if (!enableGridSystemPreview) 
        {
            // Si buildingPreview está asignado pero enableGridSystemPreview está desactivado,
            // mostrar una advertencia
            if (buildingPreview != null && buildingPreview.activeSelf)
            {
                buildingPreview.SetActive(false);
            }
            return;
        }
        
        // Verificar que buildingPreview esté asignado
        if (buildingPreview == null) 
        {
            enableGridSystemPreview = false;
            return;
        }
        
        // Obtener posición del mouse en el mundo
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        
        // Convertir a posición de grid
        gridPosition = GetGridPosition(mousePosition);
        
        // Actualizar posición del preview
        Vector3 previewPosition = GetWorldPosition(gridPosition.x, gridPosition.y);
        buildingPreview.transform.position = previewPosition;
        
        // Verificar si se puede colocar
        canPlace = CheckPlacement();
        if (previewRenderer != null)
        {
            previewRenderer.color = canPlace ? validPlacementColor : invalidPlacementColor;
        }
        
        // Colocar edificio al hacer clic
        if (Input.GetMouseButtonDown(0) && canPlace)
        {
            PlaceBuilding(gridPosition, buildingPreview);
        }

        // Toggle del grid con la tecla G
        if (Input.GetKeyDown(KeyCode.G))
        {
            ToggleGrid();
        }
    }
    
    // Convierte posición del mundo a posición del grid
    public Vector3Int GetGridPosition(Vector3 worldPosition)
    {
        // Ajustar por el tile anchor
        Vector3Int gridPos = new Vector3Int(
            Mathf.FloorToInt(worldPosition.x / cellSize),
            Mathf.FloorToInt(worldPosition.y / cellSize),
            0
        );

        if (showDebugInfo)
        {
            Debug.Log($"GridSystem: Convirtiendo posición mundial {worldPosition} a posición grid {gridPos}");
        }

        return gridPos;
    }
    
    // Convierte posición del grid a posición del mundo
    public Vector3 GetWorldPosition(int x, int y)
    {
        // Ajustar por el tile anchor
        Vector3 worldPos = new Vector3(
            x * cellSize + (cellSize * 0.5f), // Centrado en la celda
            y * cellSize + (cellSize * 0.5f),
            0
        );

        if (showDebugInfo)
        {
            Debug.Log($"GridSystem: Convirtiendo posición grid ({x},{y}) a posición mundial {worldPos}");
        }

        return worldPos;
    }
    
    // Verifica si se puede construir en una posición del grid
    public bool CanBuildAt(Vector3Int gridPos)
    {
        // Verificar límites del grid
        bool inBounds = gridPos.x >= 0 && gridPos.x < gridSize.x &&
                       gridPos.y >= 0 && gridPos.y < gridSize.y;
        
        // Verificar si hay un tile en esa posición
        bool hasTile = true;
        if (groundTilemap != null)
        {
            hasTile = groundTilemap.HasTile(gridPos);
        }
        
        bool isOccupied = ocupiedCells.ContainsKey(gridPos);
        
        if (showDebugInfo)
        {
            Debug.Log($"GridSystem: Verificando posición {gridPos}:");
            Debug.Log($"  - Dentro de límites: {inBounds}");
            Debug.Log($"  - Tiene tile: {hasTile}");
            Debug.Log($"  - Está ocupada: {isOccupied}");
        }
        
        return inBounds && hasTile && !isOccupied;
    }
    
    private bool CheckPlacement()
    {
        bool result = CanBuildAt(gridPosition);
        return result;
    }
    
    public void PlaceBuilding(Vector3Int gridPos, GameObject building)
    {
        if (building == null)
        {
            Debug.LogWarning("[DIAGNÓSTICO] PlaceBuilding - Objeto de construcción es nulo");
            return;
        }
        
        if (CanBuildAt(gridPos))
        {
            Vector3 position = GetWorldPosition(gridPos.x, gridPos.y);
            
            // NO crear un nuevo objeto, solo mover el existente
            building.transform.position = position;
            
            // NUEVO: Marcar la celda como ocupada
            ocupiedCells[gridPos] = building;
            Debug.Log($"[DIAGNÓSTICO] PlaceBuilding - Celda {gridPos} marcada como ocupada por {building.name}");
            Debug.Log($"[DIAGNÓSTICO] PlaceBuilding - Total de celdas ocupadas: {ocupiedCells.Count}");
            
            // Activar farmscript si existe
            farmscript farm = building.GetComponent<farmscript>();
            if (farm != null)
            {
                farm.enabled = true;
            }
        }
        else
        {
            Debug.LogWarning($"[DIAGNÓSTICO] PlaceBuilding - No se puede construir en {gridPos}, ya está ocupada o fuera de límites");
        }
    }
    
    public void ShowBuildingPreview(GameObject building)
    {
        if (buildingPreview != null)
        {
            buildingPreview.SetActive(false);
        }
        
        buildingPreview = building;
        if (building != null)
        {
            previewRenderer = buildingPreview.GetComponent<SpriteRenderer>();
            buildingPreview.SetActive(true);
        }
    }

    // Método para crear el grid visual con líneas de GameObject
    private void CreateVisualGrid()
    {
        // Si ya existe un grid, destruirlo
        if (gridContainer != null)
        {
            Destroy(gridContainer);
        }

        // Crear un contenedor para todas las líneas
        gridContainer = new GameObject("GridVisual");
        gridContainer.transform.SetParent(transform);

        // Ajustar el cellSize basado en el tamaño real del tilemap si está disponible
        if (groundTilemap != null)
        {
            // Intentar usar el tamaño de celda del tilemap
            Vector3 cellWorldSize = groundTilemap.transform.localScale;
            // Usar tamaño de celda del tilemap solo si es razonable
            if (cellWorldSize.x > 0.1f && cellWorldSize.y > 0.1f)
            {
                cellSize = Mathf.Min(cellWorldSize.x, cellWorldSize.y);
            }
        }

        // IMPORTANTE: Forzar un tamaño de celda de 2 como mencionó el usuario
        cellSize = 2f;

        // Crear celdas del grid
        cellObjects = new Dictionary<Vector3Int, GameObject>();
        
        int gridExtent = 10; // Cuántas celdas mostrar en cada dirección
        
        for (int x = -gridExtent; x <= gridExtent; x++)
        {
            for (int y = -gridExtent; y <= gridExtent; y++)
            {
                Vector3Int gridPos = new Vector3Int(x, y, 0);
                Vector3 worldPos = GetWorldPosition(x, y);
                
                // Crear un objeto para representar cada celda
                GameObject cellObject = new GameObject($"Cell_{x}_{y}");
                cellObject.transform.SetParent(gridContainer.transform);
                cellObject.transform.position = worldPos;
                
                // Añadir un SpriteRenderer para mostrar el color de la celda
                SpriteRenderer cellRenderer = cellObject.AddComponent<SpriteRenderer>();
                cellRenderer.sprite = CreateCellSprite();
                cellRenderer.color = new Color(0, 1, 0, 0.3f); // Color predeterminado (verde transparente)
                
                // Escalar el sprite para que cubra la celda (multiplicar por 0.95 para dejar un pequeño espacio)
                cellRenderer.transform.localScale = new Vector3(cellSize * 0.95f, cellSize * 0.95f, 1);
                
                // Asegurar que las celdas estén detrás de otros elementos
                cellRenderer.sortingOrder = -10;
                
                // Guardar la celda en el diccionario
                cellObjects[gridPos] = cellObject;
            }
        }

        // Inicialmente ocultar el grid y solo mostrarlo durante la construcción
        gridContainer.SetActive(false);
        gridCreated = true;
    }
    
    // Crear un sprite para la celda
    private Sprite CreateCellSprite()
    {
        // Crear un sprite cuadrado blanco básico
        Texture2D texture = new Texture2D(32, 32);
        Color[] colors = new Color[32 * 32];
        
        // Crear un cuadrado con bordes
        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                // Bordes del cuadrado (1 píxel de grosor)
                if (x == 0 || x == 31 || y == 0 || y == 31)
                {
                    colors[y * 32 + x] = Color.white; // Borde blanco
                }
                // Interior semi-transparente
                else
                {
                    colors[y * 32 + x] = new Color(1f, 1f, 1f, 0.7f); // Interior blanco semi-transparente
                }
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
    }
    
    // Actualizar el color de las celdas según si se puede construir o no
    public void UpdateCellColors(Vector3Int currentGridPos)
    {
        if (cellObjects == null || gridContainer == null || !gridContainer.activeSelf) return;
        
        foreach (var kvp in cellObjects)
        {
            Vector3Int gridPos = kvp.Key;
            GameObject cellObj = kvp.Value;
            
            if (cellObj == null) continue;
            
            SpriteRenderer renderer = cellObj.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                bool canBuild = CanBuildAt(gridPos);
                
                // Celda actual del cursor
                if (gridPos == currentGridPos)
                {
                    renderer.color = canBuild ? 
                        new Color(0, 1, 0, 0.7f) : // Verde más opaco
                        new Color(1, 0, 0, 0.7f);  // Rojo más opaco
                }
                // Resto de celdas
                else
                {
                    renderer.color = canBuild ? 
                        new Color(0, 1, 0, 0.3f) : // Verde transparente
                        new Color(1, 0, 0, 0.3f);  // Rojo transparente
                }
            }
        }
    }

    // Método para crear una línea del grid
    private void CreateGridLine(Vector3 start, Vector3 end)
    {
        GameObject line;
        
        // Si hay un prefab asignado, usarlo
        if (gridLinesPrefab != null)
        {
            line = Instantiate(gridLinesPrefab, Vector3.zero, Quaternion.identity);
        }
        else
        {
            // Crear un GameObject básico con LineRenderer
            line = new GameObject("GridLine");
            LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.05f;
            lineRenderer.endWidth = 0.05f;
            lineRenderer.positionCount = 2;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = gridColor;
            lineRenderer.endColor = gridColor;
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
        }
        
        line.transform.SetParent(gridContainer.transform);
    }

    // Toggle para mostrar/ocultar el grid
    public void ToggleGrid()
    {
        mostrarGrid = !mostrarGrid;
        
        // Mostrar/ocultar el grid visual si existe
        if (gridContainer != null)
        {
            gridContainer.SetActive(mostrarGrid);
        }
        else if (mostrarGrid)
        {
            // Si el grid no existe pero queremos mostrarlo, crearlo
            CreateVisualGrid();
            gridContainer.SetActive(true);
        }
    }
    
    // Método para mostrar el grid durante la construcción
    public void ShowGridDuringBuilding(bool show)
    {
        if (gridContainer == null && show)
        {
            CreateVisualGrid();
        }
        
        if (gridContainer != null)
        {
            gridContainer.SetActive(show && mostrarGrid);
        }
    }

    // Dibuja el grid en el editor
    private void OnDrawGizmos()
    {
        if (!mostrarGrid) return;

        // Obtener BuildingPlacer para verificar si está en modo de construcción
        BuildingPlacer buildingPlacer = FindAnyObjectByType<BuildingPlacer>();
        bool isPlacing = buildingPlacer != null && buildingPlacer.IsPlacing();

        // Solo dibujar la cuadrícula cuando se está construyendo o si estamos en el editor
        if (!Application.isPlaying || isPlacing)
        {
            // Obtener la posición actual del mouse en el grid cuando se está construyendo
            Vector3Int currentGridPos = Vector3Int.zero;
            if (isPlacing && Camera.main != null)
            {
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePosition.z = 0;
                currentGridPos = GetGridPosition(mousePosition);
            }

            // Dibujar el grid con colores diferentes dependiendo de si se puede construir
            for (int x = -10; x < 10; x++)
            {
                for (int y = -10; y < 10; y++)
                {
                    Vector3Int gridPos = new Vector3Int(x, y, 0);
                    Vector3 pos = GetWorldPosition(x, y);
                    
                    // Solo colorear celdas cuando se está construyendo
                    if (isPlacing)
                    {
                        bool canBuild = CanBuildAt(gridPos);
                        
                        // Celda actual del mouse
                        if (gridPos == currentGridPos)
                        {
                            Gizmos.color = canBuild ? Color.green : Color.red;
                            Gizmos.DrawCube(pos, new Vector3(cellSize * 0.9f, cellSize * 0.9f, 0.1f));
                        }
                        // Resto de celdas
                        else
                        {
                            Gizmos.color = canBuild ? new Color(0.7f, 1f, 0.7f, 0.3f) : new Color(1f, 0.7f, 0.7f, 0.2f);
                            Gizmos.DrawCube(pos, new Vector3(cellSize * 0.8f, cellSize * 0.8f, 0.05f));
                            
                            // Borde de la celda
                            Gizmos.color = canBuild ? new Color(0, 0.5f, 0, 0.5f) : new Color(0.5f, 0, 0, 0.3f);
                            Gizmos.DrawWireCube(pos, new Vector3(cellSize, cellSize, 0.1f));
                        }
                    }
                    else
                    {
                        // Grid normal cuando no se está construyendo
                        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
                        Gizmos.DrawWireCube(pos, new Vector3(cellSize, cellSize, 0.1f));
                    }
                }
            }
        }
    }

    // NUEVO: Método para eliminar un edificio y liberar la celda
    public void RemoveBuilding(Vector3Int gridPos)
    {
        Debug.Log($"[DIAGNÓSTICO] RemoveBuilding - Intentando liberar celda {gridPos}");
        if (ocupiedCells.ContainsKey(gridPos))
        {
            GameObject building = ocupiedCells[gridPos];
            ocupiedCells.Remove(gridPos);
            Debug.Log($"[DIAGNÓSTICO] RemoveBuilding - Celda {gridPos} liberada, edificio: {(building != null ? building.name : "destruido")}");
            Debug.Log($"[DIAGNÓSTICO] RemoveBuilding - Total de celdas ocupadas restantes: {ocupiedCells.Count}");
        }
        else
        {
            Debug.LogWarning($"[DIAGNÓSTICO] RemoveBuilding - No hay edificio en la celda {gridPos}");
        }
    }

    // NUEVO: Método para obtener la posición de grid desde un objeto
    public Vector3Int GetGridPositionFromObject(GameObject obj)
    {
        if (obj == null) return new Vector3Int(0, 0, 0);
        return GetGridPosition(obj.transform.position);
    }

    // NUEVO: Método público para verificar si una celda está ocupada
    public bool IsCellOccupied(Vector3Int gridPos)
    {
        bool isOccupied = ocupiedCells.ContainsKey(gridPos);
        Debug.Log($"[DIAGNÓSTICO] IsCellOccupied - Verificando celda {gridPos}: {isOccupied}");
        if (isOccupied)
        {
            Debug.Log($"[DIAGNÓSTICO] IsCellOccupied - Ocupada por: {ocupiedCells[gridPos].name}");
        }
        return isOccupied;
    }

    // Método para inicializar el grid visual
    public void InitializeGridVisual()
    {
        // Asegurar que el grid se crea correctamente
        if (mostrarGrid && !gridCreated)
        {
            CreateVisualGrid();
        }
    }
}
