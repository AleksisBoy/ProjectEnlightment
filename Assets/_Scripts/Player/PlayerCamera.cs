using System.Data;
using UnityEngine;
using Unity.Cinemachine;

public class PlayerCamera : PlayerAction
{
    [Header("Camera")]
    [SerializeField] private CinemachineCamera cineCamera = null;
    [SerializeField] private CinemachinePanTilt panTilt = null;
    [SerializeField] private CinemachineInputAxisController axisController = null;
    [SerializeField] private Vector3 cameraOffset = Vector3.zero;
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float rotationLimit = 90f;

    public CinemachineCamera Get => cineCamera;

    private Quaternion currentRotation;
    public Quaternion CurrentRotation => currentRotation;

    private Quaternion currentRotationFlat;
    public Quaternion CurrentRotationFlat => currentRotationFlat;
    public Vector3 Position => cineCamera.transform.position;
    public Quaternion Rotation 
    { 
        get
        {
            return cineCamera.transform.rotation;
        }
        set
        {
            cineCamera.transform.rotation = value;
        }
    }
    public Vector3 Forward => cineCamera.transform.forward;    

    private void Awake()
    {
        if (cineCamera == null) throw new System.Exception("NO CAMERA");

        currentRotation = cineCamera.transform.rotation;
        currentRotationFlat = Quaternion.Euler(0f, currentRotation.y, currentRotation.z);
        //cineCamera.transform.position = transform.position + cameraOffset;
    }

    public override void ActionUpdate(out bool blockOther)
    {
        blockOther = false;

        panTilt.enabled = true;
        axisController.enabled = true;
        master.Mesh.transform.rotation = cineCamera.transform.rotation;
        //CalculateCurrentRotation(GetMouseInputVector());
    }
    public override bool ActionBlocked(CharacterAction blocker)
    {
        panTilt.enabled = false;
        axisController.enabled = false;
        cineCamera.transform.rotation = master.Mesh.transform.rotation;
        return true;
    }
    private void LateUpdate()
    {
        if (!Update) return;

        //Quaternion rot = cineCamera.transform.rotation;
        Quaternion rot = Quaternion.Euler(new Vector3(0f, cineCamera.transform.eulerAngles.y, cineCamera.transform.eulerAngles.z));
        //master.RB.MoveRotation(Quaternion.Slerp(master.RB.rotation, rot, rotationSpeed * Time.deltaTime));
    }
    private void CalculateCurrentRotation(Vector2 input)
    {
        Vector3 rotationOffset = new Vector3(-input.y * rotationSpeed * Time.deltaTime,
                    input.x * rotationSpeed * Time.deltaTime, 0f);

        Vector3 euler = cineCamera.transform.eulerAngles + rotationOffset;

        if (euler.x > rotationLimit && euler.x < 180f)
        {
            euler.x = rotationLimit;
        }
        else if (euler.x < 360f - rotationLimit && euler.x >= 180f)
        {
            euler.x = 360f - rotationLimit;
        }

        cineCamera.transform.eulerAngles = euler;
        currentRotation = cineCamera.transform.rotation;
        //currentRotation = Quaternion.Euler(euler);
        currentRotationFlat = Quaternion.Euler(new Vector3(0f, euler.y, euler.z));
    }
    private static Vector2 GetMouseInputVector()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        Vector2 inputVector = new Vector2(mouseX, mouseY);

        return inputVector;
    }
    public void SetPan(float pan)
    {
        panTilt.PanAxis.Value = pan;
    }
    public void SetTilt(float tilt)
    {
        panTilt.TiltAxis.Value = tilt;
    }
}
