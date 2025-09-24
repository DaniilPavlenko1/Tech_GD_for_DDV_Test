using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class AnimalWander : MonoBehaviour
{
    [Header("Wander")]
    [SerializeField, Min(0.1f)] private float wanderRadius = 15f;
    [SerializeField, Min(0.05f)] private float arriveTolerance = 0.6f;
    [SerializeField, Min(0f)] private float minPause = 2f;
    [SerializeField, Min(0f)] private float maxPause = 4f;

    [Header("Follow")]
    [SerializeField, Min(0.1f)] private float followDuration = 10f;
    [SerializeField, Min(0f)] private float followStopDistance = 2f;
    [SerializeField, Min(0.05f)] private float followRepathInterval = 0.25f;

    [Header("Look-to-Interact")]
    [SerializeField, Min(0.5f)] private float lookInteractDistance = 4f;
    [SerializeField, Min(0.05f)] private float lookInteractRadius = 0.25f;
    [SerializeField] private LayerMask lookMask = Physics.DefaultRaycastLayers;
    [SerializeField] private Collider ownCollider;
    [SerializeField] private bool debugRay = false;

    [Header("VFX / Audio")]
    [SerializeField] private Transform heartsAnchor;
    [SerializeField] private GameObject heartsPrefab;
    [SerializeField, Min(0f)] private float heartsLifetime = 2f;
    [SerializeField] private AudioSource audioSource;

    [Header("Dependencies")]
    [SerializeField] private Transform player;

    NavMeshAgent agent;
    Vector3 home;
    Coroutine currentRoutine;

    enum State { Wander, Follow }
    State state = State.Wander;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        home = transform.position;
        if (!ownCollider) ownCollider = GetComponentInChildren<Collider>();
    }

    void Start()
    {
        agent.autoBraking = true;
        agent.stoppingDistance = 0f;
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO) player = playerGO.transform;
        StartWander();
    }

    void Update()
    {
        if (!player) return;

        if (state == State.Wander && Input.GetKeyDown(KeyCode.E) && IsPlayerLookingAtMe())
        {
            if (CarrotCounter.Instance)
            {
                var f = CarrotCounter.Instance;
                var field = typeof(CarrotCounter).GetField("_count",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                int carrots = (int)field.GetValue(f);
                if (carrots > 0)
                {
                    field.SetValue(f, carrots - 1);
                    f.SendMessage("UpdateUI", SendMessageOptions.DontRequireReceiver);
                    PlayHearts();
                    StartFollow();
                }
            }
        }
    }

    bool IsPlayerLookingAtMe()
    {
        var cam = Camera.main;
        if (!cam || !ownCollider) return false;

        Vector3 origin = cam.transform.position;
        Vector3 dir = cam.transform.forward;

        if (debugRay) Debug.DrawRay(origin, dir * lookInteractDistance, Color.cyan, 0.05f);

        if (Physics.SphereCast(origin, lookInteractRadius, dir, out var hit,
                               lookInteractDistance, lookMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider == ownCollider || hit.collider.transform.IsChildOf(transform))
                return true;
        }
        return false;
    }

    void StartWander()
    {
        state = State.Wander;
        RestartRoutine(WanderLoop());
    }

    IEnumerator WanderLoop()
    {
        while (state == State.Wander)
        {
            Vector3 target;
            while (!TryRandomPoint(out target)) yield return null;
            agent.isStopped = false;
            agent.stoppingDistance = 0f;
            agent.SetDestination(target);
            yield return new WaitUntil(() =>
                !agent.pathPending &&
                agent.remainingDistance <= arriveTolerance &&
                agent.velocity.sqrMagnitude < 0.01f
            );
            agent.isStopped = true;
            agent.ResetPath();
            yield return new WaitForSeconds(Random.Range(minPause, maxPause));
        }
    }

    void StartFollow()
    {
        state = State.Follow;
        RestartRoutine(FollowLoop());
    }

    IEnumerator FollowLoop()
    {
        float t = followDuration;
        agent.isStopped = false;
        agent.stoppingDistance = followStopDistance;
        while (state == State.Follow && t > 0f && player != null)
        {
            agent.SetDestination(player.position);
            float step = followRepathInterval;
            t -= step;
            yield return new WaitForSeconds(step);
        }
        agent.isStopped = true;
        agent.ResetPath();
        yield return new WaitForSeconds(Random.Range(minPause, maxPause));
        StartWander();
    }

    bool TryRandomPoint(out Vector3 result)
    {
        for (int i = 0; i < 10; i++)
        {
            var rnd = home + Random.insideUnitSphere * wanderRadius;
            if (NavMesh.SamplePosition(rnd, out var hit, 3f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = transform.position;
        return false;
    }

    void RestartRoutine(IEnumerator routine)
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(routine);
    }

    void PlayHearts()
    {
        if (!heartsPrefab) return;
        var anchor = heartsAnchor ? heartsAnchor : transform;
        var go = Instantiate(heartsPrefab, anchor.position, anchor.rotation, anchor);
        if (heartsLifetime > 0f) Destroy(go, heartsLifetime);
        audioSource.Play();
    }
}
