using UnityEngine;
using TMPro;

public class Barril : MonoBehaviour
{
    [SerializeField] private int capacidadMaxima = 20;
    [SerializeField] private TextMeshProUGUI textoPorcentaje;

    private int canasActuales = 0;

    public bool EstaLleno => canasActuales >= capacidadMaxima;

    private void Start ()
    {
        ActualizarUI();
    }

    public int EspacioDisponible ()
    {
        return capacidadMaxima - canasActuales;
    }

    public void RecibirCanas ( int cantidad )
    {
        canasActuales += cantidad;
        canasActuales = Mathf.Clamp(canasActuales, 0, capacidadMaxima);
        ActualizarUI();
    }

    private void ActualizarUI ()
    {
        if (canasActuales <= 0)
        {
            textoPorcentaje.gameObject.SetActive(false);
            return;
        }

        textoPorcentaje.gameObject.SetActive(true);
        float porcentaje = ((float)canasActuales / capacidadMaxima) * 100f;
        textoPorcentaje.text = $"{porcentaje:F0}%";
    }
}
