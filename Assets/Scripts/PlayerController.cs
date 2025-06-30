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
    private bool machetePuedeSonar = false;

    [Header("Referencias")]
    public Transform mano;
    public GameObject objetoTransportado;
    public GameObject animal;
    [SerializeField] private GameObject sugarcanePrefab;

    [Header("Visual")]
    public Transform meshTransform;
    [SerializeField] private Animator animator;

    [Header("Sonidos")]
    [SerializeField] private AudioSource pasosAudioSource;
    [SerializeField] private AudioSource correrAudioSource;
    [SerializeField] private AudioClip pasosAudioClip;
    [SerializeField] private AudioClip correrAudioClip;
    [SerializeField] private Collider macheteCollider;
    [SerializeField] private AudioSource recolectarAudioSource;
    [SerializeField] private AudioClip recolectarAudioClip;
    [SerializeField] private AudioSource llamarAudioSource;
    [SerializeField] private AudioClip llamarAudioClip;

    private Sugarcane sugarcaneActual;
    private Transform destinoDeposito;

    public static bool EstaCortando { get; private set; }

    private int sugarcanesRecolectados = 0;
    private const int maxSugarcanes = 5;
    private bool estaCercaDelBurro = false;

    private float velocidadBase;
    private bool estaCorriendo = false;

    void Start ()
    {
        if (animator == null)
            animator = GetComponent<Animator>();


        velocidadBase = velocidad; // Guardamos la velocidad original
        UIManager.Instance.ActualizarCanaJugador(sugarcanesRecolectados, maxSugarcanes);
    }

    void FixedUpdate ()
    {
        Mover();
        Correr();
        Bailar();
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
            macheteCollider.enabled = true;
            InvokeRepeating(nameof(RealizarCorte), 0f, 1.5f);
            estaCortando = true;
        }
        else if (!cortando && estaCortando)
        {
            macheteCollider.enabled = false;
            CancelInvoke(nameof(RealizarCorte));
            estaCortando = false;
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

        bool estaMoviendose = Mathf.Abs(v) > 0.1f;

        if (estaMoviendose)
        {
            if (estaCorriendo)
            {
                if (!correrAudioSource.isPlaying)
                {
                    correrAudioSource.clip = correrAudioClip;
                    correrAudioSource.loop = true;
                    correrAudioSource.Play();
                }

                if (pasosAudioSource.isPlaying)
                    pasosAudioSource.Stop();
            }
            else
            {
                if (!pasosAudioSource.isPlaying)
                {
                    pasosAudioSource.clip = pasosAudioClip;
                    pasosAudioSource.loop = true;
                    pasosAudioSource.Play();
                }

                if (correrAudioSource.isPlaying)
                    correrAudioSource.Stop();
            }
        }
        else
        {
            if (pasosAudioSource.isPlaying)
                pasosAudioSource.Stop();

            if (correrAudioSource.isPlaying)
                correrAudioSource.Stop();
        }

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


    private void Correr ()
    {
        bool presionoShift = Input.GetKey(KeyCode.LeftShift);
        bool puedeCorrer = presionoShift && !animator.GetBool("Cut_b");

        if (puedeCorrer && !estaCorriendo)
        {
            estaCorriendo = true;
            velocidad = velocidadBase * 2f;
            animator.SetBool("Run_b", true);

            if (correrAudioSource != null && correrAudioClip != null)
            {
                correrAudioSource.clip = correrAudioClip;
                correrAudioSource.loop = true;
                correrAudioSource.Play();
                pasosAudioSource.Stop();
            }

        }
        else if (!puedeCorrer && estaCorriendo)
        {
            estaCorriendo = false;
            velocidad = velocidadBase;
            animator.SetBool("Run_b", false);

            if (correrAudioSource != null && correrAudioSource.isPlaying)
                correrAudioSource.Stop();
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
    private void Bailar ()
    {
        bool presionoTeclaBailar = Input.GetKey(KeyCode.B);
        animator.SetBool("Dance_b", presionoTeclaBailar);
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
                if (llamarAudioSource != null && llamarAudioClip != null)
                {
                    llamarAudioSource.PlayOneShot(llamarAudioClip, 1f);
                }
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

    public void Recolectar ()
    {
        if (PuedeRecolectarCana())
        {
            sugarcanesRecolectados++;
            if (sugarcanesRecolectados < maxSugarcanes)
            {
                recolectarAudioSource.PlayOneShot(recolectarAudioClip, 1f);
            }
            recolectarAudioSource.PlayOneShot(recolectarAudioClip);
            Debug.Log($"🌱 Sugarcanes recolectadas: {sugarcanesRecolectados} / {maxSugarcanes}");
            UIManager.Instance.ActualizarCanaJugador(sugarcanesRecolectados, maxSugarcanes);
        }
        else
        {
            Debug.Log("🚫 Límite de sugarcanes alcanzado.");
        }
    }

    public void RecolectarBotella ( GameObject item )
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

    private void OnTriggerEnter ( Collider other )
    {
        if (other.CompareTag("Sugarcane"))
        {
            sugarcaneActual = other.GetComponent<Sugarcane>();            
        }

        if (other.CompareTag("Destino"))
            destinoDeposito = other.transform;

        if (other.CompareTag("Item"))
        {
            RecolectarBotella(other.gameObject);
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
