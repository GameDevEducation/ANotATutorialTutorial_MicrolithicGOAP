using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_Consume : BaseAction
{
    public override bool CanAchieve(GOAPState targetState)
    {
        if (targetState.GetFlag(EStateFlags.Consumed_Food) || targetState.GetFlag(EStateFlags.Consumed_Water))
            return true;
            
        return false;
    }

    public override bool CanRun(GOAPState currentState)
    {
        return currentState.GetFlag(EStateFlags.Holding_Food) || currentState.GetFlag(EStateFlags.Holding_Water);
    }

    public override GOAPState CalculateState(GOAPState currentState, GOAPState targetState)
    {
        var newState = currentState.Clone();

        if (currentState.GetFlag(EStateFlags.Holding_Food))
            newState.SetFlag(EStateFlags.Consumed_Food);
        else if (currentState.GetFlag(EStateFlags.Holding_Water))
            newState.SetFlag(EStateFlags.Consumed_Water);

        return newState;
    }

    public override float GetCost(GOAPState currentState)
    {
        return BaseCost;
    }

    protected override EActionResult Tick_MoveIntoPosition(GOAPState currentState)
    {
        return EActionResult.Complete;
    }

    protected override EActionResult Tick_Perform(GOAPState currentState)
    {
        if (currentState.GetFlag(EStateFlags.Holding_Water))
            Agent.Consume(Resources.EType.Water);
        else if (currentState.GetFlag(EStateFlags.Holding_Food))
            Agent.Consume(Resources.EType.Food);

        return EActionResult.Complete;
    }
}
