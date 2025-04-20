using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class TextoRecolectar : MonoBehaviour, IPointerClickHandler
{
    public Text texto; // Referencia al componente Text
    public float tiempoVisible = 2f; // Tiempo que el texto será visible
    
    // Referencia al RecursoScript que contiene la función de recolección
    public GameObject recursoAsociado;
    private Button botonRecolectar; 

    private void Start()
    {
        botonRecolectar = GetComponent<Button>();

        // Asegurarse de que el texto esté oculto al inicio
        if (botonRecolectar != null)
        {
            botonRecolectar.onClick.AddListener(OnBotonClick);
            Debug.Log("Botón de recolectar encontrado y configurado");
        }
        else
        {
            Debug.LogError("Botón de recolectar no encontrado en TextoRecolectar");
        }
        
        // Debug para verificar si el script está siendo inicializado
        Debug.Log("TextoRecolectar inicializado en: " + gameObject.name);
    }

    // Método para el evento onClick del botón
    private void OnBotonClick()
    {
        Debug.Log("Botón de recolectar presionado - OnBotonClick en " + gameObject.name);
        ActivarRecoleccion();
    }
    
    // Método central para activar la recolección
    private void ActivarRecoleccion()
    {
        if (recursoAsociado != null)
        {
            Debug.Log("Recurso asociado encontrado: " + recursoAsociado.name);
            
            // Intentar obtener cualquier tipo de RecursoScript primero
            RecursoScript recursoScript = recursoAsociado.GetComponent<RecursoScript>();
            
            if (recursoScript != null)
            {
                // Llamar al método de recolección
                Debug.Log("Llamando a OnRecolectarButtonClick en RecursoScript de " + recursoAsociado.name);
                recursoScript.OnRecolectarButtonClick();
                Debug.Log("Activando recolección de recurso: " + recursoAsociado.name);
                return;
            }
            
            // Intentar con scripts específicos si no se encontró RecursoScript
            arbol arbolScript = recursoAsociado.GetComponent<arbol>();
            if (arbolScript != null)
            {
                Debug.Log("Llamando a OnRecolectarButtonClick en arbol de " + recursoAsociado.name);
                arbolScript.OnRecolectarButtonClick();
                Debug.Log("Activando recolección de árbol desde botón");
                return;
            }
            
            roca rocaScript = recursoAsociado.GetComponent<roca>();
            if (rocaScript != null)
            {
                Debug.Log("Llamando a OnRecolectarButtonClick en roca de " + recursoAsociado.name);
                rocaScript.OnRecolectarButtonClick();
                Debug.Log("Activando recolección de roca desde botón");
                return;
            }
            
            Debug.LogError("Error: El objeto " + recursoAsociado.name + " no tiene ningún componente de recurso");
        }
        else
        {
            Debug.LogWarning("No hay recurso asociado para recolectar");
        }
    }

    // Método para mostrar el mensaje de recolección
    public void MostrarMensaje(string mensaje, GameObject objetoRecurso = null)
    {
        if (texto != null)
        {
            texto.text = mensaje; // Actualizar el texto del mensaje
            texto.gameObject.SetActive(true); // Mostrar el texto
            
            // Si se proporciona un objeto recurso, actualizar la referencia
            if (objetoRecurso != null)
            {
                AsignarRecurso(objetoRecurso);
            }
            
            StartCoroutine(OcultarMensaje()); // Iniciar la coroutine para ocultar el mensaje
        }
    }

    private IEnumerator OcultarMensaje()
    {
        yield return new WaitForSeconds(tiempoVisible); // Esperar el tiempo especificado
        texto.gameObject.SetActive(false); // Ocultar el texto
    }
    
    // Implementación de IPointerClickHandler para detectar clics directos en el texto
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Clic detectado directamente en TextoRecolectar - OnPointerClick");
        ActivarRecoleccion();
    }
    
    // Método para asignar el recurso asociado
    public void AsignarRecurso(GameObject nuevoRecurso)
    {
        recursoAsociado = nuevoRecurso;
        Debug.Log("Recurso asociado establecido a: " + nuevoRecurso.name);
    }
}