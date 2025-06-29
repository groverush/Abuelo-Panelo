using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class Burro : MonoBehaviour
{
    [Header("Comportamiento")]
    [SerializeField] private float distanciaMinima = 2f;
    [SerializeField] private int capacidadMaxima = 50;

    private NavMeshAgent agent;
    private Animator animator;

    private Vector3 destinoFijo;
    private bool yendoAlJugador = false;

    private PlayerController jugadorCercano;
    private List<GameObject> inventario = new List<GameObject>();

    void Start ()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Configura el NavMeshAgent para evitar empujar al jugador
        agent.stoppingDistance = 0f;
        agent.avoidancePriority = 50; // valor medio para evitar empujones
    }

    void Update ()
    {
        if (yendoAlJugador)
        {
            float distancia = Vector3.Distance(transform.position, destinoFijo);

            if (distancia > 0.2f)
            {
                agent.SetDestination(destinoFijo);
            }
            else
            {
                agent.ResetPath();
                yendoAlJugador = false;
            }

            float velocidad = agent.velocity.magnitude;
            animator.SetFloat("Speed_f", velocidad, 0.1f, Time.deltaTime);
        }

        // Seguridad: si el jugador se aleja más de lo esperado, ocultamos el texto
        if (jugadorCercano != null)
        {
            float distanciaJugador = Vector3.Distance(transform.position, jugadorCercano.transform.position);
            if (distanciaJugador > 3f)
            {
                UIManager.Instance.MostrarTextoBurro(false);
                jugadorCercano.SetCercaniaBurro(false);
                jugadorCercano = null;
            }
        }
    }

    public void SeguirJugador ( Transform jugador )
    {
        Vector3 posicionJugador = jugador.position;
        Vector3 direccion = (posicionJugador - transform.position).normalized;
        destinoFijo = posicionJugador - direccion * distanciaMinima;
        yendoAlJugador = true;
    }

    public bool RecibirItem ( GameObject item )
    {
        if (inventario.Count >= capacidadMaxima) return false;

        inventario.Add(item);
        item.SetActive(false);
        ActualizarUI();
        return true;
    }

    public bool TieneCarga () => inventario.Count > 0;

    public GameObject ExtraerItem ()
    {
        if (inventario.Count == 0) return null;

        GameObject item = inventario[0];
        inventario.RemoveAt(0);
        item.SetActive(true);
        ActualizarUI();
        return item;
    }

    private void TransferirACanaMaquina ( Maquina maquina )
    {
        int espacio = maquina.EspacioDisponible();
        int transferidas = 0;

        for (int i = inventario.Count - 1; i >= 0 && transferidas < espacio; i--)
        {
            GameObject item = inventario[i];
            Item datos = item.GetComponent<Item>();

            if (item.CompareTag("Item") && datos != null && datos.tipo == "Sugarcane")
            {
                inventario.RemoveAt(i);
                maquina.RecibirCana();
                transferidas++;
            }
        }

        if (transferidas > 0)
        {
            Debug.Log($"🐴 Burro transfirió {transferidas} sugarcanes a la máquina.");
            ActualizarUI();
        }
    }

    private void ActualizarUI ()
    {
        UIManager.Instance.ActualizarCanaBurro(inventario.Count, capacidadMaxima);
    }

    private void OnTriggerEnter ( Collider other )
    {
        if (other.CompareTag("Player"))
        {

            Debug.Log($"Entró al trigger: {other.gameObject.name}");
            UIManager.Instance.MostrarTextoBurro(true);

            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.SetCercaniaBurro(true);
            }
        }

        if (other.CompareTag("Maquina"))
        {
            Maquina maquina = other.GetComponent<Maquina>();
            if (maquina != null)
            {
                TransferirACanaMaquina(maquina);
            }
        }
    }

    private void OnTriggerExit ( Collider other )
    {
        if (other.CompareTag("Player"))
        {
            UIManager.Instance.MostrarTextoBurro(false);

            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.SetCercaniaBurro(false);
            }

            jugadorCercano = null;
        }
    }

    private void OnDrawGizmosSelected ()
    {
        Gizmos.color = Color.red;

        // Offset vertical hacia arriba para que el gizmo esté centrado en el cuerpo del burro
        Vector3 centro = transform.position + Vector3.up * 5f;

        // Ajusta el radio para que coincida con el collider
        float radio = 5f;

        Gizmos.DrawWireSphere(centro, radio);
    }

}
