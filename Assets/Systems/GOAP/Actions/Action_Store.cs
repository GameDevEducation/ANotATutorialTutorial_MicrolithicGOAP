using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_Store : BaseAction
{
    public override bool CanAchieve(GOAPState targetState)
    {
        if (targetState.GetFlag(EStateFlags.Restocked_Food) ||
            targetState.GetFlag(EStateFlags.Restocked_Water) ||
            targetState.GetFlag(EStateFlags.Restocked_Wood))
            return true;

        return false;
    }

    public override bool CanRun(GOAPState currentState)
    {
        return currentState.GetFlag(EStateFlags.Holding_Food) ||
               currentState.GetFlag(EStateFlags.Holding_Water) ||
               currentState.GetFlag(EStateFlags.Holding_Wood);
    }

    public override GOAPState CalculateState(GOAPState currentState, GOAPState targetState)
    {
        var newState = currentState.Clone();

        if (currentState.GetFlag(EStateFlags.Holding_Food))
        {
            newState.SetFlag(EStateFlags.Restocked_Food);
            newState.SetCurrentTarget(ResScanner.FindNearestContainerOfType(Resources.EType.Food));
        }
        else if (currentState.GetFlag(EStateFlags.Holding_Water))
        {
            newState.SetFlag(EStateFlags.Restocked_Water);
            newState.SetCurrentTarget(ResScanner.FindNearestContainerOfType(Resources.EType.Water));
        }
        else if (currentState.GetFlag(EStateFlags.Holding_Wood))
        {
            newState.SetFlag(EStateFlags.Restocked_Wood);
            newState.SetCurrentTarget(ResScanner.FindNearestContainerOfType(Resources.EType.Wood));
        }

        if (newState.CurrentTarget == null)
            return null;

        newState.SetTargetForAction(newState.CurrentTarget, this);

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

        if (currentState.GetFlag(EStateFlags.Holding_Water) && container.ResourceType == Resources.EType.Water)
            container.StoreResource(Agent.GetAmountCarried(Resources.EType.Water));
        else if (currentState.GetFlag(EStateFlags.Holding_Food) && container.ResourceType == Resources.EType.Food)
            container.StoreResource(Agent.GetAmountCarried(Resources.EType.Food));
        else if (currentState.GetFlag(EStateFlags.Holding_Wood) && container.ResourceType == Resources.EType.Wood)
            container.StoreResource(Agent.GetAmountCarried(Resources.EType.Wood));

        return EActionResult.Complete;
    }
}
