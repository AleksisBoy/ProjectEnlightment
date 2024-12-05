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
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private Vector3 groundBoxCenterOffset = new Vector3(0, 0.3f, 0f);
    [SerializeField] private Vector3 groundBoxHalfExtents = new Vector3(0.4f, 0.05f, 0.4f);
    [SerializeField] private float groundCheckDistance = 0.4f;
    [SerializeField] private Transform cameraTransform = null;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;

    private bool isGrounded = true;
    private Vector3 lastGroundedPosition;

    private bool isCrouched = false;
    public override void ActionUpdate(out bool blockOther)
    {
        blockOther = false;

        bool jumpKeyDown = Input.GetKeyDown(jumpKey);
        bool sprintKeyHold = Input.GetKey(sprintKey);
        bool crouchKeyDown = Input.GetKeyDown(crouchKey);

        if (crouchKeyDown) Crouch();

        float speed = sprintKeyHold ? sprintSpeed : walkSpeed;
        float maxSpeed = sprintSpeed;
        if (isCrouched)
        {
            speed *= crouchSpeedModifer;
            maxSpeed *= crouchSpeedModifer;
        }

        Move(GetMoveInputVector(), speed, maxSpeed, true, true, jumpKeyDown);
    }
    public override bool ActionBlocked(CharacterAction blocker) 
    {
        if (!isGrounded) return false;

        Vector3 velocity = master.RB.linearVelocity;
        if (velocity.x != 0) velocity.x = NovUtil.MoveTowards(velocity.x, 0, Time.deltaTime * 10f);
        if (velocity.z != 0) velocity.z = NovUtil.MoveTowards(velocity.z, 0, Time.deltaTime * 10f);
        master.RB.linearVelocity = velocity;
        master.Animator.SetFloat(NovUtil.SpeedHash, velocity.magnitude / walkSpeed);

        /* // Combat block react
        if (blocker as PlayerCombat)
        {
            Vector3 inputVector = GetInputVector();
            Move(inputVector, 0, 1, false, true, false);

            master.RB.position += master.RB.rotation * master.Mesh.transform.localPosition;
            master.Mesh.transform.localPosition = Vector3.zero;
        }
        */
        
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
        Quaternion cameraLookRotation = Quaternion.LookRotation(new Vector3(cameraTransform.forward.x, 0f, cameraTransform.forward.z), Vector3.up);
        Vector3 velocity = cameraLookRotation * inputVector;
        Quaternion playerLookRotation = cameraLookRotation;
        //Quaternion playerLookRotation = Quaternion.Slerp(master.RB.rotation, cameraLookRotation, rotationSpeed * Time.deltaTime);
        velocity.y = master.RB.linearVelocity.y;
        GroundCheck(spaceDown, ref speed, ref velocity, playerLookRotation);

        velocity = new Vector3(velocity.x * speed, velocity.y, velocity.z * speed);

        if (setRotation)
        {
            Quaternion rot = Quaternion.Euler(new Vector3(master.PlayerCamera.transform.eulerAngles.x, master.PlayerCamera.transform.eulerAngles.y, 0f));

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
        if (Physics.BoxCast(transform.position + groundBoxCenterOffset, groundBoxHalfExtents, Vector3.down, playerLookRotation, groundCheckDistance, groundMask))
        {
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
    private void Crouch()
    {
        if (!isGrounded) return;

        isCrouched = !isCrouched;
        // move head down
        float mod = isCrouched ? 0.5f : 2f;
        master.Mesh.transform.localPosition *= mod;
        master.Animator.SetBool(NovUtil.IsCrouchedHash, isCrouched);
    }
    private void Jump(ref Vector3 velocity)
    {
        velocity.y += jumpForce;
        master.Animator.SetTrigger(NovUtil.JumpHash);
    }
    private static Vector3 GetMoveInputVector()
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
    private void OnDrawGizmosSelected()
    {
        // transform.position + new Vector3(0, 0.3f, 0f)
        if (groundBoxHalfExtents.y <= 0f) return;
        for(int i = 0; i < groundCheckDistance / groundBoxHalfExtents.y; i++)
        {
            Gizmos.DrawCube(transform.position + groundBoxCenterOffset -
                new Vector3(0f, groundBoxHalfExtents.y * i, 0f), groundBoxHalfExtents * 2f);

        }
    }
}
