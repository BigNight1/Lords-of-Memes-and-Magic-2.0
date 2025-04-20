using UnityEngine;
using System.Collections.Generic;

public class PopulationManager : MonoBehaviour
{
    public static PopulationManager Instance { get; private set; }
    
    public int poblacionActual = 0;
    public int poblacionMaxima = 10; // Población base
    private int cantidadGranjas = 0;
    private const int POBLACION_POR_GRANJA = 10;

    // Diccionario para contar unidades por tipo
    private Dictionary<UnitType, int> unidadesPorTipo = new Dictionary<UnitType, int>();

    private void Awake()
    {
        // Singleton para la escena actual
        if (Instance == null)
        {
            Instance = this;
            InicializarContadores();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void InicializarContadores()
    {
        // Inicializar contadores para cada tipo de unidad
        foreach (UnitType tipo in System.Enum.GetValues(typeof(UnitType)))
        {
            unidadesPorTipo[tipo] = 0;
        }
    }

    public void RegistrarUnidad(Unit unidad)
    {
        if (poblacionActual < poblacionMaxima)
        {
            poblacionActual++;
            unidadesPorTipo[unidad.tipoUnidad]++;
            Debug.Log($"Unidad registrada: {unidad.tipoUnidad}. Población actual: {poblacionActual}/{poblacionMaxima}");
        }
        else
        {
            Debug.LogWarning("No se puede registrar más unidades: población máxima alcanzada");
        }
    }

    public void RemoverUnidad(Unit unidad)
    {
        if (unidadesPorTipo[unidad.tipoUnidad] > 0)
        {
            poblacionActual--;
            unidadesPorTipo[unidad.tipoUnidad]--;
            Debug.Log($"Unidad removida: {unidad.tipoUnidad}. Población actual: {poblacionActual}/{poblacionMaxima}");
        }
    }

    public void AgregarGranja()
    {
        cantidadGranjas++;
        poblacionMaxima += POBLACION_POR_GRANJA;
        Debug.Log($"Nueva granja construida. Población máxima actualizada a: {poblacionMaxima}");
    }

    public void RemoverGranja()
    {
        if (cantidadGranjas > 0)
        {
            cantidadGranjas--;
            poblacionMaxima -= POBLACION_POR_GRANJA;
            Debug.Log($"Granja destruida. Población máxima actualizada a: {poblacionMaxima}");
        }
    }

    public int GetPoblacionMaxima()
    {
        return poblacionMaxima;
    }

    public int GetPoblacionActual()
    {
        return poblacionActual;
    }

    public int GetCantidadUnidadesPorTipo(UnitType tipo)
    {
        return unidadesPorTipo[tipo];
    }

    public string GetDetallesPoblacion()
    {
        string detalles = $"Población: {poblacionActual}/{poblacionMaxima}\n";
        foreach (UnitType tipo in System.Enum.GetValues(typeof(UnitType)))
        {
            detalles += $"{tipo}: {unidadesPorTipo[tipo]}\n";
        }
        return detalles;
    }

    public string GetPoblacionAll()
    {
        return $"{poblacionActual}/{poblacionMaxima}";
    }
} 