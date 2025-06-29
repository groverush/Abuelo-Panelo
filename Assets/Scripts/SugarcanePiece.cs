using UnityEngine;

public class SugarcanePiece : MonoBehaviour
{
    public float velocidadFlotacion = 1f;
    public float amplitud = 0.25f;
    public float velocidadRotacion = 60f;

    private Vector3 posicionInicial;

    private void Start ()
    {
        posicionInicial = transform.position;
    }

    private void Update ()
    {
        // Movimiento de flotación
        float nuevaY = posicionInicial.y + Mathf.Sin(Time.time * velocidadFlotacion) * amplitud;
        transform.position = new Vector3(posicionInicial.x, nuevaY, posicionInicial.z);

        // Rotación constante
        transform.Rotate(Vector3.up * velocidadRotacion * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter ( Collider other )
    {
        if (other.CompareTag("Player"))
        {
            PlayerController jugador = other.GetComponent<PlayerController>();
            if (jugador != null && jugador.PuedeRecolectarCana())
            {
                jugador.Recolectar();
                Destroy(gameObject);
            }
        }
    }
}
