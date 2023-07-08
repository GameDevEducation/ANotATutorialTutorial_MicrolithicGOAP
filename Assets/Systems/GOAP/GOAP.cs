using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum EGOAPResult
{
    Unknown,
    InProgress,
    Failed,
    Complete
}

public class GOAPPlan
{
    public BaseGoal Goal { get; private set; }
    public List<BaseAction> Actions { get; private set; }
    public float Cost { get; private set; }
    public GOAPState DesiredState { get; private set; }

    int ActiveActionIndex = -1;
    BaseAction ActiveAction => ActiveActionIndex >= 0 ? Actions[ActiveActionIndex] : null;

    public GOAPPlan(BaseGoal goal, GOAPNode endNode)
    {
        Goal = goal;
        Cost = endNode.Cost;
        DesiredState = endNode.State;

        // build the action set
        GOAPNode currentNode = endNode;
        Actions = new List<BaseAction>();
        while (currentNode != null)
        {
            Actions.Insert(0, currentNode.Action);
            currentNode = currentNode.Parent;
        }
    }

    public bool IsValid(GOAPState currentState)
    {
        // check if the goal can no longer run
        if (!Goal.CanRun(currentState))
            return false;

        // check if any of the actions can no longer achieve the desired state
        foreach(var action in Actions)
        {
            if (!action.CanAchieve(DesiredState))
                return false;
        }

        return true;
    }

    public EGOAPResult Tick(GOAPState currentState)
    {
        if (ActiveActionIndex < 0)
        {
            ActiveActionIndex = 0;
            currentState.SetCurrentTarget(DesiredState.GetTargetForAction(ActiveAction));
        }

        EActionResult result = ActiveAction.Tick(currentState);

        if (result == EActionResult.Failed)
            return EGOAPResult.Failed;
        else if (result == EActionResult.InProgress)
            return EGOAPResult.InProgress;
        else if (result == EActionResult.Complete)
        {
            ++ActiveActionIndex;

            // no more actions to run?
            if (ActiveActionIndex >= Actions.Count)
                return EGOAPResult.Complete;

            currentState.SetCurrentTarget(DesiredState.GetTargetForAction(ActiveAction));
        }

        return EGOAPResult.Unknown;
    }

    public void ResetActions()
    {
        foreach(var action in Actions) 
        {
            action.Reset();
        }
    }
}

public class GOAPNode
{
    public GOAPNode Parent;

    public BaseAction Action;
    public GOAPState State;
    public float Cost;

    public GOAPNode(BaseAction _Action, GOAPState _State, GOAPNode _Parent)
    {
        Action  = _Action;
        State   = _State;
        Parent  = _Parent;
        Cost    = Action.GetCost(State) + (Parent != null ? Parent.Cost : 0f);
    }
}

public class GOAP : MonoBehaviour
{
    [SerializeField] UnityEvent<string> OnActiveGoalChanged = new();

    GOAPState CurrentState = new GOAPState();
    List<BaseGoal> Goals;
    List<BaseAction> Actions;
    GOAPPlan ActivePlan = null;

    void Awake()
    {
        Goals = new List<BaseGoal>(GetComponents<BaseGoal>());
        Actions = new List<BaseAction>(GetComponents<BaseAction>());
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        bool replanningNeeded = ActivePlan == null;

        // update the priority on the active goal if present
        if (ActivePlan != null)
            ActivePlan.Goal.RefreshPriority();

        // refresh the priority for all goals
        BaseGoal highestPriorityGoal = ActivePlan != null ? ActivePlan.Goal : null;
        foreach(var goal in Goals)
        {
            goal.RefreshPriority();

            if (!goal.CanRun(CurrentState))
                continue;

            if (highestPriorityGoal == null || goal.Priority > highestPriorityGoal.Priority)
                highestPriorityGoal = goal;
        }

        // most urgent goal changed => replan
        if (ActivePlan != null && highestPriorityGoal != ActivePlan.Goal)
            replanningNeeded = true;

        // may need to replan?
        if (replanningNeeded)
            Replan();

        // do we have an active plan?
        if (ActivePlan != null)
        {
            var result = ActivePlan.Tick(CurrentState);
            if (result == EGOAPResult.Failed || result == EGOAPResult.Complete)
                ActivePlan = null;
        }
    }

