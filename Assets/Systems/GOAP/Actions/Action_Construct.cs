using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_Construct : BaseAction
{
    public override bool CanAchieve(GOAPState targetState)
    {
        if (targetState.GetFlag(EStateFlags.Expanded_Storage))
            return true;
            
        return false;
    }

    public override bool CanRun(GOAPState currentState)
    {
        return currentState.GetFlag(EStateFlags.Holding_Wood);
    }

    public override GOAPState CalculateState(GOAPState currentState, GOAPState targetState)
    {
        var newState = currentState.Clone();
        newState.SetFlag(EStateFlags.Expanded_Storage);
        newState.SetCurrentTarget(ResScanner.FindSmallestContainer());

        if (newState.CurrentTarget == null)
            return null;

        return newState;
    }

    public override float GetCost(GOAPState currentState)
    {
        float distance = (currentState.CurrentTarget.transform.position - currentState.Location).magnitude;

        return BaseCost + (distance * CostPerDistance);
    }

    protected override EActionResult Tick_MoveIntoPosition(GOAPState currentState)
    {
        Agent.SetDestination(currentState.CurrentTarget.transform.position);
        return Agent.AtDestination ? EActionResult.Complete : EActionResult.InProgress;
    }

    protected override EActionResult Tick_Perform(GOAPState currentState)
    {
        var container = currentState.CurrentTarget.GetComponent<ResourceContainer>();

        container.ExpandStorage();

        return EActionResult.Complete;
    }
}
