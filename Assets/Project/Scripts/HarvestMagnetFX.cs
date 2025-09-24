using UnityEngine;
using System.Collections;

public class HarvestMagnetFX : MonoBehaviour
{
    [Header("Hop")]
    [SerializeField] private float hopHeight = 1.0f;
    [SerializeField] private float hopTime = 0.35f;

    [Header("Fly")]
    [SerializeField] private float flyTime = 0.5f;
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("FX")]
    [SerializeField] private ParticleSystem poof;
    [SerializeField] private AudioSource sfxCatch;

    Transform _target;

    public void Play(Transform target)
    {
        _target = target;
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        Vector3 start = transform.position;
        float t = 0f;
        while (t < hopTime)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / hopTime);
            float y = 4f * hopHeight * u * (1f - u);
            transform.position = start + Vector3.up * y;
            yield return null;
        }

        Vector3 flyStart = transform.position;
        t = 0f;
        while (t < flyTime && _target)
        {
            t += Time.deltaTime;
            float u = ease.Evaluate(Mathf.Clamp01(t / flyTime));
            Vector3 dst = _target.position;
            Vector3 pos = Vector3.Lerp(flyStart, dst, u);
            pos.y += Mathf.Sin(u * Mathf.PI) * 0.2f;
            transform.position = pos;
            transform.rotation = Quaternion.Slerp(transform.rotation,
                                                  Quaternion.LookRotation(dst - pos, Vector3.up),
                                                  Time.deltaTime * 8f);
            yield return null;
        }

        if (poof)
        {
            poof.transform.SetParent(null, true);
            poof.transform.position = transform.position;

            var main = poof.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            poof.Play();

            float ttl = main.duration + main.startLifetime.constantMax + 0.1f;
            Destroy(poof.gameObject, ttl);
        }

        if (sfxCatch && sfxCatch.clip)
            AudioSource.PlayClipAtPoint(sfxCatch.clip, transform.position, sfxCatch.volume);

        Destroy(gameObject);
    }
}
