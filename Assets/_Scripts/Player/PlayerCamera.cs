using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class PlayerCamera : PlayerAction
{
    [Header("Camera")]
    [SerializeField] private CinemachineCamera cineCamera = null;
    [SerializeField] private float meshRotationSpeed = 45f;
    [SerializeField] private Vector2 noiseAmplitudeRange = new Vector2(0f, 2f);
    [SerializeField] private Vector2 noiseFrequencyRange = new Vector2(0f, 2f);
    [Range(0f, 1f)]
    [SerializeField] private float minNoise = 0.1f;
    [SerializeField] private float explosionNoiseMaxDuration = 0.5f;
    [SerializeField] private float explosionNoiseAmplitude = 1f;
    [SerializeField] private float explosionNoiseFrequency = 4f;

    private CinemachinePanTilt panTilt = null;
    private CinemachineInputAxisController axisController = null;
    private CinemachineBasicMultiChannelPerlin noise = null;

    private bool explosionNoiseOn = false;
    private float lastExplosionNoiseTime = 0f;

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
    public void LookAt(Vector3 pos)
    {
        panTilt.enabled = false;
        axisController.enabled = false;
        Vector3 direction = pos - cineCamera.transform.position;
        Quaternion rotation = Quaternion.LookRotation(direction.normalized);
        SetRotation(rotation);
    }
    public void LookAtSmooth(Vector3 pos, float smoothSpeed)
    {
        panTilt.enabled = false;
        axisController.enabled = false;
        Vector3 direction = pos - cineCamera.transform.position;
        Quaternion rotation = Quaternion.LookRotation(direction.normalized);
        SetRotation(Quaternion.Slerp(cineCamera.transform.rotation, rotation, smoothSpeed * Time.deltaTime));
    }
    public void StartExplosionNoise(float distanceToExplosion)
    {
        if (explosionNoiseOn)
        {
            lastExplosionNoiseTime = Time.time;
        }
        else
        {
            StartCoroutine(ExplosionNoise(distanceToExplosion));
        }
    }
    private IEnumerator ExplosionNoise(float distanceToExplosion)
    {
        explosionNoiseOn = true;

        // scale noise to distance to explosion

        lastExplosionNoiseTime = Time.time;
        noise.AmplitudeGain = explosionNoiseAmplitude;
        noise.FrequencyGain = explosionNoiseFrequency;
        while(!NovUtil.TimeCheck(lastExplosionNoiseTime, explosionNoiseMaxDuration))
        {
            yield return null;
        }

        explosionNoiseOn = false;
    }
    public void SetRotation(Quaternion rotation)
    {
        cineCamera.transform.rotation = rotation;

        SetPan(rotation.eulerAngles.y);
        SetTilt(rotation.eulerAngles.x);
    }
    public void SetNoise(float weight)
    {
        if (explosionNoiseOn) return;

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
