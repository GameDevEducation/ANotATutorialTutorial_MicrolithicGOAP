using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseGoal : MonoBehaviour
{
    public const int MaxPriority = 100;

    [SerializeField] public int _Priority = 0;

    protected CharacterBase LinkedCharacter;

    public int Priority
    {
        get
        {
            return _Priority;
        }
        protected set
        {
            _Priority = value;
        }
    }

    public abstract bool CanRun(GOAPState currentState);
    public abstract void RefreshPriority();
    public abstract GOAPState GetDesiredState();

    protected void Start()
    {
        LinkedCharacter = GetComponent<CharacterBase>();
    }
}
