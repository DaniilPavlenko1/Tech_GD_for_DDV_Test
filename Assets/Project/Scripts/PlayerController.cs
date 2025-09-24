using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundMask;

    [Header("Move")]
    [SerializeField] private float walkSpeed = 3.5f;
    [SerializeField] private float runSpeed = 6.0f;
    [SerializeField] private float rotationSpeed = 12f;
    [SerializeField] private float airControl = 0.6f;

    [Header("Jump/Gravity")]
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float extraFall = 2.0f;
    [SerializeField] private float groundRadius = 0.2f;

    [Header("Auto Lock While Animator Plays")]
    [SerializeField] private int lockLayerIndex = 0;
    [SerializeField] private string[] lockStateNames = { "Wave" };
    bool lockMovement;

    Rigidbody rb;
    Transform cam;

    Vector2 input;
    bool pressJump;

    bool grounded, wasGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        cam = Camera.main ? Camera.main.transform : null;

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void Update()
    {
        if (animator && lockStateNames != null && lockStateNames.Length > 0)
        {
            var st = animator.GetCurrentAnimatorStateInfo(lockLayerIndex);
            bool inLocked = false;
            for (int i = 0; i < lockStateNames.Length; i++)
            {
                var name = lockStateNames[i];
                if (!string.IsNullOrEmpty(name) && st.IsName(name))
                {
                    inLocked = !st.loop || st.normalizedTime < 1f;
                    break;
                }
            }
            lockMovement = inLocked;
        }

        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");
        input = input.sqrMagnitude > 1 ? input.normalized : input;

        if (Input.GetKeyDown(KeyCode.Space)) pressJump = true;
        if (Input.GetKeyDown(KeyCode.Q)) animator.SetTrigger("Wave");
    }

    void FixedUpdate()
    {
        if (lockMovement)
        {
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            animator.SetFloat("MoveSpeed", 0f);
            return;
        }
        wasGrounded = grounded;

        const float probeUp = 0.08f;
        const float probeDown = 0.12f;
        Vector3 start = groundCheck.position + Vector3.up * probeUp;

        grounded = Physics.SphereCast(
            start,
            groundRadius,
            Vector3.down,
            out _,
            probeDown,
            groundMask,
            QueryTriggerInteraction.Ignore
        );

        Vector3 f = cam ? Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized : Vector3.forward;
        Vector3 r = cam ? cam.right : Vector3.right;
        Vector3 moveDir = (f * input.y + r * input.x);
        if (moveDir.sqrMagnitude > 1) moveDir.Normalize();

        bool sprint = Input.GetKey(KeyCode.LeftShift);
        float targetSpeed = (sprint ? runSpeed : walkSpeed) * (grounded ? 1f : airControl);

        Vector3 vel = rb.velocity;
        vel.x = moveDir.x * targetSpeed;
        vel.z = moveDir.z * targetSpeed;

        // Jump
        if (grounded && pressJump)
        {
            pressJump = false;
            vel.y = Mathf.Sqrt(2f * Physics.gravity.magnitude * jumpHeight);
            animator?.SetTrigger("Jump");
        }
        else if (vel.y < 0f)
        {
            vel += Vector3.up * Physics.gravity.y * (extraFall - 1f) * Time.fixedDeltaTime;
        }

        rb.velocity = vel;

        // Turn towards movement
        Vector3 look = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (look.sqrMagnitude > 0.001f)
        {
            Quaternion to = Quaternion.LookRotation(look.normalized, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, to, rotationSpeed * Time.fixedDeltaTime));
        }

        bool justLanded = (!wasGrounded && grounded);

        // --- Animator ---
        if (animator)
        {
            animator.SetBool("Grounded", grounded);
            animator.SetBool("Midair", !grounded);
            animator.ResetTrigger("Land");

            if (justLanded)
            {
                animator.ResetTrigger("Jump");
                animator.SetTrigger("Land");

                var st = animator.GetCurrentAnimatorStateInfo(0);
                if (!st.IsName("Land"))
                    animator.CrossFade("Land", 0.02f, 0, 0f);
            }

            Vector3 planar = rb.velocity; planar.y = 0f;
            animator.SetFloat("MoveSpeed", planar.magnitude);
        }

        if (pressJump && !grounded) pressJump = false;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }
    }
}