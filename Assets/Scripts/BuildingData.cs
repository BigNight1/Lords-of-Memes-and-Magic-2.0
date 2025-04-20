using UnityEngine;

[CreateAssetMenu(fileName = "New Building", menuName = "Game/Building Data")]
public class BuildingData : ScriptableObject
{
    [Header("Información Básica")]
    public string buildingName;
    public GameObject buildingPrefab;
    public GameObject buttonPrefab;   // Prefab del botón UI (ej: BuildingButton)
    public Sprite buildingIcon;

    [Header("Costos")]
    public int woodCost;
    public int stoneCost;

    [Header("Configuración Visual")]
    [Tooltip("Color cuando es válido construir")]
    public Color validColor = new Color(0, 1, 0, 0.5f);
    
    [Tooltip("Color cuando no es válido construir")]
    public Color invalidColor = new Color(1, 0, 0, 0.5f);
} 