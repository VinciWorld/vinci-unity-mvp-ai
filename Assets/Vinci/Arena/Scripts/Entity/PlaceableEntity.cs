using StatSystem;
using UnityEngine;
using Vinci.Core.BattleFramework;

public enum PlaceableType
{
    Unit,
    Obstacle,
    Building,
    Spell,
    Tower,
}


public class PlaceableEntity : MonoBehaviour
{
    protected PlaceableType tpye;

    [HideInInspector]
    public Targetable targetable;
    protected StatController statController;

    //public event System.Action<DamageableObject, float, Vector3> OnDied;

    protected virtual void Awake()
    {
        targetable = GetComponent<Targetable>();
        statController = GetComponent<StatController>();

        if (statController != null)
        {

            Attribute _health = statController.GetAttribute(StatType.HEALTH);
            //Debug.Log("STATS Controler: " + _health);
            targetable.Health = _health;
        }
    }

    public virtual void Reset()
    {
        statController.ResetAttributes();
    }
}

