using System.Collections.Generic;
using UnityEngine;

public class CharacterAction : MonoBehaviour, IAnimationDispatch 
{
    [Header("Base")]
    [SerializeField] protected int priority = 0;
    [SerializeField] protected bool update = true;

    protected bool isSetup = false;
    public int Priority => priority;
    public bool Update => update;
    public virtual void ActionSetup(params object[] objects)
    {
        isSetup = true;
    }
    public virtual void ActionUpdate(out bool blockOther) 
    {
        blockOther = false;
    }
    public virtual bool ActionBlocked(CharacterAction blocker) { return true; }
    public virtual void ActionDisturbed(CharacterAction disturber) { }
    public static void SortActions<T>(List<T> actions, out List<T> sortedActions) where T : CharacterAction
    {
        string debug = "Sorted: ";
        sortedActions = new List<T>();
        float mostPriority = -100;
        T priorityAction = null;
        int debugCount = 0;
        while(sortedActions.Count != actions.Count)
        {
            foreach(T t in actions)
            {
                if (sortedActions.Contains(t)) continue;

                if (t.priority > mostPriority)
                {
                    mostPriority = t.priority;
                    priorityAction = t;
                }
            }
            debug += priorityAction.GetType().Name + " / ";
            sortedActions.Add(priorityAction);
            mostPriority = -100;
            priorityAction = null;
            if(++debugCount > 50)
            {
                throw new System.Exception("ERROR WITH SORTING LIST");
            }
        }
        Debug.Log(debug);
    }

    public virtual void CallAnimationEvent(string animEvent) { }
}
