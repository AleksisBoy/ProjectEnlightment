using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IAnimationDispatch, IHealth
{
    [SerializeField] private Animator animator = null;
    [SerializeField] private Collider playerCollider = null;
    [SerializeField] private GameObject playerMesh = null;
    [SerializeField] private UserInterface userInterface = null;
    [Header("Health")]
    [SerializeField] private int maxHP = 100;
    [SerializeField] private float reviveTime = 2f;
    [Header("Pickups")]
    [SerializeField] private int maxHealingPotions = 3;
    [SerializeField] private KeyCode healKey = KeyCode.H;

    private Inventory inventory = null;

    private const string ItemName_HPotion = "HealingPotion";
    private const string ItemName_MPotion = "ManaPotion";
    private const string ItemName_Money = "Money";
    private const string ItemName_AP = "AbilityPoint";

    private int hp = 0;
    private bool dead = false;
    private float deathTime = 0;

    private IHealth.OnHealthChanged onHealthChanged;

    private Rigidbody rb = null;
    private PlayerCamera playerCamera = null;
    private PlayerAnimation playerAnimation = null;

    private List<CharacterAction> actionList = new List<CharacterAction>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        foreach (CharacterAction action in GetComponents<CharacterAction>())
        {
            actionList.Add(action);
            action.ActionSetup(this);
            if (!playerCamera && action as PlayerCamera)
            {
                playerCamera = action as PlayerCamera;
            }
            else if (!playerAnimation && action as PlayerAnimation)
            {
                playerAnimation = action as PlayerAnimation;
            }
        }
        CharacterAction.SortActions<CharacterAction>(actionList, out actionList);
    }
    private void Start()
    {
        InternalSettings.EnableCursor(false);
        inventory = InternalSettings.GetDefaultPlayerInventory();
        if (!userInterface) userInterface = InternalSettings.SpawnUserInterface();
        userInterface.SetPlayer(this);
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
            GetHit(10, null, out bool _died);
        }
        if (Input.GetKeyDown(healKey))
        {
            HealWithPotion();
        }

        bool block = false;
        CharacterAction blocker = null;
        foreach (CharacterAction action in actionList)
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
    public void AddItem(EItem item, int amount)
    {
        inventory.Add(item, amount);
        UI.ItemPickedUp(item, amount);
    }
    public float GetDotProduct(Vector3 otherPosition)
    {
        Vector3 direction = otherPosition - transform.position;
        return Vector3.Dot(transform.forward, direction.normalized);
    }

    public bool RaycastForward(float distance, LayerMask mask, out RaycastHit hit, QueryTriggerInteraction query = QueryTriggerInteraction.Ignore)
    {
        return Physics.Raycast(
            PlayerCamera.Position,
            PlayerCamera.Forward,
            out hit,
            distance,
            mask,
            query);
    }
    public bool BoxCastForward(float distance, Vector3 halfExtents, LayerMask mask, out Collider[] hits, QueryTriggerInteraction query = QueryTriggerInteraction.Ignore)
    {
        hits = Physics.OverlapBox(
            (PlayerCamera.Position + playerCamera.Position + (playerCamera.Forward * distance)) / 2f,
            halfExtents,
            playerCamera.Rotation,
            mask,
            query);

        /*
        hits = Physics.BoxCastAll(
            (PlayerCamera.Position + playerCamera.Position + (playerCamera.Forward * distance)) / 2f,
            halfExtents,
            PlayerCamera.Forward,
            PlayerCamera.Rotation,
            distance,
            mask,
            query);
        */

        return hits.Length > 0;
    }
    // Input
    public static Vector3 GetMoveInputVector()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 inputVector = new Vector3(horizontal, 0f, vertical);
        if (horizontal != 0 && vertical != 0)
        {
            inputVector *= 0.71f;
        }

        return inputVector;
    }
    public static Vector2 GetMouseInputVector()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        Vector2 inputVector = new Vector2(mouseX, mouseY);

        return inputVector;
    }
    // IAnimationDispatch
    public void CallAnimationEvent(string animEvent)
    {
        foreach (CharacterAction action in actionList)
        {
            action.CallAnimationEvent(animEvent);
        }
    }
    // Health
    public void GetHit(int damage, GameObject actor, out bool died)
    {
        died = false;
        hp -= damage;
        if (hp <= 0)
        {
            died = true;
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
        if (Time.time - deathTime > reviveTime)
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
    private void HealWithPotion()
    {
        if (inventory.HasItem(ItemName_HPotion, out Inventory.Item hpotion) || hpotion.amount <= 0) return; // add feedback that not enough potions
        if (hp >= maxHP) return; // add feedback that player is max hp

        hp = Mathf.Min(maxHP, hp + (int)(maxHP * InternalSettings.HealPotionStrength));
        onHealthChanged?.Invoke(hp, maxHP);
        hpotion.amount--;
    }

    public void AssignOnHealthChanged(IHealth.OnHealthChanged action)
    {
        onHealthChanged += action;
    }
    public void RemoveOnHealthChanged(IHealth.OnHealthChanged action)
    {
        onHealthChanged -= action;
    }
    public Animator GetAnimator()
    {
        return animator;
    }

    public PlayerCamera PlayerCamera => playerCamera;
    public PlayerAnimation PlayerAnimation => playerAnimation;
    public GameObject Mesh => playerMesh;
    public Animator Animator => animator;
    public Rigidbody RB => rb;
    public Collider Collider => playerCollider;
    public Inventory Inventory => inventory;
    public UserInterface UI => userInterface;
}