using UnityEngine;

public class PlayerAnimation : PlayerAction
{
    [SerializeField] private Animator playerAnimator = null;
    [SerializeField] private Animator handsAnimator = null;

    private Transform sourceA = null;
    private Transform sourceB = null;
    private float weight = 0f;

    private Vector3 direction = Vector3.zero;

    private Animator otherAnimator = null;
    private Vector3 animOffset = Vector3.zero;
    private bool animating = false;
    private bool lerping = false;
    private string playerAnimationName; 
    private string otherAnimationName; 
    private void Start()
    {
    }
    // very bad, will remake it later
    public override void ActionUpdate(out bool blockOther)
    {
        if (lerping)
        {
            weight += Time.deltaTime / 0.18f;
            transform.position = Vector3.Lerp(sourceA.position, sourceB.position + animOffset, weight);
            master.Mesh.transform.forward = Vector3.Lerp(sourceA.forward, direction, weight);
            if (weight >= 1f)
            {
                lerping = false;
                master.Animator.SetTrigger(playerAnimationName);
                otherAnimator.SetTrigger(otherAnimationName);
            }
        }
        blockOther = animating;
    }
    public override void CallAnimationEvent(string animEvent)
    {
        switch (animEvent)
        {
            case "PlayerAnimationFinish":
                animating = false;
                master.RB.isKinematic = false;
                master.PlayerCamera.SetPan(master.Mesh.transform.eulerAngles.y);
                master.PlayerCamera.SetTilt(master.Mesh.transform.eulerAngles.x);
                break;
        }
    }
    // very bad, will remake it later
    public void LaunchAssassination(Animator enemy, Vector3 animOffset)
    {
        if (animating || !enemy) return;

        playerAnimationName = "Assassination01";
        otherAnimationName = "Assassination01";
        master.RB.isKinematic = true;
        this.animOffset = animOffset;
        animating = true;
        lerping = true;
        otherAnimator = enemy;
        master.Animator.ResetTrigger(NovUtil.AttackHash);
        GameObject go = new GameObject();
        Destroy(go, 20f);
        go.transform.position = transform.position;
        direction = enemy.transform.position - (enemy.transform.position + animOffset);
        direction.y = -0.5f;
        direction.x += 0.2f;
        direction.Normalize();
        go.transform.forward = master.Mesh.transform.forward;
        sourceA = go.transform;
        sourceB = enemy.transform;
        weight = 0f;
    }
    // very bad, will remake it later
    public void LaunchStealthAssassination(Animator enemy, Vector3 animOffset)
    {
        if (animating || !enemy) return;

        playerAnimationName = "Assassination01";
        otherAnimationName = "StealthDeath";
        master.RB.isKinematic = true;
        this.animOffset = animOffset;
        animating = true;
        lerping = true;
        otherAnimator = enemy;
        master.Animator.ResetTrigger(NovUtil.AttackHash);
        GameObject go = new GameObject();
        Destroy(go, 20f);
        go.transform.position = transform.position;
        direction = enemy.transform.position - (enemy.transform.position + animOffset);
        direction.y = -0.2f;
        direction.x += -0.25f;
        direction.Normalize();
        go.transform.forward = master.Mesh.transform.forward;
        sourceA = go.transform;
        sourceB = enemy.transform;
        weight = 0f;
    }
}
