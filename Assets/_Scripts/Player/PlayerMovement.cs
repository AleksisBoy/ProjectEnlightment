using System.Collections;
using UnityEngine;

public class PlayerMovement : PlayerAction
{
    [Header("PlayerMovement")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float walkSpeed = 5.5f;
    [SerializeField] private float sprintSpeed = 7f;
    [SerializeField] private float inAirSpeed = 4f;
    [SerializeField] private float crouchSpeedModifer = 0.5f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float crouchLerpSeconds = 0.25f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private Vector3 groundBoxCenterOffset = new Vector3(0, 0.3f, 0f);
    [SerializeField] private Vector3 groundBoxHalfExtents = new Vector3(0.4f, 0.05f, 0.4f);
    [SerializeField] private float groundCheckDistance = 0.4f;
    [SerializeField] private float minFallVelocity = -5f;
    [SerializeField] private float damageMultiplierPerVelocity = 1.5f;
    [Header("Vaulting")]
    [SerializeField] private Vector3 vaultRayOffset = new Vector3(0f, 0.3f, 0f);
    [SerializeField] private float vaultingDotMin = 0.8f;
    [SerializeField] private float vaultingCloseDistance = 0.5f;
    [SerializeField] private float vaultingStepOffset = 0.2f;
    [SerializeField] private float heightRayLength = 6f;
    [SerializeField] private float vaultingHeightMax = 1f;
    [SerializeField] private float vaultingHeightMin = 0.5f;
    [SerializeField] private Vector2 vaultSecondsRange = new Vector2(0.2f,0.4f);
    [Header("Input")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;

    private bool vaulting = false;

    private bool isGrounded = true;
    private Vector3 lastGroundedPosition;

    private bool isCrouched = false;
    private bool isCrouchingLerping = false;
    private void OnValidate()
    {
        if (crouchLerpSeconds <= 0f) crouchLerpSeconds = 0.01f;
        if (vaultSecondsRange.x <= 0f) vaultSecondsRange.x = 0.01f;
        if (vaultSecondsRange.y <= 0f) vaultSecondsRange.y = 0.01f;
    }
    public override void ActionUpdate(out bool blockOther)
    {
        if (vaulting)
        {
            blockOther = true;
            return;
        }
        blockOther = false;

        bool jumpKeyDown = Input.GetKeyDown(jumpKey);
        bool sprintKeyHold = Input.GetKey(sprintKey);
        bool crouchKeyDown = Input.GetKeyDown(crouchKey);

        Vector3 moveInput = Player.GetMoveInputVector();

        if (VaultProcess(jumpKeyDown, moveInput)) return;

        if (crouchKeyDown) Crouch();

        float speed = sprintKeyHold ? sprintSpeed : walkSpeed;
        float maxSpeed = sprintSpeed;
        if (isCrouched)
        {
            speed *= crouchSpeedModifer;
            maxSpeed *= crouchSpeedModifer;
        }

        Move(moveInput, speed, maxSpeed, true, true, jumpKeyDown);
    }

    private bool VaultProcess(bool jumpKeyDown, Vector3 moveInput)
    {
        Quaternion cameraLookRotation = Quaternion.LookRotation(new Vector3(master.Camera.Forward.x, 0f, master.Camera.Forward.z), Vector3.up);

        ObstacleInfo hitData = new ObstacleInfo();
        hitData.hitFound = Physics.Raycast(transform.position + vaultRayOffset,
            new Vector3(master.Mesh.transform.forward.x, 0f, master.Mesh.transform.forward.z).normalized,
            out hitData.hit,
            vaultingCloseDistance,
            InternalSettings.ObstacleLayer);

        //// Ray to main hit
        //Debug.DrawRay(transform.position + vaultRayOffset,
        //    new Vector3(master.Mesh.transform.forward.x, 0f, master.Mesh.transform.forward.z).normalized * vaultingCloseDistance,
        //    hitData.hitFound ? Color.green : Color.red);

        if (hitData.hitFound)
        {
            hitData.heightHitFound = Physics.SphereCast(hitData.hit.point + Vector3.up * heightRayLength,
                0.001f,
                Vector3.down,
                out hitData.heightHit,
                heightRayLength,
                InternalSettings.ObstacleLayer);

            //// Height hit ray
            //Debug.DrawRay(hitData.hit.point + Vector3.up * heightRayLength,
            //    Vector3.down * heightRayLength,
            //    hitData.heightHitFound ? Color.green : Color.red);

            if (hitData.heightHitFound)
            {
                float dot = Vector3.Dot(-hitData.hit.normal, cameraLookRotation * moveInput);
                float heightDiff = hitData.heightHit.point.y - transform.position.y;

                if (jumpKeyDown
                    && dot > vaultingDotMin
                    && heightDiff > vaultingHeightMin
                    && heightDiff < vaultingHeightMax
                    && CheckOverlapOnObstacleInfo(hitData) == 0)
                {
                    float perc = NovUtil.GetRangePercentage(heightDiff, vaultingHeightMax, vaultingHeightMin);
                    float vaultSeconds = Mathf.Lerp(vaultSecondsRange.x, vaultSecondsRange.y, perc);
                    StartCoroutine(VaultOver(hitData, vaultSeconds));
                    return true;
                }
            }
        }
        return false;
    }

    private int CheckOverlapOnObstacleInfo(ObstacleInfo hitData)
    {
        Collider[] colls = new Collider[1];

        int overlappedCount = Physics.OverlapCapsuleNonAlloc(hitData.heightHit.point + (-hitData.hit.normal * vaultingStepOffset) + new Vector3(0f, 0.4f, 0f),
                    hitData.heightHit.point + (-hitData.hit.normal * vaultingStepOffset) +
                    new Vector3(0f, master.Mesh.transform.position.y - transform.position.y, 0f),
                    master.Radius,
                    colls,
                    InternalSettings.EnvironmentLayer);
        foreach(Collider coll in colls)
        {
            Debug.Log(coll?.name);
        }

        return overlappedCount;
    }
    private IEnumerator VaultOver(ObstacleInfo obstacle, float vaultSeconds)
    {
        master.Animator.SetFloat("VaultSpeed", 1f / vaultSeconds);
        master.Animator.SetTrigger("Vault");

        vaulting = true;
        master.RB.isKinematic = true;

        Vector3 startPos = transform.position;
        Vector3 endPos = obstacle.heightHit.point + (-obstacle.hit.normal * vaultingStepOffset);
        float weight = 0f;
        while(weight < 1f)
        {
            transform.position = Vector3.Lerp(startPos, endPos, weight);
            master.Camera.LookAtSmooth(endPos, 1f);
            weight += Time.deltaTime / vaultSeconds;
            yield return null;
        }

        transform.position = endPos;

        master.RB.isKinematic = false;
        vaulting = false;
    }
    public override bool ActionBlocked(CharacterAction blocker)
    {
        //if (!isGrounded) return false;
        if (vaulting) return false;

        // Combat block react
        if (blocker as PlayerCombat)
        {
            bool crouchKeyDown = Input.GetKeyDown(crouchKey);
            if (crouchKeyDown) Crouch();

            Move(Player.GetMoveInputVector(), walkSpeed / 1.25f, sprintSpeed, true, true, false);
            return true;
        }

        Vector3 velocity = master.RB.linearVelocity;
        if (velocity.x != 0) velocity.x = NovUtil.MoveTowards(velocity.x, 0, Time.deltaTime * 10f);
        if (velocity.z != 0) velocity.z = NovUtil.MoveTowards(velocity.z, 0, Time.deltaTime * 10f);
        //master.RB.linearVelocity = velocity;
        master.Animator.SetFloat(NovUtil.SpeedHash, velocity.magnitude / walkSpeed);


        /*
        else if (blocker as PlayerCarry)
        {
            PlayerCarry carry = (PlayerCarry)blocker;

            if (carry.CanWalk())
            {
                Move(GetInputVector(), carry.WalkSpeed, carry.WalkSpeed, true, true, false);
            }
            else
            {
                master.RB.linearVelocity = Vector3.zero;
            }
        }
        */
        return true;
    }
    private void Move(Vector3 inputVector, float baseSpeed, float maxSpeed,
        bool setVelocity, bool setRotation, bool spaceDown)
    {
        float speed = baseSpeed;
        //Quaternion cameraLookRotation = master.PlayerCamera.CurrentRotationFlat;
        Quaternion cameraLookRotation = Quaternion.LookRotation(new Vector3(master.Camera.Forward.x, 0f, master.Camera.Forward.z), Vector3.up);
        Vector3 velocity = cameraLookRotation * inputVector;
        Quaternion playerLookRotation = cameraLookRotation;
        //Quaternion playerLookRotation = Quaternion.Slerp(master.RB.rotation, cameraLookRotation, rotationSpeed * Time.deltaTime);
        velocity.y = master.RB.linearVelocity.y;
        GroundCheck(spaceDown, ref speed, ref velocity, playerLookRotation);

        velocity = new Vector3(velocity.x * speed, velocity.y, velocity.z * speed);

        if (setRotation)
        {
            Quaternion rot = Quaternion.Euler(new Vector3(master.Camera.transform.eulerAngles.x, master.Camera.transform.eulerAngles.y, 0f));

            //master.Mesh.transform.localRotation = rot;
            //master.Mesh.transform.rotation = playerLookRotation;
            //master.RB.MoveRotation(playerLookRotation);
        }
        if (setVelocity)
        {
            master.Animator.SetFloat(NovUtil.SpeedHash,
                Mathf.Lerp(master.Animator.GetFloat(NovUtil.SpeedHash), velocity.magnitude / maxSpeed, 6f * Time.deltaTime));
            master.RB.linearVelocity = velocity;
        }
    }
    private void GroundCheck(bool spaceDown, ref float speed, ref Vector3 velocity, Quaternion playerLookRotation)
    {
        //if(Physics.CapsuleCast(transform.position + new Vector3(0f, 0.1f, 0f), transform.position + Vector3.up, 0.3f, Vector3.down, 0.1f, groundMask))
        if (Physics.BoxCast(transform.position + groundBoxCenterOffset, groundBoxHalfExtents, Vector3.down, out RaycastHit hit, playerLookRotation, groundCheckDistance, groundMask))
        {
            if (!isGrounded)
            {
                CheckFallDamage();
            }
            isGrounded = true;
            if (spaceDown)
            {
                Jump(ref velocity);
            }
            lastGroundedPosition = master.RB.position;
        }
        else
        {
            isGrounded = false;
            speed *= 0.9f;
        }
        //master.Animator.SetBool(NovUtil.IsGroundedHash, isGrounded);
    }
    private void CheckFallDamage()
    {
        if(master.RB.linearVelocity.y < minFallVelocity)
        {
            float damage = 1f;
            for (int i = 0; i < -master.RB.linearVelocity.y; i++)
            {
                damage *= damageMultiplierPerVelocity;
            }
            Debug.Log(damage);
            master.GetHit((int)damage, IHealth.DamageType.Fall, null, out bool died);
        }
    }
    private void Crouch()
    {
        if (!isGrounded || isCrouchingLerping) return;

        isCrouched = !isCrouched;
        // move head down
        float mod = isCrouched ? 0.5f : 2f;
        StartCoroutine(TransformLerping(master.Mesh.transform,
            master.Mesh.transform.localPosition * mod, crouchLerpSeconds));
        master.Animator.SetBool(NovUtil.IsCrouchedHash, isCrouched);
    }
    private IEnumerator TransformLerping(Transform obj, Vector3 newLocalPos, float seconds)
    {
        isCrouchingLerping = true;
        float weight = 0f;
        Vector3 startPos = obj.localPosition;
        while (weight < 1f)
        {
            obj.localPosition = Vector3.Lerp(startPos, newLocalPos, weight);
            weight += Time.deltaTime / seconds;
            yield return null;
        }
        obj.localPosition = newLocalPos;
        isCrouchingLerping = false;
    }
    private void Jump(ref Vector3 velocity)
    {
        velocity.y += jumpForce;
        //master.Animator.SetTrigger(NovUtil.JumpHash);
    }
    private void OnDrawGizmosSelected()
    {
        // transform.position + new Vector3(0, 0.3f, 0f)
        if (groundBoxHalfExtents.y <= 0f) return;
        for (int i = 0; i < groundCheckDistance / groundBoxHalfExtents.y; i++)
        {
            Gizmos.DrawCube(transform.position + groundBoxCenterOffset -
                new Vector3(0f, groundBoxHalfExtents.y * i, 0f), groundBoxHalfExtents * 2f);

        }
    }
}
public struct ObstacleInfo 
{
    public bool hitFound;
    public bool heightHitFound;
    public RaycastHit hit;
    public RaycastHit heightHit;
}
