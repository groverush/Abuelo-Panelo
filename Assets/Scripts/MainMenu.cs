using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void Start()
    {
        // 🔊 Lógica de audio
        // Solo reproduce la música si el AudioManager.Instance existe y la música del menú no está ya sonando
        if (AudioManager.Instance != null && !AudioManager.Instance.IsPlaying(SoundType.MusicMenu))
        {
            AudioManager.Instance.PlayOneShot(SoundType.MusicMenu);
        }
    }

    public void PlayGame()
    {
        // CONGELA LA DECISIÓN DEL INPUT ANTES DE CARGAR LA ESCENA
        if (InputDeviceDetector.Instance != null)
        {
            InputDeviceDetector.Instance.StopDetection();
        }
        // Detén la música del menú antes de cargar la nueva escena
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopLoop(SoundType.MusicMenu);
            // Si la música del menú es un one-shot (sin loop), podrías simplemente no hacer nada aquí y dejar que termine
            // Pero si la iniciaste como un loop, debes detenerla explícitamente.
        }
        SceneManager.LoadSceneAsync(4);
    }

    public void InstructionsScene()
    {
        SceneManager.LoadSceneAsync(2);
    }

    public void CreditsScene()
    {
        SceneManager.LoadSceneAsync(3);
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadSceneAsync(0);
    }
}