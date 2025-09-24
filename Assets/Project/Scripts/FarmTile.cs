using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum PlotState { Empty, Planted, Growing, Ready }

public class FarmTile : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private MeshRenderer tileRenderer;
    [SerializeField] private Material normalMat;
    [SerializeField] private Material highlightMat;

    [Header("Crop")]
    [SerializeField] private Transform cropAnchor;
    [SerializeField] private GameObject cropPrefab_Stage0;
    [SerializeField] private GameObject cropPrefab_Ready;
    GameObject _crop;

    [Header("UI (World Space)")]
    [SerializeField] private Canvas worldCanvas;
    [SerializeField] private Slider progress;

    [Header("Gameplay")]
    [SerializeField] private float growSeconds = 5f;

    [Header("Watering FX")]
    [SerializeField] private GameObject wateringCanFXPrefab;
    [SerializeField] private Transform wateringSpawn;
    [SerializeField] private float wateringAnimTime = 1.2f;

    [Header("Harvest FX")]
    [SerializeField] private GameObject carrotPickupPrefab;

    [Header("Stage FX")]
    [SerializeField] private GameObject[] stageFX;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] audioClips;

    public PlotState State { get; private set; } = PlotState.Empty;

    bool _highlighted;
    Coroutine _growCR;

    bool _isBusyWatering;
    GameObject _activeWaterFX;

    void Awake()
    {
        if (worldCanvas) worldCanvas.gameObject.SetActive(false);
        if (progress) { progress.minValue = 0f; progress.maxValue = 1f; progress.value = 0f; }
        tileRenderer.material = normalMat;
    }

    public void SetHighlight(bool on)
    {
        if (_highlighted == on) return;
        _highlighted = on;
        tileRenderer.material = on ? highlightMat : normalMat;
    }

    public bool CanInteract()
    {
        return State != PlotState.Growing && !_isBusyWatering;
    }

    public void Interact()
    {
        if (!CanInteract()) return;

        if (State == PlotState.Empty)
        {
            Plant();
        }
        else if (State == PlotState.Planted)
        {
            Water();
        }
        else if (State == PlotState.Ready)
        {
            Harvest();
        }
    }

    void Plant()
    {
        if (State != PlotState.Empty) return;
        ClearCrop();
        _crop = Instantiate(cropPrefab_Stage0, cropAnchor.position, cropAnchor.rotation, cropAnchor);
        PlayStageFX(0);
        audioSource.PlayOneShot(audioClips[0]);
        State = PlotState.Planted;
    }

    void Water(Transform source = null)
    {
        if (State != PlotState.Planted) return;
        if (_isBusyWatering) return;

        _isBusyWatering = true;

        if (_activeWaterFX == null)
            _activeWaterFX = PlayWateringFX(source);

        if (_growCR != null) StopCoroutine(_growCR);
        _growCR = StartCoroutine(GrowRoutineDelayed(wateringAnimTime));
    }

    IEnumerator GrowRoutine()
    {
        State = PlotState.Growing;
        if (worldCanvas) worldCanvas.gameObject.SetActive(true);
        float t = 0f;
        while (t < growSeconds)
        {
            t += Time.deltaTime;
            if (progress) progress.value = Mathf.Clamp01(t / growSeconds);
            yield return null;
        }
        if (progress) progress.value = 1f;
        State = PlotState.Ready;
        if (worldCanvas) worldCanvas.gameObject.SetActive(false);

        ClearCrop();
        _crop = Instantiate(cropPrefab_Ready, cropAnchor.position, cropAnchor.rotation, cropAnchor);
        _growCR = null;
        PlayStageFX(1);
        audioSource.PlayOneShot(audioClips[1]);
        _isBusyWatering = false;
        _activeWaterFX = null;
    }

    IEnumerator GrowRoutineDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        yield return StartCoroutine(GrowRoutine());
    }

    GameObject PlayWateringFX(Transform source)
    {
        if (!wateringCanFXPrefab) return null;
        if (!wateringSpawn)
        {
            Debug.LogWarning("wateringSpawn не назначен на тайле " + name);
            return null;
        }

        var go = Instantiate(
            wateringCanFXPrefab,
            wateringSpawn.position,
            wateringSpawn.rotation,
            wateringSpawn
        );

        return go;
    }

    void PlayStageFX(int stageIndex)
    {
        if (stageFX == null || stageIndex < 0 || stageIndex >= stageFX.Length) return;
        if (!stageFX[stageIndex]) return;

        SpawnFX(stageFX[stageIndex], cropAnchor.position + Vector3.up * 0.05f, Quaternion.Euler(-90, 0, 0));
    }

    void SpawnFX(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        var go = Instantiate(prefab, pos, rot);
        var ps = go.GetComponent<ParticleSystem>();
        float ttl = 1.5f;

        if (ps)
        {
            var main = ps.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            ps.Play();
            ttl = main.duration + main.startLifetime.constantMax + 0.2f;
        }
        Destroy(go, ttl);
    }

    public void Harvest(Transform collector = null)
    {
        if (State != PlotState.Ready) return;

        if (carrotPickupPrefab)
        {
            PlayStageFX(2);
            var spawnPos = cropAnchor.position + Vector3.up * 0.1f;
            var go = Instantiate(carrotPickupPrefab, spawnPos, Quaternion.identity);
            audioSource.PlayOneShot(audioClips[2]);
            var fx = go.GetComponent<HarvestMagnetFX>();
            if (fx && collector) fx.Play(collector);
        }

        Destroy(_crop);
        State = PlotState.Empty;
        if (progress) progress.value = 0f;
        CarrotCounter.Instance?.AddCarrot(1);
    }

    void ClearCrop()
    {
        if (_crop) Destroy(_crop);
    }
}
