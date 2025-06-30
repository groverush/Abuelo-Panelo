using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Referencias UI")]
    [SerializeField] private TextMeshProUGUI textoCanaJugador;
    [SerializeField] private TextMeshProUGUI textoCanaBurro;
    [SerializeField] private TextMeshProUGUI textoMaquina;
    [SerializeField] private TextMeshProUGUI textoInteraccion; // El panel o contenedor del texto "Presiona E para depositar"
    [SerializeField] private TextMeshProUGUI contadorProcesamientoTexto;
    [SerializeField] private TextMeshProUGUI porcentajeBarrilTexto;
    [SerializeField] private TextMeshProUGUI textoProgresoJarabe;
    [SerializeField] private GameObject panelVictoria;
    [SerializeField] private TextMeshProUGUI textoVictoria;


    private void Awake ()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }



    public void ActualizarCanaJugador ( int actual, int maximo )
    {
        if (textoCanaJugador != null)
            textoCanaJugador.text = $"Jugador: {actual} / {maximo}";
    }

    public void ActualizarCanaBurro ( int actual, int maximo )
    {
        if (textoCanaBurro != null)
            textoCanaBurro.text = $"Burro: {actual} / {maximo}";
    }

    public void MostrarTextoInteraccion( bool mostrar, string texto )
    {
        if (textoInteraccion != null)
            textoInteraccion.gameObject.SetActive(mostrar);
        textoInteraccion.text = texto;
    }
    public void ActualizarCanaMaquina ( int actual, int maximo )
    {
        if (textoMaquina != null)
            textoMaquina.text = $"Máquina: {actual} / {maximo}";
    }


    public void ActualizarContadorProcesamiento ( int segundos )
    {
        if (contadorProcesamientoTexto != null)
            contadorProcesamientoTexto.text = $"Procesando: {segundos}s";
    }

    public void ActualizarPorcentajeBarril ( int actual, int maximo )
    {
        if (porcentajeBarrilTexto != null)
        {
            float porcentaje = ((float)actual / maximo) * 100f;
            porcentajeBarrilTexto.text = $"Barril: {porcentaje:F0}%";
        }
    }

    public void MostrarContadorProcesamiento ( bool mostrar )
    {
        if (contadorProcesamientoTexto != null)
            contadorProcesamientoTexto.gameObject.SetActive(mostrar);
    }

    public void MostrarPorcentajeBarril ( bool mostrar )
    {
        if (porcentajeBarrilTexto != null)
            porcentajeBarrilTexto.gameObject.SetActive(mostrar);
    }

    public void ActualizarProgresoJarabe ( int actual, int total )
    {
        if (textoProgresoJarabe != null)
            textoProgresoJarabe.text = $"Jarabe {actual} / {total}";
    }

    public void MostrarVictoria ( string mensaje = "¡VICTORIA!" )
    {
        if (panelVictoria != null)
            panelVictoria.SetActive(true);

        if (textoVictoria != null)
            textoVictoria.text = mensaje;
    }

}
