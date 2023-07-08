using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal_SatisfyHunger : BaseGoal
{
    public override bool CanRun(GOAPState currentState)
    {
        return currentState.GetFlag(EStateFlags.IsHungry);
    }

    public override void RefreshPriority()
    {
        Priority = Mathf.RoundToInt(BaseGoal.MaxPriority * (1f - LinkedCharacter.FoodPercent));
    }

    public override GOAPState GetDesiredState()
    {
        var desiredState = new GOAPState();
        desiredState.SetFlag(EStateFlags.Consumed_Food);
        return desiredState;
    }
}
