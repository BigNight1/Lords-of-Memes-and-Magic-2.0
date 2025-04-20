using UnityEngine;
using System.Collections;

public class HumanController : MonoBehaviour
{
    public float moveSpeed = 2.0f;
    public float fixedZPosition = -0.1f;
    private Vector3 targetPosition;
    private bool isMoving = false;
    private Transform currentTarget;
    private Animator animator;
    
    // Estados para el humano
    public enum HumanState
    {
        Idle,           // No está haciendo nada
        Moving,         // Se está moviendo hacia un objetivo
        Chopping,       // Está talando un árbol
        Mining,         // Está picando una roca
        Busy            // Está ocupado con cualquier otra tarea
    }

    private HumanState currentState = HumanState.Idle;

    void Start()
    {
        
        
        // Asegurar que la posición Z sea la correcta
        Vector3 startPos = transform.position;
        transform.position = new Vector3(startPos.x, startPos.y, fixedZPosition);
        
        Debug.Log("HumanController inicializado en: " + gameObject.name);
    }

    void Update()
    {
        // Mantener siempre la posición Z fija
        if (transform.position.z != fixedZPosition)
        {
            Vector3 currentPos = transform.position;
            transform.position = new Vector3(currentPos.x, currentPos.y, fixedZPosition);
        }
        
        // Verificar si el objetivo todavía existe
        if (currentTarget != null && currentState != HumanState.Idle)
        {
            // El objetivo aún existe, todo bien
        }
        else if (currentTarget == null && currentState != HumanState.Idle)
        {
            // El objetivo ha sido destruido, pero el humano sigue en un estado no-idle
            Debug.LogWarning(gameObject.name + " tenía un objetivo que ha sido destruido. Volviendo a estado Idle.");
            SetState(HumanState.Idle);
            isMoving = false;
            
            // Detener animación si existe
            if (animator != null)
            {
                animator.SetBool("IsWalking", false);
                animator.SetBool("IsWorking", false);
            }
            
            return; // Salir de Update para evitar continuar con el movimiento
        }
        
        if (isMoving && currentState == HumanState.Moving)
        {
            // Verificar nuevamente si el objetivo todavía existe
            if (currentTarget == null)
            {
                Debug.LogWarning(gameObject.name + " estaba moviéndose hacia un objetivo que ya no existe. Deteniéndose.");
                isMoving = false;
                SetState(HumanState.Idle);
                
                // Detener animación si existe
                if (animator != null)
                {
                    animator.SetBool("IsWalking", false);
                }
                
                return;
            }
            
            // Moverse hacia la posición objetivo
            Vector3 currentPos = transform.position;
            Vector3 targetWithCorrectZ = new Vector3(targetPosition.x, targetPosition.y, fixedZPosition);
            
            transform.position = Vector3.MoveTowards(
                currentPos,
                targetWithCorrectZ,
                moveSpeed * Time.deltaTime
            );

            // Verificar si ha llegado al objetivo
            if (Vector2.Distance(new Vector2(transform.position.x, transform.position.y), 
                               new Vector2(targetPosition.x, targetPosition.y)) < 0.1f)
            {
                isMoving = false;
                Debug.Log(gameObject.name + " ha llegado al destino");
                
                // Si hay un objetivo actual, comenzar a recolectar
                if (currentTarget != null)
                {
                    StartResourceCollection();
                }
                else
                {
                    // Si no hay objetivo, volver a estado inactivo
                    Debug.LogWarning(gameObject.name + " llegó al destino pero el objetivo ya no existe");
                    SetState(HumanState.Idle);
                    
                    // Detener animaciones
                    if (animator != null)
                    {
                        animator.SetBool("IsWalking", false);
                    }
                }
            }
        }
    }

