using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingPlacer : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Grid grid;
    [SerializeField] private Tilemap floorTilemap;

    [Header("Configuración")]
    [SerializeField] private Color validPlacementColor = new Color(0, 1, 0, 0.5f);
    [SerializeField] private Color invalidPlacementColor = new Color(1, 0, 0, 0.5f);
    [SerializeField] private float gridScale = 2f; // Escala del grid (normalmente 2)

    private GameObject buildingPreview;
    private BuildingData currentBuildingData;
    private bool isPlacing = false;
    private Material[] previewMaterials;

    private void Start()
    {
        if (!mainCamera) mainCamera = Camera.main;
        if (!grid) grid = FindAnyObjectByType<Grid>();

        // Verificar configuración
        if (!mainCamera || !grid || !floorTilemap)
        {
            Debug.LogError("[PLACER-DEBUG] Faltan referencias necesarias. Verifica la configuración en el inspector.");
            enabled = false;
            return;
        }

        Debug.Log($"[PLACER-DEBUG] Configuración inicial - Camera: {mainCamera.name}, Grid Scale: {grid.cellSize}, Projection: {mainCamera.orthographic}");
    }

    private void Update()
    {
        if (!isPlacing || !buildingPreview) return;

        // Obtener posición del mouse
        Vector3 mousePosition = Input.mousePosition;
        Debug.Log($"[PLACER-DEBUG] Mouse Position (Screen): {mousePosition}");

        // Si la cámara es ortográfica
        if (mainCamera.orthographic)
        {
            // Convertir posición del mouse a mundo
            Vector3 worldPoint = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 0));
            worldPoint.z = 0; // Asegurar que estamos en el plano XY
            
            Debug.Log($"[PLACER-DEBUG] World Point (antes de grid): {worldPoint}");

            // Convertir a posición de celda
            Vector3Int cellPosition = grid.WorldToCell(worldPoint);
            Debug.Log($"[PLACER-DEBUG] Cell Position: {cellPosition}");

            // Obtener el centro de la celda
            Vector3 finalPosition = grid.CellToWorld(cellPosition);
            finalPosition.z = 0;
            
            Debug.Log($"[PLACER-DEBUG] Final Position: {finalPosition}");

            // Actualizar posición del preview
            buildingPreview.transform.position = finalPosition;

            // Verificar si se puede construir
            bool canBuild = CanBuildAt(cellPosition);
            UpdatePreviewColor(canBuild);

            // Colocar edificio con click izquierdo
            if (Input.GetMouseButtonDown(0) && canBuild)
            {
                FinalizePlacement(finalPosition);
            }
        }

        // Cancelar con click derecho
        if (Input.GetMouseButtonDown(1))
        {
            CancelPlacement();
        }

        // Rotar el edificio con Q y E
        if (Input.GetKeyDown(KeyCode.Q))
        {
            buildingPreview.transform.Rotate(Vector3.up, -90f);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            buildingPreview.transform.Rotate(Vector3.up, 90f);
        }
    }

    public void StartPlacing(BuildingData buildingData)
    {
        if (buildingData == null || buildingData.buildingPrefab == null)
        {
            Debug.LogError("[PLACER-DEBUG] BuildingData o prefab es NULL");
            return;
        }

        CancelPlacement();
        currentBuildingData = buildingData;
        
        buildingPreview = Instantiate(buildingData.buildingPrefab);
        Debug.Log($"[PLACER-DEBUG] Preview creado: {buildingPreview.name} en posición {buildingPreview.transform.position}");
        
        // Desactivar scripts durante la previsualización
        var scripts = buildingPreview.GetComponentsInChildren<MonoBehaviour>();
        foreach (var script in scripts)
        {
            if (script != null && script.GetType().Name != "Transform")
            {
                script.enabled = false;
            }
        }

        // Configurar materiales para el preview
        var renderers = buildingPreview.GetComponentsInChildren<Renderer>();
        previewMaterials = new Material[renderers.Length];
        
        for (int i = 0; i < renderers.Length; i++)
        {
            Material previewMaterial = new Material(renderers[i].material);
            previewMaterial.color = validPlacementColor;
            renderers[i].material = previewMaterial;
            previewMaterials[i] = previewMaterial;
        }

        isPlacing = true;
    }

    private void UpdatePreviewColor(bool canBuild)
    {
        Color color = canBuild ? validPlacementColor : invalidPlacementColor;
        foreach (Material material in previewMaterials)
        {
            if (material != null)
            {
                material.color = color;
            }
        }
    }

    private bool CanBuildAt(Vector3Int cellPosition)
    {
        // Verificar si hay un tile en la posición
        if (!floorTilemap.HasTile(cellPosition))
        {
            return false;
        }

        // Verificar colisiones con otros edificios
        Vector3 worldPosition = grid.GetCellCenterWorld(cellPosition);
        Collider[] colliders = Physics.OverlapBox(
            worldPosition,
            Vector3.one * (gridScale * 0.4f),
            Quaternion.identity
        );

        foreach (Collider collider in colliders)
        {
            if (collider.gameObject != buildingPreview)
            {
                return false;
            }
        }

        return true;
    }

    private void FinalizePlacement(Vector3 position)
    {
        if (!buildingPreview || currentBuildingData == null) return;

        GameObject finalBuilding = Instantiate(currentBuildingData.buildingPrefab, 
            position, 
            buildingPreview.transform.rotation);

        Debug.Log($"[PLACER-DEBUG] Edificio colocado en posición: {position}");

        CancelPlacement();
    }

    private void CancelPlacement()
    {
        if (buildingPreview)
        {
            Destroy(buildingPreview);
            buildingPreview = null;
        }
        
        if (previewMaterials != null)
        {
            foreach (Material material in previewMaterials)
            {
                if (material != null)
                {
                    Destroy(material);
                }
            }
            previewMaterials = null;
        }
        
        currentBuildingData = null;
        isPlacing = false;
    }

    public bool IsPlacing()
    {
        return isPlacing;
    }
} 