using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IAnimationDispatch, IHealth
{
    [SerializeField] private Animator animator = null;
    [SerializeField] private Collider playerCollider = null;
    [SerializeField] private GameObject playerMesh = null;
    [Header("Health")]
    [SerializeField] private int maxHP = 4;
    [SerializeField] private float reviveTime = 2f;

    private int bits = 0;

    private int hp = 0;
    private bool dead = false;
    private float deathTime = 0;

    private IHealth.OnHealthChanged onHealthChanged;

    private Rigidbody rb = null;
    private PlayerCamera playerCamera = null;

    private List<CharacterAction> actionList = new List<CharacterAction>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        foreach(CharacterAction action in GetComponents<CharacterAction>())
        {
            actionList.Add(action);
            action.ActionSetup(this);
            if(!playerCamera && action as PlayerCamera)
            {
                playerCamera = action as PlayerCamera;
            }
        }
        CharacterAction.SortActions<CharacterAction>(actionList, out actionList);
    }
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Revive();
    }
    private void Update()
    {
        if (dead)
        {
            ReviveProcess();
            return;
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            GetHit(1, null);
        }

        bool block = false;
        CharacterAction blocker = null;
        foreach(CharacterAction action in actionList)
        {
            if (block)
            {
                // Get response on blocking this action
                block = action.ActionBlocked(blocker);
                if (block) continue;

                // Disturb blocker if unblocked
                blocker.ActionDisturbed(action);
            }

            // Update next action if enabled Update
            if (action.Update) action.ActionUpdate(out block);

            // Assign blocker if this action blocks others
            if (block) blocker = action;
        }
    }
    private void AddAction(CharacterAction action)
    {
        actionList.Add(action);
        CharacterAction.SortActions<CharacterAction>(actionList, out actionList);
    }
    public void AddBits(int amount)
    {
        bits += amount;
    }
    public float GetDotProduct(Vector3 otherPosition)
    {
        Vector3 direction = otherPosition - transform.position;
        return Vector3.Dot(transform.forward, direction.normalized);
    }

    // IAnimationDispatch
    public void CallAnimationEvent(string animEvent)
    {
        foreach(CharacterAction action in actionList)
        {
            action.CallAnimationEvent(animEvent);
        }
    }
    // Health
    public void GetHit(int damage, GameObject actor)
    {
        hp -= damage;
        if (hp <= 0)
        {
            Die();
        }
        onHealthChanged?.Invoke(hp, maxHP);
    }
    private void Die()
    {
        hp = 0;
        dead = true;
        deathTime = Time.time;
        rb.isKinematic = true;
        playerMesh.SetActive(false);
        playerCollider.gameObject.SetActive(false);
    }
    private void ReviveProcess()
    {
        if(Time.time - deathTime > reviveTime)
        {
            Revive();
        }
    }
    private void Revive()
    {
        dead = false;
        hp = maxHP;
        onHealthChanged?.Invoke(hp, maxHP);
        rb.isKinematic = false;
        playerMesh.SetActive(true);
        playerCollider.gameObject.SetActive(true);
    }

    public void AssignOnHealthChanged(IHealth.OnHealthChanged action)
    {
        onHealthChanged += action;
    }
    public void RemoveOnHealthChanged(IHealth.OnHealthChanged action)
    {
        onHealthChanged -= action;
    }

    public PlayerCamera PlayerCamera => playerCamera;
    public GameObject Mesh => playerMesh;
    public Animator Animator => animator;
    public Rigidbody RB => rb;
    public Collider Collider => playerCollider;
}
