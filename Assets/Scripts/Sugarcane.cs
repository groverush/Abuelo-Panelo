using UnityEngine;

public class Sugarcane : MonoBehaviour
{
    public float resistencia = 30f;
    public GameObject piezaDeCanaPrefab;
    public float fuerzaPorGolpe = 10f;

    private void OnTriggerEnter ( Collider other )
    {
        if (other.CompareTag("Machete") && PlayerController.EstaCortando)
        {
            ReducirResistencia(fuerzaPorGolpe);
        }
    }

    public void ReducirResistencia ( float cantidad )
    {
        resistencia -= cantidad;
        if (resistencia <= 0f)
        {
            CortarCompleto();
        }
    }

    public bool EstaCortada ()
    {
        return resistencia <= 0f;
    }

    private void CortarCompleto ()
    {
        Debug.Log("✅ ¡Caña completamente cortada!");

        if (piezaDeCanaPrefab != null)
        {
            // Instanciamos la pieza en una posición levemente elevada
            Instantiate(piezaDeCanaPrefab, transform.position + Vector3.up * 1.5f, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
