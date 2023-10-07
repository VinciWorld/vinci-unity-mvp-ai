using UnityEngine;

public enum WeaponType
{
    Range,
    Melee,
    body
}

[System.Serializable]
[CreateAssetMenu(fileName = "Weapon", menuName = "Agent/Weapon", order = 1)]
public class WeaponData : ScriptableObject
{
    public WeaponType type;

    [Header("Info")]
    public string Name;
    public Sprite image;
    public string description;

    [Header("Common parms")]
    public float range;
    public float startSpeed;
    public float speed;
    public int Damage;
    public float attackRate;
    public AudioClip shotShound;
    public AudioClip pickupSound;

    [Header("Range parms")]
    public int recoilForce;

    public int MagazineSize;
    public GameObject weaponPrefab;
    public ProjectileTopDown projectilePrefab;


}