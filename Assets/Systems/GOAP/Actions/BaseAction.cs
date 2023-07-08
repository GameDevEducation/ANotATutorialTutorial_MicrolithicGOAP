using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EActionResult
{
    Unknown,
    InProgress,
    Failed,
    Complete
}

public abstract class BaseAction : MonoBehaviour
{
    protected enum EActionStage
    {
        MovingToStart,
        Performing
    }

    [SerializeField] protected float BaseCost = 10f;
    [SerializeField] protected float CostPerDistance = 0.1f;
    
    EActionStage Stage = EActionStage.MovingToStart;
    protected ResourceScanner ResScanner;
    protected CharacterAgent Agent;

    public abstract bool CanAchieve(GOAPState targetState);
    public abstract bool CanRun(GOAPState currentState);
    public abstract GOAPState CalculateState(GOAPState currentState, GOAPState targetState);
    public abstract float GetCost(GOAPState currentState);

    public EActionResult Tick(GOAPState currentState)
    {
        if (Stage == EActionStage.MovingToStart)
        {
            EActionResult result = Tick_MoveIntoPosition(currentState);
            if (result == EActionResult.Complete)
            {
                Stage = EActionStage.Performing;
                return EActionResult.InProgress;
            }

            return result;
        }
        else if (Stage == EActionStage.Performing)
            return Tick_Perform(currentState);

        return EActionResult.InProgress;
    }

    protected abstract EActionResult Tick_MoveIntoPosition(GOAPState currentState);
    protected abstract EActionResult Tick_Perform(GOAPState currentState);

    public virtual void Reset()
    {
        Stage = EActionStage.MovingToStart;
    }

    public void Awake()
    {
        ResScanner = GetComponent<ResourceScanner>();
        Agent = GetComponent<CharacterAgent>();
    }
}
