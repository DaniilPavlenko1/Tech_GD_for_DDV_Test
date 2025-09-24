using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AnimalAnimSync : MonoBehaviour
{
    [SerializeField] private Transform visualRoot;
    [SerializeField] private float walkSpeed = 1.5f;
    [SerializeField] private float idleDeadzone = 0.05f;
    [SerializeField] private float rotateLerp = 10f;
    [SerializeField] private string speedParam = "Speed";

    NavMeshAgent agent;
    [SerializeField] private Animator anim;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (visualRoot == null)
        {
            anim = GetComponentInChildren<Animator>();
            if (anim) visualRoot = anim.transform;
        }
        else
        {
            anim = visualRoot.GetComponentInChildren<Animator>();
        }

        agent.updatePosition = true;
        agent.updateRotation = false;

        if (anim) anim.applyRootMotion = false;
    }

    void Update()
    {
        if (!anim || !agent) return;

        float v = agent.velocity.magnitude;

        bool toIdle = agent.isStopped || v < idleDeadzone;

        float target = toIdle ? 0f : Mathf.Clamp01(v / Mathf.Max(0.01f, walkSpeed));

        float current = anim.GetFloat(speedParam);
        float damp = (target < current) ? 0.03f : 0.15f;

        anim.SetFloat(speedParam, target, damp, Time.deltaTime);

        if (visualRoot)
        {
            Vector3 dir = agent.desiredVelocity; dir.y = 0;
            if (!toIdle && dir.sqrMagnitude > 0.0001f)
            {
                var q = Quaternion.LookRotation(dir);
                visualRoot.rotation = Quaternion.Slerp(visualRoot.rotation, q, rotateLerp * Time.deltaTime);
            }
        }
    }
}
