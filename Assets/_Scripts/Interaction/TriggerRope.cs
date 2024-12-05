using UnityEngine;
using UnityEngine.Events;

public class TriggerRope : PickupObject
{
    [Header("Rope")]
    [SerializeField] private Transform ropeMesh = null;
    [SerializeField] private BoxCollider coll = null;
    [SerializeField] private float length = 1f;
    [SerializeField] private UnityEvent activateEvent = null;
    [SerializeField] private UnityEvent deactivateEvent = null;

    private void OnValidate()
    {
        if (ropeMesh) ropeMesh.localScale = new Vector3(ropeMesh.localScale.x, length, ropeMesh.localScale.z);
        if (coll) coll.size = new Vector3(length * 2f, ropeMesh.localScale.x, ropeMesh.localScale.x);
    }
    private void Start()
    {
        coll.isTrigger = true;
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name + " activated " + name);
        Activate();
    }
    private void Activate()
    {
        if (activateEvent != null) activateEvent.Invoke();
        // do breaking rope animation and sound then destroy
        Destroy(gameObject);
        enabled = false;
    }
    private void Deactivate()
    {
        if (deactivateEvent != null) deactivateEvent.Invoke();
        Destroy(gameObject);
    }
    public override void Pickup(Player player)
    {
        Deactivate();
    }
}
