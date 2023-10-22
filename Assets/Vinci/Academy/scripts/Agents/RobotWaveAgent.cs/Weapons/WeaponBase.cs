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

    public bool canShoot = true;

    public WeaponData WeaponData { get => _weaponData; set => _weaponData = value; }


    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    void FixedUpdate()
    {
        if (_weaponData.attackRate + _timer <= Time.time && canShoot == false)
        {
            _timer = Time.time;
            canShoot = true;
        }
    }

    public bool Attack(Targetable owner, float damageFactor = 0f,
     NavMeshMovement _navMeshAgent = null, Targetable target = null)
    {
        if (WeaponData.attackRate + _timer <= Time.time)
        {
            _timer = Time.time;

            if (WeaponData.type == WeaponType.body)
            {
                StartCoroutine(AttackSpecial(target, owner, _navMeshAgent, damageFactor));
            }
            else if (WeaponData.type == WeaponType.Range)
            {
                StartCoroutine(AttackAtEndOfFrame(owner, damageFactor));
            }

            return true;
        }
        else
        {
         
            return false;
        }
    }

    public int CalculateDamage(float damageFactor)
    {
        return Mathf.RoundToInt(WeaponData.Damage + WeaponData.Damage * damageFactor);
    }

    public IEnumerator AttackSpecial(Targetable targetable, Targetable owner,
     NavMeshMovement _navMeshAgent, float damageFactor = 0f)
    {
        _navMeshAgent.SetPosition(owner.position);
        _navMeshAgent.enabled = false;
        Vector3 originalPos = _muzzleTransform.position;
        Vector3 attackPosition = targetable.position;

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

        targetable.takeDamage(_weaponData.Damage, attackPosition, Vector3.zero, owner);

        if (WeaponData.shotShound != null)
        {
            _audioSource.PlayOneShot(WeaponData.shotShound);
        }
        yield return null;
        transform.position = _muzzleTransform.position;
    }

    IEnumerator AttackAtEndOfFrame(Targetable owner, float damageFactor)
    {
        yield return new WaitForEndOfFrame();

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

    public float GetWeaponRange()
    {
        return WeaponData.range;
    }
}