    public void SetFlag(EStateFlags flag)
    {
        CurrentState.SetFlag(flag);
    }

    public void ClearFlag(EStateFlags flag)
    {
        CurrentState.ClearFlag(flag);
    }

    public void SetLocation(Vector3 location)
    {
        CurrentState.SetLocation(location);
    }

    void Replan()
    {
        // clear the dirty flag
        CurrentState.ClearDirtyFlag();

        // generate potential plans for any goals that can run
        foreach(var goal in Goals)
        {
            // skip if not allowed to run
            if (!goal.CanRun(CurrentState))
                continue;

            // build a plan and skip if invalid
            var plan = BuildPlan(goal);
            if (plan == null)
                continue;

            // is this plan for a lower or same priority goal?
            if (ActivePlan != null && ActivePlan.Goal.Priority >= plan.Goal.Priority)
                continue;

            // is this plan for the same goal and not a better cost?
            if (ActivePlan != null && ActivePlan.Goal == plan.Goal && ActivePlan.Cost <= plan.Cost)
                continue;

            ActivePlan = plan;
        }

        if (ActivePlan != null)
        {
            ActivePlan.ResetActions();
            OnActiveGoalChanged.Invoke(ActivePlan.Goal.GetType().Name);
        }
    }

    GOAPPlan BuildPlan(BaseGoal goal)
    {
        // get the working and target states
        GOAPState workingState = CurrentState.Clone();
        GOAPState targetState = goal.GetDesiredState();

        // working and target are the same? nothing to do
        if (workingState == targetState)
            return null;

        // filter the actions to find ones that could achieve the target state
        List<BaseAction> terminalActions = IdentifyTerminalActions(workingState, targetState);
        if (terminalActions.Count == 0)
            return null;

        // seed the open list with all runnable actions
        List<GOAPNode> openList = new List<GOAPNode>();
        AddCandidateActions(openList, workingState, targetState);

        // find a plan
        while (openList.Count > 0)
        {
            // find the best node
            GOAPNode bestNode = null;
            foreach(var node in openList)
            {
                if (bestNode == null || node.Cost < bestNode.Cost)
                    bestNode = node;
            }

            // does this achieve the target state?
            if (bestNode.State.Achieves(targetState))
            {
                // Build the plan
                return new GOAPPlan(goal, bestNode);
            }

            // remove the node as we have the chain of parents
            openList.Remove(bestNode);

            AddCandidateActions(openList, bestNode.State, targetState, bestNode);
        }

        return null;
    }

    List<BaseAction> IdentifyTerminalActions(GOAPState currentState, GOAPState targetState)
    {
        // find any actions that can achieve the goal
        List<BaseAction> actions = new List<BaseAction>();
        foreach(var action in Actions)
        {
            if (action.CanAchieve(targetState))
                actions.Add(action);
        }

        return actions;
    }

    void AddCandidateActions(List<GOAPNode> openList, GOAPState workingState, GOAPState targetState, GOAPNode parent = null)
    {
        foreach (var action in Actions)
        {
            // ignore if not permitted to run in this state
            if (!action.CanRun(workingState))
                continue;

            // ignore if the parent of this action
            if (parent != null && parent.Action == action)
                continue;

            GOAPState updatedState = action.CalculateState(workingState, targetState);

            // state is invalid
            if (updatedState == null)
                continue;

            // if the action had no effect ignore it (prevents adding the same action twice without needing another list)
            if (!updatedState.IsDirty)
                continue;

            openList.Add(new GOAPNode(action, updatedState, parent));
        }
    }
}
