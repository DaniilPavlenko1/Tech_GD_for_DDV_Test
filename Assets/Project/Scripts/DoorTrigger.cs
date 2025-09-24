using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    [SerializeField] private Animator doorAnimator;
    [Header("FX")]
    [SerializeField] private GameObject FX;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            doorAnimator.SetTrigger("OpenDoor");
            var ps = FX.GetComponent<ParticleSystem>();
            ps.Play();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            doorAnimator.SetTrigger("CloseDoor");
            var ps = FX.GetComponent<ParticleSystem>();
            ps.Stop();
        }
    }

}