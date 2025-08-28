using UnityEngine;

public class Machete : MonoBehaviour
{
    private void OnTriggerEnter ( Collider other )
    {
        if (other.CompareTag("Sugarcane") && PlayerController.EstaCortando)
        {
            AudioManager.Instance.PlayOneShot(SoundType.PlayerCut);
        }
    }
}
