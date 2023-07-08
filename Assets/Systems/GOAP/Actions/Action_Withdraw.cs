using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_Withdraw : BaseAction
{
    [SerializeField] float MinContainerAmountNeeded = 10f;

    public override bool CanAchieve(GOAPState targetState)
    {
        if (targetState.GetFlag(EStateFlags.Holding_Food) ||
            targetState.GetFlag(EStateFlags.Holding_Water) ||
            targetState.GetFlag(EStateFlags.Holding_Wood))
            return true;

        return false;
    }

    public override bool CanRun(GOAPState currentState)
    {
        return true;
    }

    public override GOAPState CalculateState(GOAPState currentState, GOAPState targetState)
    {
        var newState = currentState.Clone();

        if (targetState.GetFlag(EStateFlags.Holding_Food) || targetState.GetFlag(EStateFlags.Consumed_Food) || targetState.GetFlag(EStateFlags.Restocked_Food))
        {
            newState.SetFlag(EStateFlags.Holding_Food);
            newState.SetCurrentTarget(ResScanner.FindNearestContainerOfType(Resources.EType.Food, MinContainerAmountNeeded));
        }
        else if (targetState.GetFlag(EStateFlags.Holding_Water) || targetState.GetFlag(EStateFlags.Consumed_Water) || targetState.GetFlag(EStateFlags.Restocked_Water))
        {
            newState.SetFlag(EStateFlags.Holding_Water);
            newState.SetCurrentTarget(ResScanner.FindNearestContainerOfType(Resources.EType.Water, MinContainerAmountNeeded));
        }
        else if (targetState.GetFlag(EStateFlags.Holding_Wood) || targetState.GetFlag(EStateFlags.Restocked_Wood))
        {
            newState.SetFlag(EStateFlags.Holding_Wood);
            newState.SetCurrentTarget(ResScanner.FindNearestContainerOfType(Resources.EType.Wood, MinContainerAmountNeeded));
        }

        ResourceContainer foundContainer = null;
        if (newState.CurrentTarget == null || !newState.CurrentTarget.TryGetComponent<ResourceContainer>(out foundContainer))
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

        float amountGathered = container.RetrieveResource(Agent.GetRemainingCarryCapacity(container.ResourceType));
        Agent.AddAmountStored(container.ResourceType, amountGathered);

        return EActionResult.Complete;
    }
}
