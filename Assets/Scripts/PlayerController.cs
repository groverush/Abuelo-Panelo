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
    public GameObject animal; // Referencia al burro/burra


    [Header("Límites de Movimiento")]
    [SerializeField] private float minX = -450f;
    [SerializeField] private float maxX = 450f;
    [SerializeField] private float minY = -100f;
    [SerializeField] private float maxY = 20f;
    [SerializeField] private float minZ = -450f;
    [SerializeField] private float maxZ = 450f;

    [SerializeField] private Animator animator; // Asegúrate de asignarlo en el Inspector

    private Sugarcane sugarcaneActual;
    private Transform destinoDeposito;

    void Update ()
    {
        Mover();

        if (Input.GetKeyDown(KeyCode.C) && sugarcaneActual != null)
        {
            Cortar(sugarcaneActual);
        }

        if (Input.GetKeyDown(KeyCode.E) && destinoDeposito != null)
        {
            Depositar(destinoDeposito);
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            LlamarAnimal();
        }
        LimitarMovimiento();
    }


    public void Mover ()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 movimiento = new Vector3(0, 0, v).normalized;

        // Movimiento real del personaje
        transform.Translate(movimiento * velocidad * Time.deltaTime);
        transform.Rotate(Vector3.up * Time.deltaTime * velocidadGiro * h);

        // Calcula la velocidad como magnitud del vector de entrada
        float speed = new Vector2(h, v).magnitude;

        // Actualiza el parámetro del Animator
        animator.SetFloat("Speed_f", speed);
    }

    private void LimitarMovimiento ()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
        transform.position = pos;
        
    }

    public void Cortar ( Sugarcane sugarcane )
    {
        if (!estaCortando)
            StartCoroutine(CorteCaña(sugarcane));
    }

    private IEnumerator CorteCaña ( Sugarcane sugarcane )
    {
        estaCortando = true;
        yield return new WaitForSeconds(2f);

        sugarcane.ReducirResistencia(fuerza);
        if (sugarcane.EstaCortada())
            Debug.Log("Caña cortada.");

        estaCortando = false;
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
            animal.GetComponent<NavMeshAgent>().SetDestination(this.transform.position);
        }
    }

    public void EntregarPedido ()
    {
        Debug.Log("Pedido entregado a la abuela.");
    }

    private void OnTriggerEnter ( Collider other )
    {
        if (other.CompareTag("Caña"))
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
        if (other.CompareTag("Caña"))
            sugarcaneActual = null;

        if (other.CompareTag("Destino"))
            destinoDeposito = null;
    }
}
