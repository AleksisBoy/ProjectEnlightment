using UnityEngine;

public class AnimationEventDispatcher : MonoBehaviour
{
    [SerializeField] private GameObject target = null;
    [SerializeField] private string[] animationEvents;

    private IAnimationDispatch dispatch;

    private void Awake()
    {
        dispatch = target.GetComponent<IAnimationDispatch>();
        if(dispatch == null)
        {
            throw new System.Exception("NO INTERFACE ON THE TARGET");
        }
        target = null;
    }
    public void CallEvent(int index)
    {
        if (dispatch == null)
        {
            Debug.LogError("Calling null dispatch");
            return;
        }

        dispatch.CallAnimationEvent(animationEvents[index]);
    }
}
