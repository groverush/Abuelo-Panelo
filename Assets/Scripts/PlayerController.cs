using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

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
    private int botellasRotas = 0;
    private int maxSugarcanes = 5;
    private int maxBotellasRotas = 3;
    private bool estaCercaDelBurro = false;
    private bool estaCorriendo = false;
    private bool estaCercaDeLaMesa = false;

    private Sugarcane sugarcaneActual;
    private Transform destinoDeposito;
    private GameObject botellaCercana = null;
    private GameObject barrilCercano;

    [Header("Input Actions")]
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction runAction;
    private InputAction cutAction;
    private InputAction callAction;
    private InputAction giveAction; 
    private InputAction danceAction; 
    private InputAction danceBAction; 
    private InputAction pauseAction;
    private InputAction recollectAction; 

    public static bool EstaCortando { get; private set; }

    [SerializeField] private Collider macheteCollider;

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
    [SerializeField] private AudioSource recolectarAudioSource;
    [SerializeField] private AudioClip recolectarAudioClip;
    [SerializeField] private AudioSource llamarAudioSource;
    [SerializeField] private AudioClip llamarAudioClip;
    [SerializeField] private AudioSource llenarAudioSource;
    [SerializeField] private AudioClip llenarAudioClip;
    [SerializeField] private AudioSource dejarBotellaAudioSource;
    [SerializeField] private AudioClip dejarBotellaAudioClip;
    [SerializeField] private AudioSource romperBotellaAudioSource;
    [SerializeField] private AudioClip romperBotellaAudioClip;
    [SerializeField] private AudioSource recogerBotellaAudioSource;
    [SerializeField] private AudioClip recogerBotellaAudioClip;

    [Header("Entrega de Jarabe")]
    [SerializeField] private Transform[] posicionesEntrega; // 5 posiciones vacías sobre la mesa
    [SerializeField] private Transform mesaDestino;


    [Header("Botella")]
    [SerializeField] private GameObject botellaPrefab;
    [SerializeField] private GameObject botellaLlenaPrefab;
    [SerializeField] private Vector3 posicionBotellaEnMano = new Vector3(0.22f, -0.77f, -0.7f);
    [SerializeField] private Vector3 rotacionBotellaEnMano = new Vector3(45f, 135f, 55f);
    [SerializeField] private float duracionLlenado = 2f;
    private bool estaLlenandoBotella = false;
    private bool estaSosteniendoBotella = false;
    private Coroutine llenadoCoroutine = null;
    private int cañasAntesDeLlenado = 0;
    private Barril barrilEnProceso = null;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        moveAction = playerInput.actions["Move"];

        cutAction = playerInput.actions["Cut"];
        cutAction.performed += ManejarCorte;   // cuando se presiona
        cutAction.canceled += ManejarCorte;    // cuando se suelta

        callAction = playerInput.actions["Call"];
        callAction.performed += ManejarLlamadoBurro;

        giveAction = playerInput.actions["Give"];
        giveAction.performed += ManejarDeposito;

        runAction = playerInput.actions["Run"];
        runAction.performed += Correr;          // cuando se presiona
        runAction.canceled += Correr;   
                // cuando se suelta
        danceAction = playerInput.actions["Dance 1"];
        danceAction.performed += Bailar;          // cuando se presiona
        danceAction.canceled += Bailar;   
                // cuando se suelta
        danceBAction = playerInput.actions["Dance 2"];
        danceBAction.performed += BailarB;      // cuando se presiona
        danceBAction.canceled += BailarB;  // cuando se suelta
                 
        pauseAction = playerInput.actions["Pause"];
        pauseAction.performed += Pausar; // cuando se presiona

        recollectAction = playerInput.actions["HoldBottle"];
        recollectAction.performed += ManejarBotellaSostenida; // cuando se presiona
        recollectAction.canceled += ManejarBotellaSostenida; // cuando se suelta
        recollectAction.performed += RecolectarBotella; // cuando se presiona
        recollectAction.canceled += RecolectarBotella; // cuando se suelta

    }


    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();


        velocidadBase = velocidad; // Guardamos la velocidad original
        UIManager.Instance.ActualizarCanaJugador(sugarcanesRecolectados, maxSugarcanes);
    }

    // void Update()
    // {   
    // }

    void FixedUpdate()
    {
        Mover();
        LlenarBotella();
    }


    private void Mover()
    {
        Vector2 input = playerInput.actions["Move"].ReadValue<Vector2>();
        float h = input.x;
        float v = input.y;

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

    private void Correr(InputAction.CallbackContext context)
    {
        bool presionoShift = context.ReadValue<float>() > 0.5f;
        bool estaBailando = animator.GetBool("Dance_b") || animator.GetBool("Danceb_b");
        bool puedeCorrer = presionoShift && !animator.GetBool("Cut_b");

        if (estaBailando)
        {
            puedeCorrer = false;
        }

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

    public void ManejarCorte(InputAction.CallbackContext context)
    {
        bool estaBailando = animator.GetBool("Dance_b") || animator.GetBool("Danceb_b");

        if (estaBailando) return;

        if (context.performed)
        {
            animator.SetBool("Cut_b", true);
            EstaCortando = true;
            macheteCollider.enabled = true;
            
            InvokeRepeating(nameof(RealizarCorte), 0f, 1.5f);
        }
        else if (context.canceled)
        {
            animator.SetBool("Cut_b", false);
            EstaCortando = false;
            macheteCollider.enabled = false;

            CancelInvoke(nameof(RealizarCorte));
        }
    }

    private void RealizarCorte()
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

    public void RecolectarCana()
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

    public void DetenerPasos()
    {
        if (pasosAudioSource.isPlaying)
            pasosAudioSource.Stop();

        if (correrAudioSource.isPlaying)
            correrAudioSource.Stop();
    }
    private void ManejarDeposito(InputAction.CallbackContext context)
    {
        if (context.performed)
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

    private void ManejarLlamadoBurro(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("📢 Llamando al burro...");
            LlamarAnimal();
        }
    }    

    private void ManejarBotellaSostenida(InputAction.CallbackContext context)
    {
        // Si se está presionando U y hay una botella en mano, está sosteniéndola
        if (context.performed && objetoTransportado != null)
        {
            estaSosteniendoBotella = true;
        }

        // Si se suelta U, dejar de sostenerla y actuar según el contexto
        if (context.canceled && objetoTransportado != null && estaSosteniendoBotella)
        {
            estaSosteniendoBotella = false;

            if (estaLlenandoBotella)
            {
                CancelarLlenadoBotella();
                SoltarYRomperBotella();
            }
            else if (estaCercaDeLaMesa && objetoTransportado.GetComponent<Item>()?.tipo == "BotellaLlena")
            {
                ManejarEntregaJarabe(objetoTransportado);
            }
            else
            {
                SoltarYRomperBotella();
            }
        }
    }

    private void RecolectarBotella(InputAction.CallbackContext context)
    {
        if (botellaCercana != null && objetoTransportado == null && context.performed)
        {
            Item datos = botellaCercana.GetComponent<Item>();
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
            if (col != null) col.enabled = false;

            objetoTransportado = nuevaBotella;
            cargaActual += datos.peso;

            Destroy(botellaCercana);
            recogerBotellaAudioSource.PlayOneShot(recogerBotellaAudioClip, 3f);
            UIManager.Instance.MostrarTextoInteraccion(false, "");
            botellaCercana = null;

            estaSosteniendoBotella = true;
            Debug.Log("🍾 Botella recogida y colocada en la mano.");

        }
    }

    private void SoltarYRomperBotella()
    {
        objetoTransportado.transform.SetParent(null);

        Rigidbody rb = objetoTransportado.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.AddForce(transform.forward * 2f + Vector3.up * 1f, ForceMode.Impulse); // fuerzaSoltar
        }

        Collider col = objetoTransportado.GetComponent<Collider>();
        if (col != null) col.enabled = true;

        if (romperBotellaAudioSource != null && romperBotellaAudioClip != null)
            romperBotellaAudioSource.PlayOneShot(romperBotellaAudioClip, 1f);

        Destroy(objetoTransportado, 2f);
        objetoTransportado = null;

        botellasRotas++;
        UIManager.Instance.ActualizarBotellasRotas(botellasRotas, maxBotellasRotas);

        if (botellasRotas == maxBotellasRotas)
        {
            GameManager.Instance.PerderJuego();
            pasosAudioSource.Stop();
            correrAudioSource.Stop();


            Debug.Log("🏆 ¡Perdiste!");
        }

        Debug.Log("💥 Botella soltada y rota.");
    }

    private void LlenarBotella()
    {
        if (estaLlenandoBotella || objetoTransportado == null || barrilCercano == null) return;

        Item item = objetoTransportado.GetComponent<Item>();
        if (item != null && item.tipo == "Botella")
        {
            Barril barril = barrilCercano.GetComponent<Barril>();
            if (barril != null && barril.canasActuales >= 5)
            {
                estaLlenandoBotella = true;
                barrilEnProceso = barril;
                cañasAntesDeLlenado = barril.canasActuales;

                UIManager.Instance.MostrarTextoInteraccion(true, "Llenando botella...");
                if (llenarAudioSource != null && llenarAudioClip != null)
                    llenarAudioSource.PlayOneShot(llenarAudioClip, 1f);

                llenadoCoroutine = StartCoroutine(FinalizarLlenadoBotella(barril));
            }
        }
    }

    private IEnumerator FinalizarLlenadoBotella(Barril barril)
    {
        yield return new WaitForSeconds(duracionLlenado);

        // Restar cañas
        barril.canasActuales -= 5;
        barril.ActualizarUI();

        // Destruir botella vacía
        Destroy(objetoTransportado);

        // Instanciar botella llena
        GameObject botellaLlena = Instantiate(botellaLlenaPrefab, mano.position, mano.rotation);
        botellaLlena.tag = "Item";
        botellaLlena.transform.SetParent(mano);
        botellaLlena.transform.localPosition = posicionBotellaEnMano;
        botellaLlena.transform.localRotation = Quaternion.Euler(rotacionBotellaEnMano);

        // Desactivar física
        Rigidbody rb = botellaLlena.GetComponent<Rigidbody>();
        if (rb != null) { rb.isKinematic = true; rb.useGravity = false; }

        Collider col = botellaLlena.GetComponent<Collider>();
        if (col != null) { col.enabled = false; }

        objetoTransportado = botellaLlena;
        estaSosteniendoBotella = true;

        Item nuevoItem = botellaLlena.GetComponent<Item>();
        if (nuevoItem != null)
            nuevoItem.tipo = "BotellaLlena";

        // Ocultar texto
        UIManager.Instance.MostrarTextoInteraccion(false, "");

        // Reactivar procesamiento si es necesario
        Maquina maquina = FindFirstObjectByType<Maquina>();
        if (maquina != null && maquina.TieneCanaPendiente() && !barril.EstaLleno)
        {
            maquina.ReanudarProcesamiento();
        }

        Debug.Log("✅ Botella llenada con 5 cañas.");
        estaLlenandoBotella = false;
        llenadoCoroutine = null;
        barrilEnProceso = null;
    }

    private void CancelarLlenadoBotella()
    {
        if (llenadoCoroutine != null)
        {
            StopCoroutine(llenadoCoroutine);
            llenadoCoroutine = null;
        }

        if (barrilEnProceso != null)
        {
            barrilEnProceso.canasActuales = cañasAntesDeLlenado;
            barrilEnProceso.ActualizarUI();
        }

        estaLlenandoBotella = false;
        barrilEnProceso = null;

        UIManager.Instance.MostrarTextoInteraccion(false, "");
        Debug.Log("⛔ Llenado de botella cancelado.");
    }

    private void ManejarEntregaJarabe(GameObject botella)
    {
        if (!estaCercaDeLaMesa || botella == null) return;

        Item item = botella.GetComponent<Item>();
        if (item != null && item.tipo == "BotellaLlena")
        {
            if (cantidadEntregada < posicionesEntrega.Length)
            {
                Transform punto = posicionesEntrega[cantidadEntregada];
                GameObject nuevaBotella = Instantiate(botellaLlenaPrefab, punto.position, punto.rotation);
                nuevaBotella.transform.SetParent(punto);

                dejarBotellaAudioSource.PlayOneShot(dejarBotellaAudioClip, 1f);
                cantidadEntregada++;
                UIManager.Instance.ActualizarProgresoJarabe(cantidadEntregada, posicionesEntrega.Length);

                Destroy(botella);
                objetoTransportado = null;

                UIManager.Instance.MostrarTextoInteraccion(false, "");

                if (cantidadEntregada == posicionesEntrega.Length)
                {
                    GameManager.Instance.GanarJuego();
                    Debug.Log("🏆 ¡Ganaste el juego!");
                }
            }

        }
    }

    private void OnDrawGizmosSelected()
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

    private void Bailar(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            animator.SetBool("Dance_b", true);
        }
        else if (context.canceled)
        {
            animator.SetBool("Dance_b", false);
        }
    }

    private void BailarB(InputAction.CallbackContext context)
    {
        
        if (context.performed)
        {
            animator.SetBool("Danceb_b", true);
        }
        else if (context.canceled)
        {
            animator.SetBool("Danceb_b", false);
        }
    }

    public void Depositar(Transform destino)
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

    public void LlamarAnimal()
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

    public bool PuedeRecolectarCana()
    {
        return sugarcanesRecolectados < maxSugarcanes;
    }

    public void SetCercaniaBurro(bool estaCerca)
    {
        estaCercaDelBurro = estaCerca;
    }

    public void Pausar(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            GameManager.Instance.PausarJuego();
        }
    }

    private void OnTriggerEnter(Collider other)
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
            UIManager.Instance.MostrarTextoInteraccion(true, "Manten U para recoger una botella");
        }
        if (other.CompareTag("Barril"))
        {
            barrilCercano = other.gameObject;
        }
        if (other.CompareTag("MesaEntrega"))
        {
            estaCercaDeLaMesa = true;
            UIManager.Instance.MostrarTextoInteraccion(true, "Suelta U para entregar el jarabe");
        }

    }

    private void OnTriggerExit(Collider other)
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
