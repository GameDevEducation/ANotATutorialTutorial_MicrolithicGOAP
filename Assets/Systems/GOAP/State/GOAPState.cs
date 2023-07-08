using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum EStateFlags : System.UInt32
{
    None                = 0,

    // Character states (may last multiple frames)
    Holding_Food        = 0x00000001,
    Holding_Water       = 0x00000002,
    Holding_Wood        = 0x00000004,
    IsHungry            = 0x00000008,
    IsThirsty           = 0x00000010,

    // Global states (may last multiple frames)
    ExpansionNeeded     = 0x00000100,
    RestockNeeded_Food  = 0x00000200,
    RestockNeeded_Water = 0x00000400,
    RestockNeeded_Wood  = 0x00000800,

    // Temporary states (only used in planning)
    Restocked_Food      = 0x00010000,
    Restocked_Water     = 0x00020000,
    Restocked_Wood      = 0x00040000,
    Expanded_Storage    = 0x00080000,
    Consumed_Food       = 0x00100000,
    Consumed_Water      = 0x00200000,
}

public class GOAPState : System.IEquatable<GOAPState>
{
    public EStateFlags Flags { get; private set; } = EStateFlags.None;
    public Vector3 Location { get; private set; } = Vector3.zero;
    public bool IsDirty { get; private set; } = false;
    public MonoBehaviour CurrentTarget { get; private set; } = null;

    private Dictionary<BaseAction, MonoBehaviour> ActionTargets = new();

    public GOAPState Clone()
    {
        GOAPState copy = new GOAPState();

        copy.Flags          = Flags;
        copy.Location       = Location;
        copy.IsDirty        = false;
        copy.CurrentTarget  = CurrentTarget;

        copy.ActionTargets = new Dictionary<BaseAction, MonoBehaviour>(ActionTargets);

        return copy;
    }

    public void ClearDirtyFlag()
    {
        IsDirty = false;
    }
    
    public bool GetFlag(EStateFlags flag)
    {
        return Flags.HasFlag(flag);
    }

    public void SetCurrentTarget(MonoBehaviour target)
    {
        CurrentTarget = target;
    }

    public void SetTargetForAction(MonoBehaviour target, BaseAction action)
    {
        ActionTargets[action] = target;
    }

    public MonoBehaviour GetTargetForAction(BaseAction action)
    {
        MonoBehaviour foundTarget = null;

        ActionTargets.TryGetValue(action, out foundTarget);

        return foundTarget;
    }

    public void SetFlag(EStateFlags flag)
    {
        if (!Flags.HasFlag(flag))
            IsDirty = true;

        Flags |= flag;
    }

    public void ClearFlag(EStateFlags flag)
    {
        if (Flags.HasFlag(flag))
            IsDirty = true;

        Flags &= ~flag;
    }

    public void SetLocation(Vector3 location)
    {
        if (location != Location)
            IsDirty = true;

        Location = location;
    }

    public bool Achieves(GOAPState other)
    {
        return (Flags & other.Flags) == other.Flags;
    }

    public override bool Equals(object obj) => this.Equals(obj as GOAPState);

    public bool Equals(GOAPState other)
    {
        if (other is null)
            return false;

        // Optimization for a common success case.
        if (Object.ReferenceEquals(this, other))
            return true;

        // If run-time types are not exactly the same, return false.
        if (this.GetType() != other.GetType())
            return false;

        return Flags == other.Flags;
    }

    public override int GetHashCode() => Flags.GetHashCode();

    public static bool operator == (GOAPState lhs, GOAPState rhs)
    {
        if (lhs is null)
        {
            if (rhs is null)
                return true;

            return false;
        }

        return lhs.Equals(rhs);
    }

    public static bool operator != (GOAPState lhs, GOAPState rhs) => !(lhs == rhs);
}
