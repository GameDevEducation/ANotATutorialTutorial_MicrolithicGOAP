using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal_SatisfyThirst : BaseGoal
{
    public override bool CanRun(GOAPState currentState)
    {
        return currentState.GetFlag(EStateFlags.IsThirsty);
    }

    public override void RefreshPriority()
    {
        Priority = Mathf.RoundToInt(BaseGoal.MaxPriority * (1f - LinkedCharacter.WaterPercent));
    }

    public override GOAPState GetDesiredState()
    {
        var desiredState = new GOAPState();
        desiredState.SetFlag(EStateFlags.Consumed_Water);
        return desiredState;
    }
}
