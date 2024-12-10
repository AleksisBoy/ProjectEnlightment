using UnityEngine;
using Unity.Cinemachine;

public class PlayerCamera : PlayerAction
{
    [Header("Camera")]
    [SerializeField] private CinemachineCamera cineCamera = null;
    [SerializeField] private float meshRotationSpeed = 45f;
    [SerializeField] private Vector2 noiseAmplitudeRange = new Vector2(0f, 2f);
    [SerializeField] private Vector2 noiseFrequencyRange = new Vector2(0f, 2f);
    [Range(0f, 1f)]
    [SerializeField] private float minNoise = 0.1f;

    private CinemachinePanTilt panTilt = null;
    private CinemachineInputAxisController axisController = null;
    private CinemachineBasicMultiChannelPerlin noise = null;

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

        panTilt = cineCamera.GetComponent<CinemachinePanTilt>();
        axisController = cineCamera.GetComponent<CinemachineInputAxisController>();
        noise = cineCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();

        SetNoise(0.1f);
    }

    public override void ActionUpdate(out bool blockOther)
    {
        blockOther = false;

        panTilt.enabled = true;
        axisController.enabled = true;
    }
    private void LateUpdate()
    {
        master.Mesh.transform.rotation = Quaternion.Slerp(master.Mesh.transform.rotation,
            cineCamera.transform.rotation, Time.deltaTime * meshRotationSpeed);
    }
    public override bool ActionBlocked(CharacterAction blocker)
    {
        panTilt.enabled = false;
        axisController.enabled = false;
        cineCamera.transform.rotation = master.Mesh.transform.rotation;
        return true;
    }
    public void SetNoise(float weight)
    {
        weight = Mathf.Max(minNoise, Mathf.Clamp01(weight));
        noise.AmplitudeGain = Mathf.Lerp(noiseAmplitudeRange.x, noiseAmplitudeRange.y, weight);
        noise.FrequencyGain = Mathf.Lerp(noiseFrequencyRange.x, noiseFrequencyRange.y, weight);
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
