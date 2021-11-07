using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CharacterBase : MonoBehaviour
{
    [SerializeField] EFaction _Faction;

    public EFaction Faction => _Faction;

    [SerializeField] float CurrentFood = 50f;
    [SerializeField] float MaxFood = 100f;
    [SerializeField] float FoodDepletionRate = 0.5f;

    [SerializeField] float CurrentWater = 50f;
    [SerializeField] float MaxWater = 100f;
    [SerializeField] float WaterDepletionRate = 1f;

    protected virtual void Update()
    {
        // update stats
        CurrentFood = Mathf.Clamp(CurrentFood - FoodDepletionRate * Time.deltaTime, 0, MaxFood);
        CurrentWater = Mathf.Clamp(CurrentWater - WaterDepletionRate * Time.deltaTime, 0, MaxWater);
    }
}
