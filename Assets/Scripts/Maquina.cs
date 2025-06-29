using UnityEngine;

public class Maquina : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private int capacidadMaxima = 20;

    private int cantidadActual = 0;

    /// <summary>
    /// Devuelve cuántas sugarcanes puede recibir todavía.
    /// </summary>
    public int EspacioDisponible ()
    {
        return Mathf.Max(0, capacidadMaxima - cantidadActual);
    }

    /// <summary>
    /// Recibe una sugarcane si hay espacio.
    /// </summary>
    public bool RecibirCana ()
    {
        if (cantidadActual < capacidadMaxima)
        {
            cantidadActual++;
            UIManager.Instance.ActualizarCanaMaquina(cantidadActual, capacidadMaxima);
            Debug.Log($"🏭 Máquina recibió una sugarcane. Total: {cantidadActual}/{capacidadMaxima}");
            return true;
        }
        else
        {
            Debug.Log("⚠️ La máquina está llena. No puede recibir más sugarcanes.");
            return false;
        }
    }

    /// <summary>
    /// Para futuras mecánicas, como vaciar la máquina.
    /// </summary>
    public void Vaciar ()
    {
        cantidadActual = 0;
        UIManager.Instance.ActualizarCanaMaquina(cantidadActual, capacidadMaxima);
    }

    /// <summary>
    /// Opcional: para obtener la cantidad actual si lo necesitas desde otro script.
    /// </summary>
    public int ObtenerCantidadActual ()
    {
        return cantidadActual;
    }
}
