using UnityEngine;

public class Sugarcane : MonoBehaviour
{
    public float resistencia = 20f;

    public void ReducirResistencia ( float cantidad )
    {
        resistencia -= cantidad;
        if (resistencia <= 0)
            CortarCompletada();
    }

    public bool EstaCortada ()
    {
        return resistencia <= 0;
    }

    private void CortarCompletada ()
    {
        Debug.Log("¡Cortada!");
        // Puedes instanciar un "item" recolectable aquí
    }
}
