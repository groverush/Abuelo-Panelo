using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class Burro : MonoBehaviour
{
    [Header("Comportamiento")]
    [SerializeField] private float distanciaMinima = 2f;
    [SerializeField] private float offsetDistancia = 1.5f;
    [SerializeField] private int capacidadMaxima = 50;

    private NavMeshAgent agent;
    private Animator animator;

    private Transform objetivo; // Referencia al jugador
    private Vector3 destinoFinal;

    private List<GameObject> inventario = new List<GameObject>();

    void Start ()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void Update ()
    {
        if (objetivo != null)
        {
            float distancia = Vector3.Distance(transform.position, destinoFinal);

            if (distancia > distanciaMinima)
            {
                agent.SetDestination(destinoFinal);
            }
            else
            {
                agent.ResetPath();
            }

            float velocidad = agent.velocity.magnitude;
            animator.SetFloat("Speed_f", velocidad, 0.1f, Time.deltaTime);
        }
    }

    public void SeguirJugador ( Transform jugador )
    {
        objetivo = jugador;

        // Calcula un punto offset para no colisionar con el jugador
        Vector3 direccion = (transform.position - jugador.position).normalized;
        destinoFinal = jugador.position + direccion * offsetDistancia;
    }

    // 🧺 Inventario del burro
    public bool RecibirItem ( GameObject item )
    {
        if (inventario.Count >= capacidadMaxima)
            return false;

        inventario.Add(item);
        item.SetActive(false); // Lo "oculta" visualmente
        return true;
    }

    public bool TieneCarga ()
    {
        return inventario.Count > 0;
    }

    public GameObject ExtraerItem ()
    {
        if (inventario.Count == 0)
            return null;

        GameObject item = inventario[0];
        inventario.RemoveAt(0);
        item.SetActive(true); // Lo "reactiva" visualmente
        return item;
    }
}
