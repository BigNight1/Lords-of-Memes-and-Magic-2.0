using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class RecursoScript : MonoBehaviour
{
    public int cantidadRecurso = 0; // Cantidad de recurso que se puede recolectar
    public float tiempoRecoleccion = 3f; // Tiempo que tarda en recolectar el recurso
    private bool estaRecolectando = false; // Estado de recolección
    private bool estaAsignado = false; // Para controlar si ya hay un humano asignado
    private GameObject humanoAsignado = null; // Referencia al humano asignado a este recurso

    public GameObject canvasRecolectar; // Referencia al canvas con el botón de recolectar
    protected GameObject player; // Referencia al jugador

    public TimeBar timeBar; // Referencia a la barra de tiempo

    protected virtual void Start()
    {
        if (canvasRecolectar != null)
        {
            canvasRecolectar.SetActive(false);
        }

        if (timeBar != null)
        {
            timeBar.gameObject.SetActive(false);
        }

        player = GameObject.FindGameObjectWithTag("Human");
    }

    protected virtual void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            if (estaAsignado)
            {
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null)
            {
                if (hit.collider.gameObject == gameObject)
                {
                    ShowRecolectarButton();
                }
                else if (canvasRecolectar != null && canvasRecolectar.activeSelf)
                {
                    canvasRecolectar.SetActive(false);
                }
            }
            else
            {
                if (canvasRecolectar != null && canvasRecolectar.activeSelf)
                {
                    canvasRecolectar.SetActive(false);
                }
            }
        }
    }

    // Método para mostrar el botón de recolectar
    public virtual void ShowRecolectarButton()
    {
        if (estaAsignado)
        {
            return;
        }

        if (canvasRecolectar != null)
        {
            canvasRecolectar.SetActive(true);
        }
    }

    // Método que se llama cuando se hace clic en el botón de recolectar
    public virtual void OnRecolectarButtonClick()
    {
        if (estaAsignado)
        {
            if (canvasRecolectar != null)
            {
                canvasRecolectar.SetActive(false);
            }
            return;
        }
        
        HumanController[] allHumans = FindObjectsByType<HumanController>(FindObjectsSortMode.None);
        
        if (allHumans.Length == 0)
        {
            return;
        }
        
        HumanController availableHuman = null;

        foreach (HumanController human in allHumans)
        {
            if (human.GetState() == HumanController.HumanState.Idle)
            {
                availableHuman = human;
                break;
            }
        }

        if (availableHuman != null)
        {
            estaAsignado = true;
            humanoAsignado = availableHuman.gameObject;
            
            availableHuman.SetCurrentTarget(transform);
            availableHuman.MoveToTarget(transform.position);

            if (canvasRecolectar != null)
            {
                canvasRecolectar.SetActive(false);
            }
            
            if (timeBar != null)
            {
                timeBar.gameObject.SetActive(false);
            }
        }
    }

    // Método base para iniciar la recolección
    public virtual void StartRecolection(GameObject player)
    {
        if (!estaRecolectando)
        {
            estaRecolectando = true;

            if (timeBar != null)
            {
                timeBar.gameObject.SetActive(true);
                timeBar.StartTimer(tiempoRecoleccion);
            }

            StartCoroutine(RecolectarRecurso(player));
        }
    }

    // Corrutina para la recolección
    protected virtual IEnumerator RecolectarRecurso(GameObject player)
    {
        yield return new WaitForSeconds(tiempoRecoleccion);

        if (timeBar != null)
        {
            timeBar.StopTimer();
        }

        if (Inventory.Instance != null)
        {
            estaRecolectando = false;
            estaAsignado = false;
            
            HumanController humanController = player.GetComponent<HumanController>();
            if (humanController != null)
            {
                humanController.FinishChopping();
            }
            
            GameObject prevHuman = humanoAsignado;
            humanoAsignado = null;
            
            AgregarRecursoAlInventario(Inventory.Instance);
        }

        Destroy(gameObject);
    }

    // Método para verificar si el recurso está asignado
    public bool EstaAsignado()
    {
        return estaAsignado;
    }
    
    // Método para obtener el humano asignado
    public GameObject GetHumanoAsignado()
    {
        return humanoAsignado;
    }

    // Método para agregar el recurso al inventario, será sobrescrito por las clases hijas
    protected virtual void AgregarRecursoAlInventario(Inventory inventory)
    {
        inventory.AddResource(cantidadRecurso, "wood");
    }

    // Este método se llama automáticamente cuando se destruye el objeto
    private void OnDestroy()
    {
        if (humanoAsignado != null)
        {
            HumanController humanCtrl = humanoAsignado.GetComponent<HumanController>();
            if (humanCtrl != null)
            {
                if (humanCtrl.GetState() == HumanController.HumanState.Moving && 
                    humanCtrl.GetCurrentTarget() == transform)
                {
                    humanCtrl.FinishChopping();
                }
            }
            
            humanoAsignado = null;
        }
    }
}