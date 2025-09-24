using UnityEngine;

public class TPSCharacterController : MonoBehaviour
{
    public enum ControlMode { Tank, Direct }
    [Header("Mode")]
    public ControlMode m_controlMode = ControlMode.Direct;

    [Header("Refs")]
    [SerializeField] Transform m_cameraRoot;  // Pivot, тот же, что Follow/LookAt у виртуальной камеры
    [SerializeField] Animator m_animator;

    [Header("Movement")]
    [SerializeField] float m_moveSpeed = 4.5f;
    [SerializeField] float m_turnSpeed = 360f;
    [SerializeField] float m_interpolation = 8f;

    [Header("Scales")]
    [SerializeField] float m_walkScale = 0.5f;
    [SerializeField] float m_backwardsWalkScale = 0.5f;
    [SerializeField] float m_backwardRunScale = 0.8f;

    [Header("Jumping")]
    [SerializeField] float m_jumpForce = 5.5f;
    [SerializeField] float m_minJumpInterval = 0.25f;
    [SerializeField] LayerMask m_groundMask = ~0;
    [SerializeField] float m_groundCheckDistance = 0.25f;

    Rigidbody m_rigidBody;

    // input & smoothing
    float m_inV, m_inH;
    float m_currentV, m_currentH;
    Vector3 m_currentDirection = Vector3.forward;

    // ground/jump state
    bool m_isGrounded = true;
    bool m_wasGrounded = true;
    bool m_jumpInput = false;
    float m_jumpTimeStamp = 0f;

    void Awake()
    {
        if (!m_rigidBody) m_rigidBody = GetComponent<Rigidbody>();
        if (!m_animator) m_animator = GetComponentInChildren<Animator>();
        if (!m_cameraRoot && Camera.main) m_cameraRoot = Camera.main.transform;

        if (m_rigidBody)
        {
            m_rigidBody.interpolation = RigidbodyInterpolation.Interpolate;
            m_rigidBody.constraints &= ~RigidbodyConstraints.FreezeRotationY; // на всякий
        }
    }

    void Update()
    {
        // Ground check (простой рейкаст от центра вниз)
        //GroundCheck();

        // Read input
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");
        bool walk = Input.GetKey(KeyCode.LeftShift);

        if (v < 0f)
            v *= walk ? m_backwardsWalkScale : m_backwardRunScale;
        else if (walk)
            v *= m_walkScale;

        h = h * (walk ? m_walkScale : 1f);

        m_inV = v;
        m_inH = h;

        // smoothing (на целевые значения)
        m_currentV = Mathf.Lerp(m_currentV, m_inV, Time.deltaTime * m_interpolation);
        m_currentH = Mathf.Lerp(m_currentH, m_inH, Time.deltaTime * m_interpolation);

        if (Input.GetKeyDown(KeyCode.Space))
            m_jumpInput = true;
    }

    void FixedUpdate()
    {
        switch (m_controlMode)
        {
            case ControlMode.Tank:
                TankFixed();
                break;
            case ControlMode.Direct:
                DirectFixed();
                break;
            default:
                Debug.LogError("Unsupported state");
                break;
        }

        JumpingAndLanding();

        m_wasGrounded = m_isGrounded;
        m_jumpInput = false;
    }

    void TankFixed()
    {
        Vector3 forward = transform.forward;
        Vector3 delta = forward * (m_currentV * m_moveSpeed * Time.fixedDeltaTime);
        m_rigidBody.MovePosition(m_rigidBody.position + delta);

        if (Mathf.Abs(m_currentH) > 0.0001f)
        {
            float yawDelta = m_currentH * m_turnSpeed * Time.fixedDeltaTime;
            Quaternion rot = Quaternion.AngleAxis(yawDelta, Vector3.up) * m_rigidBody.rotation;
            m_rigidBody.MoveRotation(rot);
        }

        if (m_animator) m_animator.SetFloat("MoveSpeed", m_currentV);
    }

    void DirectFixed()
    {
        Vector3 camF = m_cameraRoot ? m_cameraRoot.forward : Vector3.forward;
        Vector3 camR = m_cameraRoot ? m_cameraRoot.right : Vector3.right;
        camF.y = 0f; camR.y = 0f;
        camF.Normalize(); camR.Normalize();

        Vector3 desired = camF * m_currentV + camR * m_currentH;
        float len = desired.magnitude;
        if (len > 1f) desired /= len;
        if (desired.sqrMagnitude > 0.0001f)
        {
            m_currentDirection = Vector3.Slerp(m_currentDirection, desired, Time.fixedDeltaTime * m_interpolation);

            Quaternion targetRot = Quaternion.LookRotation(m_currentDirection, Vector3.up);
            m_rigidBody.MoveRotation(Quaternion.RotateTowards(m_rigidBody.rotation, targetRot, m_turnSpeed * Time.fixedDeltaTime));

            Vector3 delta = m_currentDirection * (m_moveSpeed * Time.fixedDeltaTime);
            m_rigidBody.MovePosition(m_rigidBody.position + delta);

            if (m_animator) m_animator.SetFloat("MoveSpeed", len);
        }
        else
        {
            if (m_animator) m_animator.SetFloat("MoveSpeed", 0f);
        }
    }

    void JumpingAndLanding()
    {
        bool jumpCooldownOver = (Time.time - m_jumpTimeStamp) >= m_minJumpInterval;

        if (jumpCooldownOver && m_isGrounded && m_jumpInput)
        {
            m_jumpTimeStamp = Time.time;
            m_rigidBody.AddForce(Vector3.up * m_jumpForce, ForceMode.Impulse);
            m_isGrounded = false;
        }
    }

    //void GroundCheck()
    //{
    //    Vector3 origin = transform.position + Vector3.up * 0.1f;
    //    m_isGrounded = Physics.Raycast(origin, Vector3.down, m_groundCheckDistance + 0.1f, m_groundMask);
    //}

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = m_isGrounded ? Color.green : Color.red;
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        Gizmos.DrawLine(origin, origin + Vector3.down * (m_groundCheckDistance + 0.1f));
    }
#endif
}