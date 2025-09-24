using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolWateringPot : MonoBehaviour
{
    [SerializeField] private ParticleSystem waterStream;
    [SerializeField] private AudioSource sfx;
    [SerializeField] private float autoDestroyAfter = 2.0f;

    void OnEnable() { Destroy(gameObject, autoDestroyAfter); }

    public void StartWater()
    {
        if (waterStream && !waterStream.isPlaying) waterStream.Play();
        if (sfx) sfx.Play();
    }
    public void EndWater()
    {
        if (waterStream && waterStream.isPlaying) waterStream.Stop();
    }
}
