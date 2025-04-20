using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitCardUI : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private Image unitImage;
    [SerializeField] private TextMeshProUGUI unitNameText;
    [SerializeField] private TextMeshProUGUI woodText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private Button trainButton;

    [Header("Datos de la Unidad")]
    [SerializeField] private UnitData unitData;

    private void Start()
    {
        trainButton.onClick.AddListener(OnTrainButtonClicked);

        if (unitData != null)
        {
            ConfigureUnitCard(unitData);
        }
    }

    public void ConfigureUnitCard(UnitData data)
    {
        unitData = data;
        if (unitNameText != null) unitNameText.text = data.unitName;
        if (unitImage != null && data.unitSprite != null) unitImage.sprite = data.unitSprite;
        if (woodText != null) woodText.text = data.woodCost.ToString();
        if (goldText != null) goldText.text = data.goldCost.ToString();
        if (timeText != null) timeText.text = data.trainingTime.ToString("F1") + "s";
    }

    private void OnTrainButtonClicked()
    {
        if (unitData != null)
        {
            Debug.Log($"Entrenando unidad: {unitData.unitName}");
            Debug.Log($"Costes - Madera: {unitData.woodCost}, Oro: {unitData.goldCost}");
            Debug.Log($"Tiempo de entrenamiento: {unitData.trainingTime}s");
        }
    }
} 