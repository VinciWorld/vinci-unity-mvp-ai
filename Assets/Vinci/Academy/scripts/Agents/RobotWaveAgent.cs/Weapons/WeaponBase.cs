using System.Collections;
using UnityEngine;
using Vinci.Core.BattleFramework;

public class WeaponBase : MonoBehaviour
{
    [SerializeField]
    private WeaponData _weaponData;
    [SerializeField]
    private Transform _muzzleTransform;

    private AudioSource _audioSource;
    private float _timer;

    public WeaponData WeaponData { get => _weaponData; set => _weaponData = value; }


    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void Attack(Targetable owner, float damageFactor = 0f,
     NavMeshMovement _navMeshAgent = null, Transform target = null)
    {
        if (WeaponData.type == WeaponType.body)
        {
            StartCoroutine(AttackSpecial(target, owner, _navMeshAgent, damageFactor));
        }
        else if (WeaponData.type == WeaponType.Range)
        {
            StartCoroutine(AttackAtEndOfFrame(owner, damageFactor));
        }
    }

    public int CalculateDamage(float damageFactor)
    {
        return Mathf.RoundToInt(WeaponData.Damage + WeaponData.Damage * damageFactor);
    }

    public IEnumerator AttackSpecial(Transform target, Targetable owner,
     NavMeshMovement _navMeshAgent, float damageFactor = 0f)
    {
        _navMeshAgent.SetPosition(owner.position);
        _navMeshAgent.enabled = false;
        Vector3 originalPos = transform.position;
        Vector3 attackPosition = target.position;

        if (WeaponData.attackRate + _timer <= Time.time)
        {
            _timer = Time.time;

            float percent = 0;

            while (percent <= 1)
            {
                //Debug.Log("percent");
                percent += Time.deltaTime * WeaponData.speed;
                float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
                transform.position = Vector3.Lerp(originalPos, attackPosition, interpolation);

                yield return null;
            }
            _navMeshAgent.enabled = true;

            Targetable targetable = target.GetComponent<Targetable>();

            targetable.takeDamage(_weaponData.Damage, attackPosition, Vector3.zero, owner);

            if (WeaponData.shotShound != null)
            {
                _audioSource.PlayOneShot(WeaponData.shotShound);
            }
        }
    }

    IEnumerator AttackAtEndOfFrame(Targetable owner, float damageFactor)
    {
        yield return new WaitForEndOfFrame();

        if (WeaponData.attackRate + _timer <= Time.time)
        {
            _timer = Time.time;

            //CameraManager.instance.SendImpulse();

            ProjectileTopDown projectile = Instantiate(
                WeaponData.projectilePrefab,
                _muzzleTransform.position,
                _muzzleTransform.rotation
            );

            int damage = CalculateDamage(damageFactor);

            projectile.owner = owner;
            projectile.damage = damage;
            projectile.Speed = WeaponData.speed;


            if (WeaponData.shotShound != null)
            {
                _audioSource.PlayOneShot(WeaponData.shotShound);
            }
        }
    }

    public float GetWeaponRange()
    {
        return WeaponData.range;
    }
}