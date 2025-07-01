using UnityEngine;
using System.Collections.Generic;

public abstract class AnimalCarga : MonoBehaviour
{
    [Header("Datos generales")]
    public string nombre;
    public int resistencia = 100;
    public float velocidadBase = 3f;
    public float capacidadCarga = 50f;
    public float cargaActual = 0f;

    protected List<GameObject> inventario = new List<GameObject>();

    public virtual bool RecibirCarga ( GameObject obj, float peso )
    {
        if ((cargaActual + peso) > capacidadCarga)
        {
            Debug.Log("?? No se puede cargar más.");
            return false;
        }

        inventario.Add(obj);
        cargaActual += peso;
        obj.SetActive(false); // Ocultamos visualmente
        Debug.Log($"? {nombre} recibió un objeto. Carga actual: {cargaActual}/{capacidadCarga}");
        return true;
    }

    public bool ValidarCarga ()
    {
        return cargaActual <= capacidadCarga;
    }

    public abstract void Mover ( Vector3 destino );
}
