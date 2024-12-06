using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IAnimationDispatch, IHealth
{
    [SerializeField] private Animator animator = null;
    [SerializeField] private Collider playerCollider = null;
    [SerializeField] private GameObject playerMesh = null;
    [Header("Health")]
    [SerializeField] private int maxHP = 100;
    [SerializeField] private float reviveTime = 2f;
    [Header("Pickups")]
    [SerializeField] private int maxHealingPotions = 3;
    [SerializeField] private KeyCode healKey = KeyCode.H;

    private int money = 0;
    private int healingPotions = 0;
    private int manaPotions = 0;
    private int abilityPoints = 0;

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
            GetHit(10, null);
        }
        if (Input.GetKeyDown(healKey))
        {
            HealWithPotion();
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
    public void AddPickup(PlayerItemPickup.Type type, int amount)
    {
        switch (type)
        {
            case PlayerItemPickup.Type.Money:
                money += amount;
                break;
            case PlayerItemPickup.Type.HealingPotion:
                healingPotions += amount;
                break;
            case PlayerItemPickup.Type.ManaPotion:
                manaPotions += amount;
                break;
            case PlayerItemPickup.Type.AbilityPoint:
                abilityPoints += amount;
                break;
        }
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
    private void HealWithPotion()
    {
        if (healingPotions <= 0) return; // add feedback that not enough potions
        if (hp >= maxHP) return; // add feedback that player is max hp

        hp = Mathf.Min(maxHP, hp + (int)(maxHP * InternalSettings.HealPotionStrength));
        healingPotions--;
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

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 500, 80), string.Format("Coins: {0}", money), InternalSettings.DebugStyle);
        GUI.Label(new Rect(10, 90, 500, 80), string.Format("HP: {0}", hp), InternalSettings.DebugStyle);
        GUI.Label(new Rect(10, 170, 500, 80), string.Format("HPotions: {0}", healingPotions), InternalSettings.DebugStyle);
        GUI.Label(new Rect(10, 260, 500, 80), string.Format("MPotions: {0}", manaPotions), InternalSettings.DebugStyle);
        GUI.Label(new Rect(10, 340, 500, 80), string.Format("APs: {0}", abilityPoints), InternalSettings.DebugStyle);
    }
}
