using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Referencias UI")]
    [SerializeField] private TextMeshProUGUI textoCanaJugador;
    [SerializeField] private TextMeshProUGUI textoCanaBurro;
    [SerializeField] private TextMeshProUGUI textoMaquina;
    [SerializeField] private TextMeshProUGUI textoInteraccionBurro; // El panel o contenedor del texto "Presiona E para depositar"


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
            textoCanaJugador.text = $"Caña: {actual} / {maximo}";
    }

    public void ActualizarCanaBurro ( int actual, int maximo )
    {
        if (textoCanaBurro != null)
            textoCanaBurro.text = $"Caña (Burro): {actual} / {maximo}";
    }

    public void MostrarTextoBurro ( bool mostrar )
    {
        if (textoInteraccionBurro != null)
            textoInteraccionBurro.gameObject.SetActive(mostrar);

    }
    public void ActualizarCanaMaquina ( int actual, int maximo )
    {
        if (textoMaquina != null)
            textoMaquina.text = $"Máquina: {actual} / {maximo}";
    }

}
