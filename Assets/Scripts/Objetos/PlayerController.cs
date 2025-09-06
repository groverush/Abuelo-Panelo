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
    public int maxBotellasRotas = 3;
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
    private InputAction lookAction;
    private InputAction runAction;
    private InputAction cutAction;
    private InputAction callAction;
    private InputAction giveAction; 
    private InputAction danceAction; 
    private InputAction danceBAction; 
    private InputAction pauseAction;
    private InputAction recollectAction; 

    public static bool EstaCortando { get; private set; }
    public static bool EstaBailando { get; private set; }

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
    [SerializeField] private AudioSource audioSource;

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

        // Las referencias a las acciones se pueden obtener en Awake.
        // Las suscripciones deben ir en OnEnable.
        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Look"];
        runAction = playerInput.actions["Run"];
        cutAction = playerInput.actions["Cut"];
        callAction = playerInput.actions["Call"];
        giveAction = playerInput.actions["Give"];
        danceAction = playerInput.actions["Dance 1"];
        danceBAction = playerInput.actions["Dance 2"];
        pauseAction = playerInput.actions["Pause"];
        recollectAction = playerInput.actions["HoldBottle"];
    }


    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();


        velocidadBase = velocidad; // Guardamos la velocidad original
        UIManager.Instance.ActualizarCanaJugador(sugarcanesRecolectados, maxSugarcanes);
    }

    private void OnEnable()
    {
        // Suscribir todas las acciones cuando el objeto se activa
        runAction.performed += Correr;
        runAction.canceled += Correr;

        cutAction.performed += Cortar;
        cutAction.canceled += Cortar;

        callAction.performed += LlamarBurro;

        giveAction.performed += ManejarDeposito;

        danceAction.performed += Bailar;
        danceAction.canceled += Bailar;

        danceBAction.performed += BailarB;
        danceBAction.canceled += BailarB;

        pauseAction.performed += Pausar;

        recollectAction.performed += ManejarBotellaSostenida;
        recollectAction.canceled += ManejarBotellaSostenida;
        recollectAction.performed += RecolectarBotella;
        recollectAction.canceled += RecolectarBotella;
    }

    private void OnDisable()
    {
        // Desuscribir todas las acciones cuando el objeto se desactiva
        runAction.performed -= Correr;
        runAction.canceled -= Correr;

        cutAction.performed -= Cortar;
        cutAction.canceled -= Cortar;

        callAction.performed -= LlamarBurro;

        giveAction.performed -= ManejarDeposito;

        danceAction.performed -= Bailar;
        danceAction.canceled -= Bailar;

        danceBAction.performed -= BailarB;
        danceBAction.canceled -= BailarB;

        pauseAction.performed -= Pausar;

        recollectAction.performed -= ManejarBotellaSostenida;
        recollectAction.canceled -= ManejarBotellaSostenida;
        recollectAction.performed -= RecolectarBotella;
        recollectAction.canceled -= RecolectarBotella;
    }


    void FixedUpdate()
    {
        Mover();
        ManejarLlenado();
    }


    private void Mover()
    {
        // Movimiento con el stick izquierdo
        Vector2 inputMove = playerInput.actions["Move"].ReadValue<Vector2>();
        Vector3 movimiento = new Vector3(0, 0, inputMove.y); // Mueve solo hacia adelante y atrás

        // Rotación con el stick derecho
        Vector2 inputLook = lookAction.ReadValue<Vector2>();
        float giroHorizontal = inputLook.x; // El eje X del stick derecho para la rotación

        // Aplica el movimiento y la rotación
        transform.Translate(movimiento * velocidad * Time.deltaTime);
        transform.Rotate(Vector3.up * Time.deltaTime * velocidadGiro * giroHorizontal);

        bool isMoving = Mathf.Abs(movimiento.magnitude) > 0.1f;
        bool isRunning = playerInput.actions["Run"].IsPressed();

        // 🔊 Lógica de Audio
        // Asegúrate de que AudioManager.Instance exista antes de usarlo.
        if (AudioManager.Instance != null)
        {
            if (isMoving)
            {
                if (isRunning)
                {
                    // El personaje está corriendo
                    AudioManager.Instance.StopLoop(SoundType.PlayerWalk);
                    if (!AudioManager.Instance.IsPlaying(SoundType.PlayerRun))
                    {
                        AudioManager.Instance.PlayLoop(SoundType.PlayerRun);
                    }
                }
                else
                {
                    // El personaje está caminando
                    AudioManager.Instance.StopLoop(SoundType.PlayerRun);
                    if (!AudioManager.Instance.IsPlaying(SoundType.PlayerWalk))
                    {
                        AudioManager.Instance.PlayLoop(SoundType.PlayerWalk);
                    }
                }
            }
            else // El personaje está quieto
            {
                AudioManager.Instance.StopLoop(SoundType.PlayerWalk);
                AudioManager.Instance.StopLoop(SoundType.PlayerRun);
            }
        }

        // Actualiza el Animator con la velocidad de movimiento
        float speed = Mathf.Clamp(Mathf.Abs(inputMove.y), 0, 0.5f);
        animator.SetFloat("Speed_f", speed);

        // La lógica de la cabeza del personaje también debe usar la nueva acción de rotación
        if (!animator.GetBool("Cut_b"))
        {
            float giroCabeza = Mathf.Lerp(animator.GetFloat("Head_Horizontal_f"), giroHorizontal, Time.deltaTime * 5f);
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

            // La lógica de audio fue movida al método Mover()
        }
        else if (!puedeCorrer && estaCorriendo)
        {
            estaCorriendo = false;
            velocidad = velocidadBase;
            animator.SetBool("Run_b", false);

        }
    }
    public void Cortar(InputAction.CallbackContext context)
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
                AudioManager.Instance.PlayOneShot(SoundType.CaneCollect);
            }
            Debug.Log($"🌱 Sugarcanes recolectadas: {sugarcanesRecolectados} / {maxSugarcanes}");
            UIManager.Instance.ActualizarCanaJugador(sugarcanesRecolectados, maxSugarcanes);
        }
        else
        {
            Debug.Log("🚫 Límite de sugarcanes alcanzado.");
        }
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


                    AudioManager.Instance.PlayOneShot(SoundType.CaneGive);
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

    private void LlamarBurro(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            AudioManager.Instance.PlayOneShot(SoundType.PlayerCall);
            LlamadoBurro();
        }
    }  
    
    public void LlamadoBurro()
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

            if (estaLlenandoBotella && duracionLlenado > 0f)
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
            AudioManager.Instance.PlayOneShot(SoundType.BottleRecollected);
            UIManager.Instance.MostrarTextoRecoger(false, "");
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

        AudioManager.Instance.PlayOneShot(SoundType.BottleBroken);

        Destroy(objetoTransportado, 2f);
        objetoTransportado = null;

        botellasRotas++;
        UIManager.Instance.ActualizarBotellasRotas(botellasRotas, maxBotellasRotas);

        if (botellasRotas == maxBotellasRotas)
        {
            GameManager.Instance.PerderJuego();
            Debug.Log("🏆 ¡Perdiste!");
        }

        Debug.Log("💥 Botella soltada y rota.");
    }

    private void ManejarLlenado()
{
    // Si tienes una botella en la mano y estás cerca de un barril...
    if (objetoTransportado != null && barrilCercano != null && !estaLlenandoBotella)
    {
        Item item = objetoTransportado.GetComponent<Item>();
        if (item != null && item.tipo == "Botella")
        {
            // ... y el jugador presiona el botón para llenar
            if (recollectAction.IsPressed())
            {
                Barril barril = barrilCercano.GetComponent<Barril>();
                if (barril != null && barril.canasActuales >= 5)
                {
                    // Iniciar el llenado
                    estaLlenandoBotella = true;
                    barrilEnProceso = barril;
                    cañasAntesDeLlenado = barril.canasActuales;
                    UIManager.Instance.MostrarTextoLlenado(true, "Llenando botella...");
                    AudioManager.Instance.PlayOneShot(SoundType.BottleFilled);
                    llenadoCoroutine = StartCoroutine(FinalizarLlenadoBotella(barril));
                }
            }
        }
    }

    // Lógica para cancelar el llenado si te alejas del barril
    if (estaLlenandoBotella && barrilCercano == null)
    {
        Debug.Log("⛔ Jugador se alejó del barril, cancelando llenado.");
        CancelarLlenadoBotella();
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
        UIManager.Instance.MostrarTextoLlenado(false, "");

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

        UIManager.Instance.MostrarTextoLlenado(false, "");
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

                AudioManager.Instance.PlayOneShot(SoundType.BottleDelivered);
                cantidadEntregada++;
                UIManager.Instance.ActualizarProgresoJarabe(cantidadEntregada, posicionesEntrega.Length);

                Destroy(botella);
                objetoTransportado = null;

                UIManager.Instance.MostrarTextoRecoger(false, "");

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
        if (!EstaCortando)
        {
            if (context.performed && !EstaCortando)
            {
                animator.SetBool("Dance_b", true);
            }
            else if (context.canceled)
            {
                animator.SetBool("Dance_b", false);
            }            
        }
    }

    private void BailarB(InputAction.CallbackContext context)
    {
        if (!EstaCortando)
        {
            if (context.performed && !EstaCortando)
            {
                animator.SetBool("Danceb_b", true);
            }
            else if (context.canceled)
            {
                animator.SetBool("Danceb_b", false);
            }         
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
            UIManager.Instance.MostrarTextoRecoger(true, "Recoger Botella");
        }
        if (other.CompareTag("Barril"))
        {
            barrilCercano = other.gameObject;
        }
        if (other.CompareTag("MesaEntrega"))
        {
            estaCercaDeLaMesa = true;
            UIManager.Instance.MostrarTextoEntregarJarabe(true, "Entregar Jarabe");
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
            UIManager.Instance.MostrarTextoRecoger(false, "No hay objetos cerca");
        }
        if (other.CompareTag("Barril"))
        {
            barrilCercano = null;
            UIManager.Instance.MostrarTextoLlenado(false, "");
        }
        if (other.CompareTag("MesaEntrega"))
        {
            estaCercaDeLaMesa = false;
            UIManager.Instance.MostrarTextoEntregarJarabe(false, "");
        }
    }


}
