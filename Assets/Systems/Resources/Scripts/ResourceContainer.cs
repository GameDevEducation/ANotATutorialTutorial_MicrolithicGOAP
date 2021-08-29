using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceContainer : MonoBehaviour
{
    [SerializeField] Resources.EType Type;
    [SerializeField] Transform ScaledMesh;
    [SerializeField] float MinScale = 0.1f;
    [SerializeField] float MaxScale = 3f;

    [SerializeField] float AmountStored = 0f;
    [SerializeField] float MaxCapacity = 1000f;

    public Resources.EType ResourceType => Type;
    public bool CanStore => AmountStored < MaxCapacity;

    // Start is called before the first frame update
    void Start()
    {
        UpdateMesh();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StoreResource(float amount)
    {
        AmountStored = Mathf.Min(AmountStored + amount, MaxCapacity);

        UpdateMesh();
    }

    public void RetrieveResource(float amount)
    {
        AmountStored = Mathf.Max(AmountStored - amount, 0f);

        UpdateMesh();
    }

    void UpdateMesh()
    {
        ScaledMesh.localScale = new Vector3(1f, Mathf.Lerp(MinScale, MaxScale, AmountStored / MaxCapacity), 1f);
    }
}
