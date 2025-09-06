using UnityEngine;

public class SeguirObjeto : MonoBehaviour
{
    public Transform objetoASeguir; // Aquí arrastrarás al burro
    public Vector3 offset; // Opcional, para ajustar la posición

    private Transform camaraPrincipal;

    void Start()
    {
        camaraPrincipal = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (objetoASeguir != null)
        {
            // Posiciona el objeto de la UI en la misma posición que el burro,
            // con un pequeño desplazamiento.
            transform.position = objetoASeguir.position + offset;

            // Rota el objeto de UI para que siempre mire a la cámara,
            // usando un enfoque de "billboard".
            transform.LookAt(transform.position + camaraPrincipal.forward);
        }
    }
}