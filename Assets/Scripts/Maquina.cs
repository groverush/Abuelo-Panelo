using UnityEngine;

public class Maquina : MonoBehaviour
{
    [SerializeField] private int capacidadInterna = 100;
    [SerializeField] private int cantidadProcesar = 5;
    [SerializeField] private float tiempoProceso = 3f;
    [SerializeField] private AudioSource sonidoMotor;
    [SerializeField] private Barril barril;

    private int canasEnMaquina = 0;
    private bool estaProcesando = false;

    void Update ()
    {
        if (!estaProcesando && canasEnMaquina >= cantidadProcesar && barril != null && !barril.EstaLleno)
        {
            StartCoroutine(ProcesarCanas());
        }
    }

    public int EspacioDisponible ()
    {
        return capacidadInterna - canasEnMaquina;
    }

    public void RecibirCana ()
    {
        if (canasEnMaquina < capacidadInterna)
        {
            canasEnMaquina++;
            UIManager.Instance.ActualizarCanaMaquina(canasEnMaquina, capacidadInterna);
        }
    }

    private System.Collections.IEnumerator ProcesarCanas ()
    {
        estaProcesando = true;

        if (sonidoMotor != null) sonidoMotor.Play();
        UIManager.Instance.MostrarContadorProcesamiento(true);

        float tiempoRestante = tiempoProceso;

        while (tiempoRestante > 0f)
        {
            UIManager.Instance.ActualizarContadorProcesamiento((int)tiempoRestante);
            yield return new WaitForSeconds(1f);
            tiempoRestante -= 1f;

            if (barril.EstaLleno)
            {
                UIManager.Instance.MostrarContadorProcesamiento(false);
                if (sonidoMotor != null) sonidoMotor.Stop();
                estaProcesando = false;
                yield break;
            }
        }

        int cantidadTransferida = Mathf.Min(canasEnMaquina, cantidadProcesar, barril.EspacioDisponible());
        canasEnMaquina -= cantidadTransferida;
        barril.RecibirCanas(cantidadTransferida);
        UIManager.Instance.ActualizarCanaMaquina(canasEnMaquina, capacidadInterna);

        UIManager.Instance.MostrarContadorProcesamiento(false);
        if (sonidoMotor != null) sonidoMotor.Stop();
        estaProcesando = false;
    }

}
