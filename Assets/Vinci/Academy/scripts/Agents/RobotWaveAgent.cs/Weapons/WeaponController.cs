using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Vinci.Core.BattleFramework;

[RequireComponent(typeof(AudioSource))]
public class WeaponController : MonoBehaviour
{
    public WeaponBase _currentWeapon;
    [SerializeField]
    private WeaponBase _startingWeapom;

    [SerializeField]
    private Transform _weaponSocket;

    private List<WeaponBase> _weapons = new List<WeaponBase>();

    private void Start()
    {
        if (_currentWeapon == null)
        {
            AddWeapon(_startingWeapom);
        }
    }

    public void Shoot(Targetable owner, float damageFactor = 0f)
    {
        _currentWeapon?.Attack(owner, damageFactor);
    }

    public void AddWeapon(WeaponBase weapon)
    {
        if (_currentWeapon != null)
        {
            Destroy(_currentWeapon.gameObject);
        }

        Debug.Log("ADD WEAPON: " + _currentWeapon);

        _currentWeapon = Instantiate(weapon, _weaponSocket.position, Quaternion.Euler(-90f, 90f, 0));
        _currentWeapon.transform.parent = _weaponSocket;
        _weapons.Add(_currentWeapon);
    }
}
