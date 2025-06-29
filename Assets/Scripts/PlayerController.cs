using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    [Header("Atributos")]
    public float resistencia = 100f;
    public float fuerza = 10f;
    public float velocidad = 5f;
    public float velocidadGiro = 5f;
    public float capacidadCarga = 50f;

    private float cargaActual = 0f;
    private bool estaCortando = false;

    [Header("Referencias")]
    public Transform mano;
    public GameObject objetoTransportado;
    public GameObject animal;
    [SerializeField] private GameObject sugarcanePrefab; // Prefab con Item + tag "Item"

    [Header("Visual")]
    public Transform meshTransform;
    [SerializeField] private Animator animator;

    private Sugarcane sugarcaneActual;
    private Transform destinoDeposito;

    public static bool EstaCortando { get; private set; }

    private int sugarcanesRecolectados = 0;
    private const int maxSugarcanes = 5;
    private bool estaCercaDelBurro = false;

    void Start ()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        UIManager.Instance.ActualizarCanaJugador(sugarcanesRecolectados, maxSugarcanes);
    }

    void FixedUpdate ()
    {
        Mover();
        ManejarCorte();
        ManejarDeposito();
        ManejarLlamadoBurro();
        ManejarRecogerDelBurro();
    }

    private void ManejarCorte ()
    {
        bool cortando = Input.GetKey(KeyCode.Space);
        animator.SetBool("Cut_b", cortando);
        EstaCortando = cortando;

        if (cortando && !estaCortando)
        {
            InvokeRepeating(nameof(RealizarCorte), 0f, 1.5f);
            estaCortando = true;
        }
        else if (!cortando && estaCortando)
        {
            CancelInvoke(nameof(RealizarCorte));
            estaCortando = false;
        }
    }

    private void ManejarDeposito ()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (destinoDeposito != null)
            {
                Depositar(destinoDeposito);
                return;
            }

            if (animal != null && estaCercaDelBurro)
            {
                Burro burro = animal.GetComponent<Burro>();
                if (burro != null && sugarcanesRecolectados > 0)
                {
                    for (int i = 0; i < sugarcanesRecolectados; i++)
                    {
                        GameObject nuevaSugarcane = Instantiate(sugarcanePrefab);
                        nuevaSugarcane.tag = "Item";

                        Item item = nuevaSugarcane.GetComponent<Item>();
                        if (item == null)
                            item = nuevaSugarcane.AddComponent<Item>();

                        item.peso = 10f;
                        item.tipo = "Sugarcane";

                        burro.RecibirItem(nuevaSugarcane);
                    }

                    Debug.Log($"🐴 Se transfirieron {sugarcanesRecolectados} sugarcanes al burro.");
                    sugarcanesRecolectados = 0;
                    UIManager.Instance.ActualizarCanaJugador(sugarcanesRecolectados, maxSugarcanes);
                }
                else
                {
                    Debug.Log("🚫 No tienes sugarcanes para transferir.");
                }
            }
            else
            {
                Debug.Log("❌ El burro está demasiado lejos para interactuar.");
            }
        }
    }

    private void ManejarLlamadoBurro ()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("📢 Llamando al burro...");
            LlamarAnimal();
        }
    }

    private void ManejarRecogerDelBurro ()
    {
        if (Input.GetKeyDown(KeyCode.R) && animal != null)
        {
            float distancia = Vector3.Distance(transform.position, animal.transform.position);
            if (distancia <= 2.5f)
            {
                Burro burro = animal.GetComponent<Burro>();
                if (burro != null && burro.TieneCarga())
                {
                    GameObject item = burro.ExtraerItem();
                    if (item != null && objetoTransportado == null)
                    {
                        objetoTransportado = item;
                        item.transform.SetParent(mano);
                        item.transform.localPosition = Vector3.zero;

                        Item datos = item.GetComponent<Item>();
                        cargaActual += datos != null ? datos.peso : 0;

                        Debug.Log("🎒 Objeto recuperado del burro");
                    }
                }
            }
        }
    }

    public void Mover ()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 movimiento = new Vector3(0, 0, v).normalized;
        transform.Translate(movimiento * velocidad * Time.deltaTime);
        transform.Rotate(Vector3.up * Time.deltaTime * velocidadGiro * h);

        if (v < 0)
            meshTransform.localRotation = Quaternion.Euler(0, 180, 0);
        else if (v > 0)
            meshTransform.localRotation = Quaternion.identity;

        float speed = Mathf.Clamp(Mathf.Abs(v), 0, 0.5f);
        animator.SetFloat("Speed_f", speed);

        if (!animator.GetBool("Cut_b"))
        {
            float giroCabeza = Mathf.Lerp(animator.GetFloat("Head_Horizontal_f"), h, Time.deltaTime * 5f);
            animator.SetFloat("Head_Horizontal_f", giroCabeza);
        }
        else
        {
            animator.SetFloat("Head_Horizontal_f", 0f);
        }
    }

    private void RealizarCorte ()
    {
        if (sugarcaneActual != null)
        {
            sugarcaneActual.ReducirResistencia(fuerza);
            if (sugarcaneActual.EstaCortada())
            {
                Debug.Log("✅ Sugarcane cortada.");
            }
        }
    }

    public void Recolectar ()
    {
        if (PuedeRecolectarCana())
        {
            sugarcanesRecolectados++;
            Debug.Log($"🌱 Sugarcanes recolectadas: {sugarcanesRecolectados} / {maxSugarcanes}");
            UIManager.Instance.ActualizarCanaJugador(sugarcanesRecolectados, maxSugarcanes);
        }
        else
        {
            Debug.Log("🚫 Límite de sugarcanes alcanzado.");
        }
    }

    public void RecolectarItem ( GameObject item )
    {
        if (cargaActual < capacidadCarga && objetoTransportado == null)
        {
            Item datos = item.GetComponent<Item>();
            if (datos != null && (cargaActual + datos.peso <= capacidadCarga))
            {
                objetoTransportado = item;
                item.transform.SetParent(mano);
                item.transform.localPosition = Vector3.zero;
                cargaActual += datos.peso;
            }
        }
    }

    public void Depositar ( Transform destino )
    {
        if (objetoTransportado != null)
        {
            Item datos = objetoTransportado.GetComponent<Item>();

            if (destino.CompareTag("Burro"))
            {
                Burro burro = destino.GetComponent<Burro>();
                if (burro != null && burro.RecibirItem(objetoTransportado))
                {
                    cargaActual -= datos.peso;
                    objetoTransportado = null;
                    return;
                }
            }

            objetoTransportado.transform.SetParent(destino);
            objetoTransportado.transform.position = destino.position;
            cargaActual -= datos.peso;
            objetoTransportado = null;
        }
    }

    public void LlamarAnimal ()
    {
        if (animal != null)
        {
            Burro burro = animal.GetComponent<Burro>();
            if (burro != null)
            {
                burro.SeguirJugador(this.transform);
            }
        }
    }

    public void EntregarPedido ()
    {
        Debug.Log("📦 Pedido entregado a la abuela.");
    }

    public bool PuedeRecolectarCana ()
    {
        return sugarcanesRecolectados < maxSugarcanes;
    }

    public void SetCercaniaBurro ( bool estaCerca )
    {
        estaCercaDelBurro = estaCerca;
    }

    private void OnTriggerEnter ( Collider other )
    {
        if (other.CompareTag("Sugarcane"))
            sugarcaneActual = other.GetComponent<Sugarcane>();

        if (other.CompareTag("Destino"))
            destinoDeposito = other.transform;

        if (other.CompareTag("Item"))
            RecolectarItem(other.gameObject);
    }

    private void OnTriggerExit ( Collider other )
    {
        if (other.CompareTag("Sugarcane"))
            sugarcaneActual = null;

        if (other.CompareTag("Destino"))
            destinoDeposito = null;
    }
}
