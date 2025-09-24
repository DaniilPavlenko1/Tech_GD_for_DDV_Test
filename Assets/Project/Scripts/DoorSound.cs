using UnityEngine;

public class DoorSound : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openClip;
    [SerializeField] private AudioClip closeClip;

    public void PlayOpenSound()
    {
        audioSource.PlayOneShot(openClip);
    }

    public void PlayCloseSound()
    {
        audioSource.PlayOneShot(closeClip);
    }

    public void StopSound() { if (audioSource) audioSource.Stop(); }
}
