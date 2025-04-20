using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;

    public int wood = 0;
    public int stone = 0;
    public int food = 0;
    public int gold = 0;  // Oro para compras y mejoras

    private void Awake()
    {
        // Asegurarse de que solo haya una instancia de Inventory
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Mantener el inventario entre escenas
            Debug.Log("Instancia de Inventory creada y persistente.");
        }
        else
        {
            Destroy(gameObject); // Destruir duplicados
            Debug.Log("Instancia de Inventory duplicada destruida.");
        }
    }

    // Método para agregar recursos al inventario
    public void AddResource(int cantidad, string tipoRecurso)
    {
        switch (tipoRecurso.ToLower())
        {
            case "wood":
                wood += cantidad;
                Debug.Log($"Agregado {cantidad} de madera. Total: {wood}");
                break;
            case "stone":
                stone += cantidad;
                Debug.Log($"Agregado {cantidad} de piedra. Total: {stone}");
                break;
            case "food":
                food += cantidad;
                Debug.Log($"Agregado {cantidad} de comida. Total: {food}");
                break;
            case "gold":
                gold += cantidad;
                Debug.Log($"Agregado {cantidad} de oro. Total: {gold}");
                break;
            default:
                Debug.LogWarning($"Tipo de recurso desconocido: {tipoRecurso}");
                break;
        }
    }

    // Método para verificar si hay suficientes recursos
    public bool HasEnoughResources(int woodNeeded, int stoneNeeded, int foodNeeded, int goldNeeded)
    {
        return wood >= woodNeeded && 
               stone >= stoneNeeded && 
               food >= foodNeeded && 
               gold >= goldNeeded;
    }

    // Método para gastar recursos
    public bool SpendResources(int woodCost, int stoneCost, int foodCost, int goldCost)
    {
        if (HasEnoughResources(woodCost, stoneCost, foodCost, goldCost))
        {
            wood -= woodCost;
            stone -= stoneCost;
            food -= foodCost;
            gold -= goldCost;
            Debug.Log($"Recursos gastados - Madera: {woodCost}, Piedra: {stoneCost}, Comida: {foodCost}, Oro: {goldCost}");
            return true;
        }
        Debug.LogWarning("No hay suficientes recursos para realizar esta acción");
        return false;
    }

    // Método para mostrar los recursos (puedes llamarlo desde otro script)
    public void ShowResources()
    {
        Debug.Log($"Recursos del jugador - Madera: {wood}, Piedra: {stone}, Comida: {food}, Oro: {gold}");
    }

    // Método para mostrar el inventario en la consola (útil para depuración)
    public void ShowInventory()
    {
        Debug.Log($"Inventario actual: Madera: {wood}, Piedra: {stone}, Comida: {food}, Oro: {gold}");
    }

    // Método para obtener la cantidad de un recurso específico
    public int GetResourceAmount(string tipoRecurso)
    {
        switch (tipoRecurso.ToLower())
        {
            case "wood": return wood;
            case "stone": return stone;
            case "food": return food;
            case "gold": return gold;
            default:
                Debug.LogWarning($"Tipo de recurso desconocido: {tipoRecurso}");
                return 0;
        }
    }
}
