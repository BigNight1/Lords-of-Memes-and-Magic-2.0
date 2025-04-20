using UnityEngine;

public class Unit : MonoBehaviour
{
    public UnitType tipoUnidad;
    
    private void Start()
    {
        // Registrar la unidad en el PopulationManager
        if (PopulationManager.Instance != null)
        {
            PopulationManager.Instance.RegistrarUnidad(this);
        }
    }

    private void OnDestroy()
    {
        // Remover la unidad del PopulationManager cuando se destruye
        if (PopulationManager.Instance != null)
        {
            PopulationManager.Instance.RemoverUnidad(this);
        }
    }
} 