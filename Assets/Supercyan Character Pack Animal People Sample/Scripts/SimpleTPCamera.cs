using UnityEngine;

public class SimpleTPCamera : MonoBehaviour
{
    public Transform cameraRoot;
    public float xSens = 220f;
    public float ySens = 140f;
    public float minPitch = -40f;
    public float maxPitch = 70f;

    float yaw;
    float pitch;

    void Start()
    {
        var e = cameraRoot.eulerAngles;
        yaw = e.y;
        pitch = e.x;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void LateUpdate()
    {
        float mx = Input.GetAxisRaw("Mouse X");
        float my = Input.GetAxisRaw("Mouse Y");

        yaw += mx * xSens * Time.deltaTime;
        pitch -= my * ySens * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        cameraRoot.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}