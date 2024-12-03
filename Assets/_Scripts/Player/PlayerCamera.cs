using System.Data;
using UnityEngine;

public class PlayerCamera : PlayerAction
{
    [Header("Camera")]
    [SerializeField] private Camera _camera = null;
    [SerializeField] private Vector3 cameraOffset = Vector3.zero;
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float rotationLimit = 90f;

    public Camera Get => _camera;

    private Quaternion currentRotation;
    public Quaternion CurrentRotation => currentRotation;

    private Quaternion currentRotationFlat;
    public Quaternion CurrentRotationFlat => currentRotationFlat;

    private void Awake()
    {
        if (_camera == null) throw new System.Exception("NO CAMERA");

        currentRotation = _camera.transform.rotation;
        _camera.transform.position = transform.position + cameraOffset;
    }
    private void LateUpdate()
    {
        if (!Update) return;

        RotateCamera();
    }

    public override void ActionUpdate(out bool blockOther)
    {
        blockOther = false;

        CalculateCurrentRotation(GetMouseInputVector());
    }

    private void RotateCamera()
    {
        _camera.transform.rotation = currentRotation;
        _camera.transform.position = transform.position + cameraOffset;
    }
    private void CalculateCurrentRotation(Vector2 input)
    {
        Vector3 rotationOffset = new Vector3(-input.y * rotationSpeed * Time.deltaTime,
                    input.x * rotationSpeed * Time.deltaTime, 0f);
        
        Vector3 euler = _camera.transform.localEulerAngles + rotationOffset;

        if (euler.x > rotationLimit && euler.x < 180f)
        {
            euler.x = rotationLimit;
        }
        else if (euler.x < 360f - rotationLimit && euler.x >= 180f)
        {
            euler.x = 360f - rotationLimit;
        }

        currentRotation = Quaternion.Euler(euler);
        currentRotationFlat = Quaternion.Euler(new Vector3(0f, euler.y, euler.z));
    }
    private static Vector2 GetMouseInputVector()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        Vector2 inputVector = new Vector2(mouseX, mouseY);

        return inputVector;
    }
}
