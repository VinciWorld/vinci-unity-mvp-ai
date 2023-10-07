using UnityEngine;
using Vinci.Core.BattleFramework;

public class ProjectileTopDown : MonoBehaviour 
{
    [SerializeField]
    LayerMask colllisionMask;

    private float _speed = 20f;
    public float Speed {get { return _speed; } set{ _speed = value; } }

    public float bulletDuration = 10f;
    private float _destroyTimer = 1f;

    public Targetable owner;

    //VFX
    public GameObject hittVfx;
    public GameObject muzzleFlashVfx;

    public int damage { get; set; }

    void Start()
    {
        _destroyTimer = bulletDuration;
        GameObject muzzleVfx = Instantiate(muzzleFlashVfx, transform.position, transform.rotation);
        muzzleVfx.AddComponent<ParticleDestroy>();
    }

    void Update()
    {
        _destroyTimer -= Time.deltaTime;
        
        if(_destroyTimer <= 0)
        {
            _destroyTimer = bulletDuration;
            Destroy(gameObject);
        }
    }

    void FixedUpdate()
    {
        float moveDistance = _speed * Time.fixedDeltaTime;
        transform.Translate(Vector3.forward * moveDistance);
        DetectColision(moveDistance);
    }

    private void OnCollisionEnter(Collision other)
    {
        /*
        Targetable otherTargatable = other.gameObject.GetComponent<Targetable>();

        if (otherTargatable != null)
        {
            if(otherTargatable.team != owner.team)
            {
                OnHit(otherTargatable, other);
            }
            else
            {
                otherTargatable.registerMiss();
            }
        }

        Destroy(gameObject);
        */
    }

    private void DetectColision(float moveDistance)
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, moveDistance, colllisionMask))
        {
            Targetable otherTargatable = hit.collider.gameObject.GetComponent<Targetable>();
            if (otherTargatable != null)
            {
                if (otherTargatable.team != owner.team)
                {
                    OnHit(otherTargatable, hit);
                }
            }
            else
            {
                owner.registerMiss();
            }

            GameObject hitVfx = Instantiate(hittVfx, transform.position, transform.rotation);
            hitVfx.AddComponent<ParticleDestroy>();
            GameObject.Destroy(this.gameObject);
        }
    }

    private void OnHit(Targetable opponnent, RaycastHit hit)
    {
        opponnent.takeDamage(
            damage, hit.point, hit.normal, owner);
    }

    private void OnHit(Targetable opponnent, Collision other)
    {

        opponnent.takeDamage(
            damage, other.contacts[0].point, other.contacts[0].normal, owner);
    }

}