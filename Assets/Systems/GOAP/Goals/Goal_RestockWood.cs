using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal_RestockWood : BaseGoal
{
    public override bool CanRun(GOAPState currentState)
    {
        return true;
    }

    public override void RefreshPriority()
    {

    }

    public override GOAPState GetDesiredState()
    {
        var desiredState = new GOAPState();
        desiredState.SetFlag(EStateFlags.Restocked_Wood);
        return desiredState;
    }
}
