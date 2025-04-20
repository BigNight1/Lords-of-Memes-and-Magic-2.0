using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject[] arbolPrefabs;
    public GameObject[] rocaPrefabs;
    
    [Header("Configuración de Generación")]
    public int cantidadArboles = 25;
    public int cantidadRocas = 25;
    [Tooltip("Distancia mínima entre objetos generados")]
    public float minDistanciaEntreObjetos = 1f;
    
    [Header("Referencias")]
    public Tilemap groundTilemap;
    public bool mostrarDebug = true;

    private Grid grid;
    private BoundsInt areaTiles;

    private void Start()
    {
        // Resetear la posición del objeto Map y sus hijos
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        
        // Obtener el componente Grid
        grid = GetComponentInChildren<Grid>();
        if (grid == null)
        {
            Debug.LogError("MapGenerator: No se encontró el componente Grid!");
            return;
        }

        if (groundTilemap == null)
        {
            groundTilemap = GetComponentInChildren<Tilemap>();
            if (groundTilemap == null)
            {
                Debug.LogError("MapGenerator: No se encontró el Tilemap!");
                return;
            }
        }

        // Asegurar que el Tilemap esté en la posición correcta
        groundTilemap.transform.position = new Vector3(0, 0, 0);
        
        // Obtener el área donde hay tiles
        areaTiles = groundTilemap.cellBounds;
        
        // Contar tiles y mostrar información
        int totalTiles = ContarTiles();
        Debug.Log($"MapGenerator: Área de tiles: x={areaTiles.xMin} a {areaTiles.xMax}, y={areaTiles.yMin} a {areaTiles.yMax}");
        Debug.Log($"MapGenerator: Cantidad de tiles encontrados: {totalTiles}");

        if (totalTiles == 0)
        {
            Debug.LogError("MapGenerator: No hay tiles en el Tilemap! Dibuja algunos tiles primero.");
            return;
        }

        GenerarObjetos();
    }

    private int ContarTiles()
    {
        int count = 0;
        foreach (var pos in areaTiles.allPositionsWithin)
        {
            if (groundTilemap.HasTile(pos))
            {
                count++;
            }
        }
        return count;
    }

    private void GenerarObjetos()
    {
        // Limpiar objetos existentes
        Transform existingContainer = transform.Find("Objetos_Generados");
        if (existingContainer != null)
        {
            DestroyImmediate(existingContainer.gameObject);
        }

        // Crear nuevo contenedor
        Transform contenedor = new GameObject("Objetos_Generados").transform;
        contenedor.parent = transform;
        contenedor.localPosition = Vector3.zero;

        // Generar árboles
        int arbolesGenerados = 0;
        for (int i = 0; i < cantidadArboles; i++)
        {
            if (ColocarObjetoAleatorio(arbolPrefabs, contenedor))
            {
                arbolesGenerados++;
            }
        }
        Debug.Log($"MapGenerator: Se generaron {arbolesGenerados} árboles");

        // Generar rocas
        int rocasGeneradas = 0;
        for (int i = 0; i < cantidadRocas; i++)
        {
            if (ColocarObjetoAleatorio(rocaPrefabs, contenedor))
            {
                rocasGeneradas++;
            }
        }
        Debug.Log($"MapGenerator: Se generaron {rocasGeneradas} rocas");
    }

    private bool ColocarObjetoAleatorio(GameObject[] prefabs, Transform parent)
    {
        if (prefabs == null || prefabs.Length == 0) return false;

        for (int intentos = 0; intentos < 10; intentos++)
        {
            // Obtener una posición aleatoria dentro del área con tiles
            Vector3Int posicionGrid = new Vector3Int(
                Random.Range(areaTiles.xMin, areaTiles.xMax),
                Random.Range(areaTiles.yMin, areaTiles.yMax),
                0
            );

            // Verificar si hay un tile en esa posición
            if (!groundTilemap.HasTile(posicionGrid))
            {
                if (mostrarDebug) Debug.Log($"MapGenerator: No hay tile en {posicionGrid}");
                continue;
            }

            // Convertir a posición mundial
            Vector3 posicionMundo = groundTilemap.CellToWorld(posicionGrid);
            posicionMundo += new Vector3(1, 1, 0); // Centrar en el tile
            posicionMundo.z = 0; // Asegurar que Z sea 0

            // Verificar si hay otros objetos cerca
            if (HayObjetosCerca(posicionMundo))
            {
                if (mostrarDebug) Debug.Log($"MapGenerator: Hay objetos cerca en {posicionMundo}");
                continue;
            }

            try
            {
                // Crear el objeto
                GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
                GameObject objeto = Instantiate(prefab, posicionMundo, Quaternion.identity, parent);
                objeto.transform.localPosition = new Vector3(
                    objeto.transform.localPosition.x,
                    objeto.transform.localPosition.y,
                    0 // Forzar Z a 0
                );

            

                if (mostrarDebug) Debug.Log($"MapGenerator: Objeto {objeto.name} creado en {objeto.transform.position}");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"MapGenerator: Error al crear objeto: {e.Message}");
            }
        }

        return false;
    }

    private bool HayObjetosCerca(Vector3 posicion)
    {
        // Ignorar el Tilemap en la detección de colisiones
        int layerMask = ~LayerMask.GetMask("Default"); // Ajusta esto según tus layers
        Collider2D[] colliders = Physics2D.OverlapCircleAll(posicion, minDistanciaEntreObjetos, layerMask);
        
        // Filtrar el Tilemap de la lista de colisiones
        int objetosRelevantes = 0;
        foreach (var collider in colliders)
        {
            if (collider.gameObject != groundTilemap.gameObject)
            {
                objetosRelevantes++;
                if (mostrarDebug)
                {
                    Debug.Log($"MapGenerator: Objeto cercano: {collider.gameObject.name} en {collider.transform.position}");
                }
            }
        }
        
        return objetosRelevantes > 0;
    }

    private void OnDrawGizmos()
    {
        if (!mostrarDebug || groundTilemap == null) return;

        // Dibujar el área del Tilemap
        Gizmos.color = Color.yellow;
        Vector3 center = groundTilemap.transform.position;
        Vector3 size = new Vector3(
            groundTilemap.size.x * 2,
            groundTilemap.size.y * 2,
            1
        );
        Gizmos.DrawWireCube(center, size);

        // Dibujar cada tile
        Gizmos.color = Color.green;
        foreach (var pos in groundTilemap.cellBounds.allPositionsWithin)
        {
            if (groundTilemap.HasTile(pos))
            {
                Vector3 worldPos = groundTilemap.CellToWorld(pos);
                worldPos += new Vector3(1, 1, 0);
                Gizmos.DrawWireCube(worldPos, new Vector3(1.8f, 1.8f, 0.1f));
            }
        }
    }
} 