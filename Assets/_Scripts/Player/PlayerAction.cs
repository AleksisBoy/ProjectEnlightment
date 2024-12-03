using UnityEngine;

public class PlayerAction : CharacterAction
{
    protected Player master;
    public override void ActionSetup(params object[] objects)
    {
        foreach (var obj in objects)
        {
            if (obj is Player)
            {
                master = (Player)obj;
            }
        }
        base.ActionSetup(objects);
    }
}
