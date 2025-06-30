using System.Collections;
using TMPro;
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
    private float velocidadBase;
    private float cargaActual = 0f;
    private int sugarcanesRecolectados = 0;
    private int cantidadEntregada = 0;
    private const int maxSugarcanes = 5;

    private bool estaCortando = false;
    private bool estaCercaDelBurro = false;
    private bool estaCorriendo = false;
    private bool estaCercaDeLaMesa = false;

    private Sugarcane sugarcaneActual;
    private Transform destinoDeposito;
    private GameObject botellaCercana = null;
    private GameObject barrilCercano;

    public static bool EstaCortando { get; private set; }


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
    [SerializeField] private GameObject botellaPrefab;
    [SerializeField] private GameObject botellaLlenaPrefab;

    [Header("Entrega de Jarabe")]
    [SerializeField] private Transform[] posicionesEntrega; // 5 posiciones vacías sobre la mesa
    [SerializeField] private Transform mesaDestino;


    [Header("Posición y rotación de la botella")]
    [SerializeField] private Vector3 posicionBotellaEnMano = new Vector3(0.22f, -0.77f, -0.7f);
    [SerializeField] private Vector3 rotacionBotellaEnMano = new Vector3(45f, 135f, 55f);


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
        BailarB();
        ManejarCorte();
        ManejarDeposito();
        ManejarLlamadoBurro();
        ManejarRecogerDelBurro();
        ManejarRecolectarBotella();
        ManejarLlenadoBotella();
        ManejarEntregaJarabe();
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


                    recolectarAudioSource.PlayOneShot(recolectarAudioClip, 5f);
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
    private void ManejarRecolectarBotella ()
    {
        if (Input.GetKeyDown(KeyCode.R) && botellaCercana != null && objetoTransportado == null)
        {
            Debug.Log("🍾 Intentando recoger botella cercana...");

            Item datos = botellaCercana.GetComponent<Item>();
            if (datos != null && (cargaActual + datos.peso <= capacidadCarga))
            {
                // Instanciar una nueva botella en la mano
                GameObject nuevaBotella = Instantiate(botellaPrefab, mano.position, mano.rotation);
                nuevaBotella.transform.SetParent(mano);
                nuevaBotella.transform.localPosition = posicionBotellaEnMano;
                nuevaBotella.transform.localRotation = Quaternion.Euler(rotacionBotellaEnMano);


                Rigidbody rb = nuevaBotella.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                }
                Collider col = nuevaBotella.GetComponent<Collider>();
                if (col != null)
                {
                    col.enabled = false;
                }

                objetoTransportado = nuevaBotella;
                cargaActual += datos.peso;

                // Ocultar o destruir la botella de la escena
                //botellaCercana.SetActive(false);
                Destroy(botellaCercana);
                recolectarAudioSource.PlayOneShot(recolectarAudioClip, 1f);

                UIManager.Instance.MostrarTextoInteraccion(false, "No hay objetos cerca");
                Debug.Log("🍾 Botella recogida y colocada en la mano.");
                botellaCercana = null;
            }
            else
            {
                Debug.Log("🚫 No se pudo recoger la botella: falta espacio o datos nulos.");
            }
        }
    }

    private void ManejarLlenadoBotella ()
    {
        if (Input.GetKeyDown(KeyCode.F) && objetoTransportado != null && barrilCercano != null)
        {
            Debug.Log("🛢️ Intentando llenar botella en barril cercano...");
            Item item = objetoTransportado.GetComponent<Item>();
            if (item != null && item.tipo == "Botella")
            {
                Barril barril = barrilCercano.GetComponent<Barril>();
                if (barril != null && barril.canasActuales >= 5)
                {
                    // 1️⃣ Restar caña al barril
                    barril.canasActuales -= 5;
                    barril.ActualizarUI(); // 🔄 actualizar UI de porcentaje

                    // 2️⃣ Eliminar botella vacía
                    Destroy(objetoTransportado);

                    // 3️⃣ Instanciar botella llena en la mano
                    GameObject botellaLlena = Instantiate(botellaLlenaPrefab, mano.position, mano.rotation);
                    botellaLlena.tag = "Item";
                    botellaLlena.transform.SetParent(mano);
                    botellaLlena.transform.localPosition = posicionBotellaEnMano;
                    botellaLlena.transform.localRotation = Quaternion.Euler(rotacionBotellaEnMano);

                    // 4️⃣ Desactivar física
                    Rigidbody rb = botellaLlena.GetComponent<Rigidbody>();
                    if (rb != null) { rb.isKinematic = true; rb.useGravity = false; }

                    Collider col = botellaLlena.GetComponent<Collider>();
                    if (col != null) { col.enabled = false; }

                    // 5️⃣ Asignar como objeto en mano
                    objetoTransportado = botellaLlena;

                    // 6️⃣ Cambiar tipo a BotellaLlena
                    Item nuevoItem = botellaLlena.GetComponent<Item>();
                    if (nuevoItem != null)
                        nuevoItem.tipo = "BotellaLlena";

                    // 7️⃣ Ocultar UI
                    UIManager.Instance.MostrarTextoInteraccion(false, "");

                    Debug.Log("✅ Botella llenada con 5 cañas.");

                    // 8️⃣ Notificar a la máquina si puede reanudar
                    Maquina maquina = FindObjectOfType<Maquina>();
                    if (maquina != null && maquina.TieneCanaPendiente() && !barril.EstaLleno)
                    {
                        maquina.ReanudarProcesamiento();
                    }
                }
                else
                {
                    Debug.Log("🚫 No hay suficiente caña en el barril.");
                }
            }
        }
    }
    private void ManejarEntregaJarabe ()
    {
        if (Input.GetKeyDown(KeyCode.E) && estaCercaDeLaMesa && objetoTransportado != null)
        {
            Item item = objetoTransportado.GetComponent<Item>();
            if (item != null && item.tipo == "BotellaLlena")
            {
                if (cantidadEntregada < posicionesEntrega.Length)
                {
                    // Instanciar una copia de la botella llena en la posición de la mesa
                    Transform punto = posicionesEntrega[cantidadEntregada];
                    GameObject nuevaBotella = Instantiate(botellaLlenaPrefab, punto.position, punto.rotation);
                    nuevaBotella.transform.SetParent(punto);

                    cantidadEntregada++;
                    UIManager.Instance.ActualizarProgresoJarabe(cantidadEntregada, posicionesEntrega.Length);


                    // Quitar botella de la mano
                    Destroy(objetoTransportado);
                    objetoTransportado = null;

                    UIManager.Instance.MostrarTextoInteraccion(false, "");

                    // Verificar victoria
                    if (cantidadEntregada == posicionesEntrega.Length)
                    {
                        UIManager.Instance.MostrarVictoria();
                        Debug.Log("🏆 ¡Ganaste el juego!");
                    }
                }
            }
        }
    }




    private void OnDrawGizmosSelected ()
    {
        #if UNITY_EDITOR
                if (mano != null)
                {
                    Gizmos.color = Color.cyan;
                    Matrix4x4 rotationMatrix = Matrix4x4.TRS(mano.position + mano.rotation * posicionBotellaEnMano, mano.rotation * Quaternion.Euler(rotacionBotellaEnMano), Vector3.one);
                    Gizmos.matrix = rotationMatrix;
                    Gizmos.DrawWireCube(Vector3.zero, new Vector3(0.1f, 0.25f, 0.1f)); // Tamaño estimado de botella
                    Gizmos.DrawRay(Vector3.zero, Vector3.forward * 0.3f); // Dirección hacia adelante (pico)
                }
        #endif
    }

    private void Bailar ()
    {
        bool presionoTeclaBailar = Input.GetKey(KeyCode.B);
        animator.SetBool("Dance_b", presionoTeclaBailar);
    }

    private void BailarB ()
    {
        bool presionoTeclaBailar = Input.GetKey(KeyCode.N);
        animator.SetBool("Danceb_b", presionoTeclaBailar);
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
            botellaCercana = other.gameObject;
            UIManager.Instance.MostrarTextoInteraccion(true, "Presiona R para recoger botella");
        }
        if (other.CompareTag("Barril"))
        {
            barrilCercano = other.gameObject;
            UIManager.Instance.MostrarTextoInteraccion(true, "Presiona F para llenar la botella");
        }
        if (other.CompareTag("MesaEntrega"))
        {
            estaCercaDeLaMesa = true;
            UIManager.Instance.MostrarTextoInteraccion(true, "Presiona E para entregar jarabe");
        }

    }

    private void OnTriggerExit ( Collider other )
    {
        if (other.CompareTag("Sugarcane"))
            sugarcaneActual = null;

        if (other.CompareTag("Destino"))
            destinoDeposito = null;

        if (other.CompareTag("Item") && other.gameObject == botellaCercana)
        {
            botellaCercana = null;
            UIManager.Instance.MostrarTextoInteraccion(false, "No hay objetos cerca");
        }
        if (other.CompareTag("Barril"))
        {
            barrilCercano = null;
            UIManager.Instance.MostrarTextoInteraccion(false, "");
        }
        if (other.CompareTag("MesaEntrega"))
        {
            estaCercaDeLaMesa = false;
            UIManager.Instance.MostrarTextoInteraccion(false, "");
        }
    }


}
