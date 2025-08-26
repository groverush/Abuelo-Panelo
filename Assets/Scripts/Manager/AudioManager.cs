using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;


public enum SoundType
{
    PlayerWalk,
    PlayerRun,
    PlayerCall,
    PlayerCut,
    CaneCollect,
    CaneGive,
    BottleRecollected,
    BottleFilled,
    BottleBroken,
    BottleDelivered,
    DonkeyWalk,
    Machine,
    MusicMenu,
    MusicWorld,
    Victory,
    GameOver
}

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;
    private Dictionary<SoundType, AudioSource> soundSources;
    public static AudioManager Instance;

    [System.Serializable]
    public class Sound
    {
        public SoundType type;
        public AudioClip clip;
        public bool loop;
        [Range(0f, 1f)] public float volume = 1f;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            // Si no existe una instancia, esta se convierte en la única
            Instance = this;
            // ¡Este método es crucial! 
            // Evita que el GameObject se destruya al cargar una nueva escena.
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Si ya existe una instancia, destruye este objeto duplicado
            Destroy(gameObject);
        }

        soundSources = new Dictionary<SoundType, AudioSource>();

        foreach (var s in sounds)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.clip = s.clip;
            source.loop = s.loop;
            source.volume = s.volume;
            soundSources.Add(s.type, source);
        }
    }

    public void PlayOneShot(SoundType type)
    {
        if (soundSources.ContainsKey(type))
            soundSources[type].PlayOneShot(soundSources[type].clip);
    }

    // 🔁 Para sonidos que deben mantenerse en loop
    public void PlayLoop(SoundType type)
    {
        if (soundSources.ContainsKey(type) && !soundSources[type].isPlaying)
            soundSources[type].Play();
    }

    public void StopLoop(SoundType type)
    {
        if (soundSources.ContainsKey(type) && soundSources[type].isPlaying)
            soundSources[type].Stop();
    }
    
    public bool IsPlaying(SoundType type)
    {
        if (soundSources.ContainsKey(type))
        {
            return soundSources[type].isPlaying;
        }
        return false;
    }

}
