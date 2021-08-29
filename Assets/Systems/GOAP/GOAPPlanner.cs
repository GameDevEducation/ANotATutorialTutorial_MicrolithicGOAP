using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GOAPPlanner : MonoBehaviour
{
    Goal_Base[] Goals;
    Action_Base[] Actions;

    Goal_Base ActiveGoal;
    Action_Base ActiveAction;

    void Awake()
    {
        Goals = GetComponents<Goal_Base>();
        Actions = GetComponents<Action_Base>();
    }

    void Update()
    {

    }
}
