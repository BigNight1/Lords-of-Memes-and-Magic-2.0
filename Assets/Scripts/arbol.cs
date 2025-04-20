using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class arbol : RecursoScript
{
    public int woodAmount = 10;

    protected override void Start()
    {
        // Establecer valores específicos del árbol
        cantidadRecurso = woodAmount;

        // Llamar al método Start de la clase base
        base.Start();
    }

    // Sobrescribir el método para llamar a StartChopping en lugar de StartRecolection
    public override void OnRecolectarButtonClick()
    {
        base.OnRecolectarButtonClick(); // Esto enviará a un humano a recolectar
    }

    // Método específico para empezar a talar el árbol
    public void StartChopping(GameObject player)
    {
        
        // Activar el TimeBar cuando el humano llega al árbol
        if (timeBar != null)
        {
            timeBar.gameObject.SetActive(true);
        }
        
        StartRecolection(player);
    }

    // Implementación específica para agregar madera al inventario
    protected override void AgregarRecursoAlInventario(Inventory inventory)
    {
        
        if (Inventory.Instance == null)
        {
            Debug.LogError("ERROR: Inventory.Instance es null");
            return;
        }
        
        Inventory.Instance.AddResource(woodAmount, "wood"); // Llama al método de agregar madera al inventario global
        
        // Actualizar la UI
        UIManager uiManager = FindFirstObjectByType<UIManager>();
        if (uiManager != null)
        {
            uiManager.UpdateResourceUI();
        }
        else
        {
            Debug.LogError("ERROR: No se encontró UIManager");
        }
        
        // Marcar al humano como disponible después de talar
        HumanController humanController = FindFirstObjectByType<HumanController>();
        if (humanController != null)
        {
            humanController.FinishChopping(); // Marcar como no ocupado
        }
        else
        {
            Debug.LogError("ERROR: No se encontró HumanController");
        }
    }
}