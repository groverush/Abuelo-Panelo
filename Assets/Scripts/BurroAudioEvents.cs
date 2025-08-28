using UnityEngine;

public class BurroAudioEvents : MonoBehaviour
{
    public void PlayDonkeyWalkSound()
    {
        if (AudioManager.Instance != null && !AudioManager.Instance.IsPlaying(SoundType.DonkeyWalk))
        {
            AudioManager.Instance.PlayLoop(SoundType.DonkeyWalk);
        }
    }

    public void StopDonkeyWalkSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopLoop(SoundType.DonkeyWalk);
        }
    }
}