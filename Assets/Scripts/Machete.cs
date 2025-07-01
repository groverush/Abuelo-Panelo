using UnityEngine;

public class Machete : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip corteAudioClip;

    private void OnTriggerEnter ( Collider other )
    {
        if (other.CompareTag("Sugarcane") && PlayerController.EstaCortando)
        {
            if (audioSource != null && corteAudioClip != null)
            {
                audioSource.PlayOneShot(corteAudioClip, 1f);
            }
        }
    }
}
