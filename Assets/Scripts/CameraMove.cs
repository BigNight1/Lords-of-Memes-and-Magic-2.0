using UnityEngine;

public class CameraLimit : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float moveSpeed = 20f; // Aumentado de 5f a 20f para movimiento más rápido
    public float borderThickness = 20f; // Aumentado para una zona más amplia de detección
    public float touchSensitivity = 1f; // Nueva variable para ajustar sensibilidad del touch

    private Vector2 touchStartPosition;
    private bool isTouching = false;

    public float DerechaMax; 
    public float IzquierdaMax; 
    public float ArribaMax; 
    public float AbajoMax; 

    void Update()
    {
        // En desktop, usar el mouse
        if (Application.platform != RuntimePlatform.Android && Application.platform != RuntimePlatform.IPhonePlayer)
        {
            HandleMouseMovement();
        }
        
        // En mobile, usar touch
        HandleTouchMovement();

        // Limitar el movimiento de la cámara
        LimitCameraPosition();
    }

    private void HandleMouseMovement()
    {
        Vector3 movement = Vector3.zero;
        Vector3 mousePosition = Input.mousePosition;

        // Mover la cámara con el mouse en los bordes de la pantalla
        if (mousePosition.y >= Screen.height - borderThickness)
        {
            movement += Vector3.up;
        }
        else if (mousePosition.y <= borderThickness)
        {
            movement += Vector3.down;
        }
        
        if (mousePosition.x <= borderThickness)
        {
            movement += Vector3.left;
        }
        else if (mousePosition.x >= Screen.width - borderThickness)
        {
            movement += Vector3.right;
        }

        // Normalizar el vector de movimiento para movimiento diagonal consistente
        if (movement.magnitude > 0)
        {
            movement.Normalize();
            transform.position += movement * moveSpeed * Time.deltaTime;
        }
    }

    private void HandleTouchMovement()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStartPosition = touch.position;
                    isTouching = true;
                    break;

                case TouchPhase.Moved:
                    if (isTouching)
                    {
                        // Calcular el delta del movimiento
                        Vector2 touchDelta = (Vector2)touch.position - touchStartPosition;
                        
                        // Mover la cámara en la dirección opuesta al movimiento del dedo
                        Vector3 moveDirection = new Vector3(-touchDelta.x, -touchDelta.y, 0);
                        transform.position += moveDirection * moveSpeed * touchSensitivity * Time.deltaTime;
                        
                        // Actualizar la posición inicial para el próximo frame
                        touchStartPosition = touch.position;
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    isTouching = false;
                    break;
            }
        }
    }

    private void LimitCameraPosition()
    {
        // Usar las variables definidas como límites de la cámara
        float minX = IzquierdaMax;
        float maxX = DerechaMax;
        float minY = AbajoMax;
        float maxY = ArribaMax;

        // Limitar la posición de la cámara
        Vector3 posicionLimitada = new Vector3(
            Mathf.Clamp(transform.position.x, minX, maxX),
            Mathf.Clamp(transform.position.y, minY, maxY),
            transform.position.z
        );
        
        transform.position = posicionLimitada;
    }
}
