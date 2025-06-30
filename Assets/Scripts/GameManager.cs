using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake ()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Opcional: si quieres que persista entre escenas
    }

    public void GanarJuego ()
    {
        Debug.Log("🎉 Has ganado el juego.");
        UIManager.Instance.MostrarVictoria("¡Has entregado todas las botellas de jarabe!");
    }
}