    // Método para mover al humano a una posición específica
    public void MoveToTarget(Vector3 position)
    {
        if (currentState != HumanState.Idle)
        {
            Debug.LogWarning(gameObject.name + " no puede moverse porque no está inactivo. Estado actual: " + currentState);
            return;
        }
        
        targetPosition = position;
        isMoving = true;
        SetState(HumanState.Moving);
        
        // Actualizar animación si existe
        if (animator != null)
        {
            animator.SetBool("IsWalking", true);
        }
        
        Debug.Log(gameObject.name + " comenzó a moverse hacia: " + position);
    }

    // Método para establecer el objetivo actual
    public void SetCurrentTarget(Transform target)
    {
        if (currentState != HumanState.Idle)
        {
            Debug.LogWarning(gameObject.name + " no puede asignarse a un nuevo objetivo porque no está inactivo. Estado actual: " + currentState);
            return;
        }
        
        // Verificar si el objetivo ya está asignado a otro humano
        RecursoScript recurso = target.GetComponent<RecursoScript>();
        if (recurso != null && recurso.EstaAsignado() && recurso.GetHumanoAsignado() != gameObject)
        {
            Debug.LogWarning(gameObject.name + " no puede asignarse a " + target.name + " porque ya está asignado a otro humano.");
            return;
        }
        
        currentTarget = target;
        Debug.Log(gameObject.name + " tiene un nuevo objetivo: " + (target != null ? target.name : "null"));
    }

    // Método para iniciar la recolección del recurso
    private void StartResourceCollection()
    {
        if (currentTarget == null)
        {
            Debug.LogError(gameObject.name + " no tiene un objetivo para recolectar");
            SetState(HumanState.Idle);
            return;
        }

        Debug.Log(gameObject.name + " comenzando recolección de recurso: " + currentTarget.name);
        
        // Detener animación de caminar
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsWorking", true);
        }

        // Comprobar qué tipo de recurso es y llamar al método correspondiente
        arbol arbolTarget = currentTarget.GetComponent<arbol>();
        if (arbolTarget != null)
        {
            SetState(HumanState.Chopping);
            Debug.Log(gameObject.name + " está talando un árbol");
            arbolTarget.StartChopping(gameObject);
            return;
        }

        roca rocaTarget = currentTarget.GetComponent<roca>();
        if (rocaTarget != null)
        {
            SetState(HumanState.Mining);
            Debug.Log(gameObject.name + " está minando una roca");
            rocaTarget.StartMining(gameObject);
            return;
        }

        // Recurso genérico
        RecursoScript recursoTarget = currentTarget.GetComponent<RecursoScript>();
        if (recursoTarget != null)
        {
            SetState(HumanState.Busy);
            Debug.Log(gameObject.name + " está recolectando un recurso genérico");
            recursoTarget.StartRecolection(gameObject);
            return;
        }

        Debug.LogError(gameObject.name + " no pudo determinar el tipo de recurso a recolectar");
        SetState(HumanState.Idle); // Volver a estado inactivo si no pudo iniciar la recolección
    }

    // Método para finalizar la tala/recolección
    public void FinishChopping()
    {
        currentTarget = null;
        
        // Detener animación de trabajo
        if (animator != null)
        {
            animator.SetBool("IsWorking", false);
        }
        
        // Marcar como disponible nuevamente
        SetState(HumanState.Idle);
        
        Debug.Log(gameObject.name + " ha terminado de recolectar y ahora está disponible");
    }

    // Método para establecer el estado del humano
    private void SetState(HumanState newState)
    {
        HumanState oldState = currentState;
        currentState = newState;
        Debug.Log(gameObject.name + " cambió de estado: " + oldState + " -> " + newState);
    }

    // Método para verificar si el humano está disponible
    public bool IsAvailable()
    {
        return currentState == HumanState.Idle;
    }
    
    // Obtener estado actual
    public HumanState GetState()
    {
        return currentState;
    }
    
    // Obtener el objetivo actual
    public Transform GetCurrentTarget()
    {
        return currentTarget;
    }
}
