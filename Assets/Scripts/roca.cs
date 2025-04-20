using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class roca : RecursoScript
{
    public int stoneAmount = 5;

    protected override void Start()
    {
        cantidadRecurso = stoneAmount;
        tiempoRecoleccion = 4f;
        base.Start();
    }

    public override void OnRecolectarButtonClick()
    {
        base.OnRecolectarButtonClick();
    }

    public void StartMining(GameObject player)
    {
        if (timeBar != null)
        {
            timeBar.gameObject.SetActive(true);
        }
        
        StartRecolection(player);
    }

    protected override void AgregarRecursoAlInventario(Inventory inventory)
    {
        Inventory.Instance.AddResource(stoneAmount, "stone");
        FindFirstObjectByType<UIManager>().UpdateResourceUI();
        HumanController humanController = FindFirstObjectByType<HumanController>();
        if (humanController != null)
        {
            humanController.FinishChopping();
        }
    }
}