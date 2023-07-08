using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CharacterBase : MonoBehaviour
{
    [SerializeField] EFaction _Faction;

    public EFaction Faction => _Faction;

    [SerializeField] float _CurrentFood = 50f;
    [SerializeField] float _MaxFood = 100f;
    [SerializeField] float _FoodDepletionRate = 0.5f;
    [SerializeField] float _HungryLevel = 20f;

    [SerializeField] float _CurrentWater = 50f;
    [SerializeField] float _MaxWater = 100f;
    [SerializeField] float _WaterDepletionRate = 1f;
    [SerializeField] float _ThirstyLevel = 20f;

    [SerializeField] float MaxCapacityPerResource = 50f;

    GOAP Brain;

    public float FoodPercent => _CurrentFood / _MaxFood;
    public float WaterPercent => _CurrentWater / _MaxWater;

    Dictionary<Resources.EType, float> AmountCarried = new Dictionary<Resources.EType, float>();

    protected virtual void Start()
    {
        foreach(var value in System.Enum.GetValues(typeof(Resources.EType)))
        {
            AmountCarried[(Resources.EType)value] = 0f;
        }

        Brain = GetComponent<GOAP>();
    }

    protected virtual void Update()
    {
        // update stats
        _CurrentFood = Mathf.Clamp(_CurrentFood - _FoodDepletionRate * Time.deltaTime, 0, _MaxFood);
        _CurrentWater = Mathf.Clamp(_CurrentWater - _WaterDepletionRate * Time.deltaTime, 0, _MaxWater);

        if (_CurrentFood < _HungryLevel)
            Brain.SetFlag(EStateFlags.IsHungry);
        else
            Brain.ClearFlag(EStateFlags.IsHungry);

        if (_CurrentWater < _ThirstyLevel)
            Brain.SetFlag(EStateFlags.IsThirsty);
        else
            Brain.ClearFlag(EStateFlags.IsThirsty);
    }

    protected virtual void LateUpdate()
    {
        Brain.SetLocation(transform.position);
    }

    public float GetRemainingCarryCapacity(Resources.EType resourceType)
    {
        if (!AmountCarried.ContainsKey(resourceType))
            AmountCarried[resourceType] = 0f;

        return MaxCapacityPerResource - AmountCarried[resourceType];
    }

    public float GetAmountCarried(Resources.EType resourceType)
    {
        if (!AmountCarried.ContainsKey(resourceType))
            AmountCarried[resourceType] = 0f;

        return AmountCarried[resourceType];
    }

    public void AddAmountStored(Resources.EType resourceType, float amountGathered)
    {
        AmountCarried[resourceType] = AmountCarried[resourceType] + amountGathered;

        if (AmountCarried[Resources.EType.Food] > 0)
            Brain.SetFlag(EStateFlags.Holding_Food);
        else
            Brain.ClearFlag(EStateFlags.Holding_Food);
        if (AmountCarried[Resources.EType.Water] > 0)
            Brain.SetFlag(EStateFlags.Holding_Water);
        else
            Brain.ClearFlag(EStateFlags.Holding_Water);
    }

    public void Consume(Resources.EType resourceType)
    {
        if (!AmountCarried.ContainsKey(resourceType) || AmountCarried[resourceType] == 0f)
            return;

        if (resourceType == Resources.EType.Food)
            _CurrentFood = Mathf.Min(_MaxFood, _CurrentFood + AmountCarried[Resources.EType.Food]);
        else
            _CurrentWater = Mathf.Min(_MaxWater, _CurrentWater + AmountCarried[Resources.EType.Water]);

        AmountCarried[resourceType] = 0f;

        if (AmountCarried[Resources.EType.Food] > 0)
            Brain.SetFlag(EStateFlags.Holding_Food);
        else
            Brain.ClearFlag(EStateFlags.Holding_Food);
        if (AmountCarried[Resources.EType.Water] > 0)
            Brain.SetFlag(EStateFlags.Holding_Water);
        else
            Brain.ClearFlag(EStateFlags.Holding_Water);        
    }
}
