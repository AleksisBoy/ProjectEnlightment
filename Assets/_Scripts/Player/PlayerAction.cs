using UnityEngine;

public class PlayerAction : CharacterAction
{
    protected Player master;
    public override void Init(params object[] objects)
    {
        foreach (var obj in objects)
        {
            if (obj is Player)
            {
                master = (Player)obj;
            }
        }
        base.Init(objects);
    }
}
