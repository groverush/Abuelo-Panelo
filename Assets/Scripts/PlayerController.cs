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

    [Header("Visual")]
    public Transform meshTransform;
    [SerializeField] private Animator animator;

    private Sugarcane sugarcaneActual;
    private Transform destinoDeposito;

    public static bool EstaCortando { get; private set; }

    // ✅ Sistema de cañas recolectadas
    private int canasRecolectadas = 0;
    private const int maxCanas = 5;

    void Start ()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void FixedUpdate ()
    {
        Mover();

        // 🔁 Animación de corte mientras se mantenga "C"
        bool cortando = Input.GetKey(KeyCode.C);
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

        // Depositar
        if (Input.GetKeyDown(KeyCode.E) && destinoDeposito != null)
        {
            Debug.Log("📦 Depositar objeto...");
            Depositar(destinoDeposito);
        }

        // Llamar al animal (burro)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("📢 Llamando al burro...");
            LlamarAnimal();
        }

        // Recoger del burro
        if (Input.GetKeyDown(KeyCode.R) && animal != null)
        {
            float distancia = Vector3.Distance(transform.position, animal.transform.position);
            if (distancia <= 2.5f)
            {
                Burro burro = animal.GetComponent<Burro>();
                if (burro != null && burro.TieneCarga())
                {
                    GameObject item = burro.ExtraerItem();
                    if (item != null)
                    {
                        Recolectar(item);
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

        // Rotación del cuerpo
        if (v < 0)
            meshTransform.localRotation = Quaternion.Euler(0, 180, 0);
        else if (v > 0)
            meshTransform.localRotation = Quaternion.identity;

        // Animaciones
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
                Debug.Log("✅ Caña cortada.");
            }
        }
    }

    public void Recolectar ( GameObject item )
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

            // Por defecto deposita en destino normal
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
        return canasRecolectadas < maxCanas;
    }

    public void RecolectarCana ()
    {
        if (PuedeRecolectarCana())
        {
            canasRecolectadas++;
            Debug.Log($"🌱 Cañas recolectadas: {canasRecolectadas} / {maxCanas}");
        }
        else
        {
            Debug.Log("🚫 Límite de cañas alcanzado.");
        }
    }

    private void OnTriggerEnter ( Collider other )
    {
        if (other.CompareTag("Sugarcane"))
        {
            sugarcaneActual = other.GetComponent<Sugarcane>();
        }

        if (other.CompareTag("Destino"))
        {
            destinoDeposito = other.transform;
        }

        if (other.CompareTag("Item"))
        {
            Recolectar(other.gameObject);
        }
    }

    private void OnTriggerExit ( Collider other )
    {
        if (other.CompareTag("Sugarcane"))
            sugarcaneActual = null;

        if (other.CompareTag("Destino"))
            destinoDeposito = null;
    }
}
