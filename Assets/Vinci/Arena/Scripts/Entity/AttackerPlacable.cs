using UnityEngine;
using Vinci.Core.BattleFramework;

public class AttackerPlacable : PlaceableEntity
{
    protected DetectionModule _detectionModule;
    protected Animator _animator;

    [SerializeField]
    protected WeaponBase weapon;
    private bool _canShoot;

    protected override void Awake()
    {
        base.Awake();

        _animator = GetComponentInChildren<Animator>();
        _detectionModule = GetComponent<DetectionModule>();
        weapon = GetComponentInChildren<WeaponBase>();
    }

    protected virtual void Start()
    {
        targetable.Health.currentValueChanged += OnHealthChanged;
    }

    public virtual void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    protected void TryAtack(Targetable target, NavMeshMovement _navMeshAgent)
    {
        weapon.Attack(targetable, 0, _navMeshAgent, target);
    }

    public override void Reset()
    {
        base.Reset();
    }

    private void OnHealthChanged(float currentHealth, float maxHealth)
    {
        //Debug.Log("health: " + currentHealth);
    }
}