using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public GameObject player;
    // Posicion de la camara
    [SerializeField] Vector3 offset = new Vector3(0, 5, -12);

    void LateUpdate ()
    {
        // Posici�n con rotaci�n del jugador (para que la c�mara gire con �l)
        transform.position = player.transform.position + player.transform.rotation * offset;

        // Mira hacia un punto un poco adelante del jugador (no directamente al jugador)
        Vector3 lookTarget = player.transform.position + player.transform.forward * 8f;
        transform.LookAt(lookTarget);
    }
}
