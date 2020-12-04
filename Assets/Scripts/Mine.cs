using UnityEngine;

public class Mine : Trap
{
    [SerializeField] GameObject _explosionEffect;

    protected override void FixedUpdate()
    {
        if (!_targetIn || _target == null)
            return;

        _target.AffectHealth(-_damage);
        Destroy(Instantiate(_explosionEffect, transform.position + new Vector3(0f, 2f, 0f), Quaternion.identity), 1f);
        Destroy(gameObject);
    }
}
