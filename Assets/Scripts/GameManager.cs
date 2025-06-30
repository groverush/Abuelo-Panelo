using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Sonidos")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip victoriaAudioClip;
    [SerializeField] private AudioClip derrotaAudioClip;

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
        UIManager.Instance.MostrarVictoria("¡VICTORIA!");
        audioSource.PlayOneShot(victoriaAudioClip); // Reproducir sonido de victoria
        Time.timeScale = 0; // Detener el tiempo del juego
    }

    public void PerderJuego ()
    {
        Debug.Log("🎉 Has ganado el juego.");
        UIManager.Instance.MostrarDerrota("¡DERROTA!");
        audioSource.PlayOneShot(derrotaAudioClip); // Reproducir sonido de victoria
        Time.timeScale = 0; // Detener el tiempo del juego
    }

    public void ReiniciarJuego ()
    {
        Debug.Log("Reiniciando juego...");
        // Aquí puedes agregar lógica para reiniciar el juego, como recargar la escena actual
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Time.timeScale = 1; // Asegurarse de que el tiempo del juego esté activo
    }

    public void PausarJuego ()
    {
        Debug.Log("Saliendo del juego...");
        UIManager.Instance.MostrarPausa( true, "Juego pausado");
        Time.timeScale = 0; // Detener el tiempo del juego
    }
    public void Continuar ()
    {
        Debug.Log("Saliendo del juego...");
        UIManager.Instance.MostrarPausa( false, " ");
        Time.timeScale = 1; // Detener el tiempo del juego
    }
}
