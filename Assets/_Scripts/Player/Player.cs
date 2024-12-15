using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IAnimationDispatch, IHealth, IActor, IMana
{
    [SerializeField] private Animator animator = null;
    [SerializeField] private Collider playerCollider = null;
    [SerializeField] private GameObject playerMesh = null;
    [SerializeField] private UserInterface userInterface = null;
    [SerializeField] private float radius = 0.35f;
    [Header("Health")]
    [SerializeField] private int maxHP = 100;
    [SerializeField] private float reviveTime = 2f;
    [Header("Mana")]
    [SerializeField] private float maxMana = 100;
    [SerializeField] private float manaRestoreTimer = 8f;
    [SerializeField] private float manaRestorePerSec = 1f;
    [Header("Pickups")]
    [SerializeField] private int maxHealingPotions = 3;

    private bool isTeleporting = false;

    private int hp = 0;
    private bool dead = false;
    private float deathTime = 0;
    private IHealth.OnHealthChanged onHealthChanged;

    private float mana = 0;
    private float manaRestoreGap = 0;
    private float lastManaUseTime = 0f;
    private IMana.OnManaChanged onManaChanged;

    private Rigidbody rb = null;
    private PlayerCamera playerCamera = null;
    private PlayerAnimation playerAnimation = null;
    private PlayerEquipment equipment = null;

    private List<CharacterAction> actionList = new List<CharacterAction>();

    private void OnValidate()
    {
        if (!playerCollider) return;

        if (playerCollider as BoxCollider)
        {
            BoxCollider box = (BoxCollider)playerCollider;
            box.size = new Vector3(radius * 2f, box.size.y, radius * 2f);
        }
        else if (playerCollider as CapsuleCollider)
        {
            CapsuleCollider capsule = (CapsuleCollider)playerCollider;
            capsule.radius = radius;
        }
        else
        {
            playerCollider.transform.localScale = new Vector3(radius * 2f, playerCollider.transform.localScale.y, radius * 2f);
        }
    }
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        CharacterAction[] actions = GetComponents<CharacterAction>();
        foreach (CharacterAction action in actions)
        {
            if (!playerCamera && action as PlayerCamera)
            {
                playerCamera = action as PlayerCamera;
            }
            else if (!playerAnimation && action as PlayerAnimation)
            {
                playerAnimation = action as PlayerAnimation;
            }
            else if (!equipment && action as PlayerEquipment)
            {
                equipment = action as PlayerEquipment;
            }
        }
        foreach (CharacterAction action in actions)
        {
            actionList.Add(action);
            action.Init(this);
        }
        CharacterAction.SortActions<CharacterAction>(actionList, out actionList);
    }
    private void Start()
    {
        InternalSettings.EnableCursor(false);
        if (!userInterface) userInterface = InternalSettings.SpawnUserInterface();
        userInterface.SetPlayer(this);
        equipment.Equip(InternalSettings.SwordItem);
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
        if (NovUtil.TimeCheck(lastManaUseTime, manaRestoreTimer))
        {
            mana = NovUtil.MoveTowards(mana, manaRestoreGap, manaRestorePerSec * Time.deltaTime);
            onManaChanged((int)mana, maxMana);
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
        equipment.AddItem(item, amount);
        UI.ItemPickedUp(item, amount);
    }
    public float GetDotProduct(Vector3 otherPosition)
    {
        Vector3 direction = otherPosition - transform.position;
        return Vector3.Dot(transform.forward, direction.normalized);
    }

    public bool RaycastForward(float distance, LayerMask mask, out RaycastHit hit, out Vector3 direction, QueryTriggerInteraction query = QueryTriggerInteraction.Ignore)
    {
        direction = Camera.Forward;

        return Physics.Raycast(
            Camera.Position,
            Camera.Forward,
            out hit,
            distance,
            mask,
            query);
    }
    public bool RaycastForwardAll(float distance, LayerMask mask, out RaycastHit[] hits, QueryTriggerInteraction query = QueryTriggerInteraction.Ignore)
    {
        hits = Physics.RaycastAll(
            Camera.Position,
            Camera.Forward,
            distance,
            mask,
            query);

        return hits.Length > 0;
    }
    public bool OverlapBoxForward(float distance, Vector3 halfExtents, LayerMask mask, out Collider[] colls, QueryTriggerInteraction query = QueryTriggerInteraction.Ignore)
    {
        colls = Physics.OverlapBox(
            (Camera.Position + playerCamera.Position + (playerCamera.Forward * distance)) / 2f,
            halfExtents,
            playerCamera.Rotation,
            mask,
            query);


        return colls.Length > 0;
    }
    public bool BoxCastForwardAll(float distance, Vector3 halfExtents, LayerMask mask, out RaycastHit[] hits, QueryTriggerInteraction query = QueryTriggerInteraction.Ignore)
    {
        hits = Physics.BoxCastAll(
            Camera.Position + Camera.Forward * 0.1f,
            halfExtents,
            Camera.Forward,
            Camera.Rotation,
            distance,
            mask,
            query);

        return hits.Length > 0;
    }
    public bool BoxCastForward(float distance, Vector3 halfExtents, LayerMask mask, out RaycastHit hit, out Vector3 direction, QueryTriggerInteraction query = QueryTriggerInteraction.Ignore)
    {
        direction = Camera.Forward;

        return Physics.BoxCast(
            Camera.Position + Camera.Forward * 0.1f,
            halfExtents,
            Camera.Forward,
            out hit,
            Camera.Rotation,
            distance,
            mask,
            query);
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
    public void CallAnimationEvent(NovUtil.AnimEvent animEvent)
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
        mana = maxMana;
        manaRestoreGap = maxMana;
        onHealthChanged?.Invoke(hp, maxHP);
        onManaChanged?.Invoke((int)mana, maxMana);
        rb.isKinematic = false;
        playerMesh.SetActive(true);
        playerCollider.gameObject.SetActive(true);
    }
    public bool TryHeal (uint add)
    {
        if (hp >= maxHP) return false; // add feedback that player is max hp

        Heal((int)add);
        return true;
    }
    public bool TryHealFromMax (float perc)
    {
        if (hp >= maxHP) return false; // add feedback that player is max hp

        int heal = (int)(maxHP * perc);
        Heal(heal);
        return true;
    }
    private void Heal(int add)
    {
        hp = Mathf.Min(maxHP, hp + add);
        onHealthChanged?.Invoke(hp, maxHP);
    }

    public void AssignOnHealthChanged(IHealth.OnHealthChanged action)
    {
        onHealthChanged += action;
    }
    public void RemoveOnHealthChanged(IHealth.OnHealthChanged action)
    {
        onHealthChanged -= action;
    }
    // IActor

    public void ProcessActorData(IActor.Data data)
    {
        switch (data.type)
        {
            case IActor.Data.Type.StaminaPerc:
                SetCameraNoise(data.value);
                break;
        }
    }
    public void Teleport(Vector3 position)
    {
        StartCoroutine(TeleportSequence(position));
    }
    private IEnumerator TeleportSequence(Vector3 position)
    {
        isTeleporting = true;
        Vector3 endPos = position - new Vector3(0f, GetHeight(), 0f);
        Vector3 startPos = rb.position;
        rb.useGravity = false;
        float weight = 0f;
        float teleportSpeed = 6f;
        while (weight < 1f)
        {
            rb.position = Vector3.Lerp(startPos, endPos, weight);
            weight += Time.fixedDeltaTime * teleportSpeed;
            yield return new WaitForFixedUpdate();
        }
        rb.position = endPos;
        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero;
        isTeleporting = false;
    }
    public bool CanTeleport()
    {
        return !isTeleporting;
    }
    private void SetCameraNoise(float perc)
    {
        Camera.SetNoise(perc);
    }
    public Animator GetAnimator()
    {
        return animator;
    }
    public IHealth GetHealth() => this;
    public GameObject GetGameObject() => gameObject;
    public IMana GetMana() => this;

    public float GetHeight()
    {
        return Mesh.transform.position.y - transform.position.y;
    }
    // IMana
    public bool UseMana(float usedMana)
    {
        if (mana >= usedMana)
        {
            manaRestoreGap = mana;
            mana -= usedMana;
            lastManaUseTime = Time.time;    
            onManaChanged?.Invoke(mana, maxMana);
            return true;
        }
        else return false;
    }
    public bool TryRestoreMana(float restoreMana)
    {
        if (mana >= maxMana) return false;

        mana += restoreMana;
        manaRestoreGap = mana;
        onManaChanged?.Invoke(mana, maxMana);

        return true;
    }
    public void AssignOnManaChanged(IMana.OnManaChanged action)
    {
        onManaChanged += action;
    }

    public void RemoveOnManaChanged(IMana.OnManaChanged action)
    {
        onManaChanged -= action;
    }

    public PlayerCamera Camera => playerCamera;
    public PlayerAnimation PlayerAnimation => playerAnimation;
    public PlayerEquipment Equipment => equipment;
    public GameObject Mesh => playerMesh;
    public Animator Animator => animator;
    public Rigidbody RB => rb;
    public Collider Collider => playerCollider;
    public UserInterface UI => userInterface;
    public float Radius => radius;
}