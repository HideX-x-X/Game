using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class ProjectileBase : MonoBehaviour
{
    [SerializeField] private int _damage;
    [SerializeField] private Vector2 _velocity;
    protected Rigidbody2D _rigidbody2D;
    private bool disabled;
    public bool Flip { get; set; }

    protected void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        if(Flip)
        {
            _velocity = new Vector2(-_velocity.x, _velocity.y);
            transform.Rotate(new Vector3(0f, 180f, 0f));
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D collider)
    {
        if (disabled)
            return;

        var health = collider.gameObject.GetComponent<Health>();
        if (health != null)
        {
            health.AffectHealth(-_damage);
        }

        disabled = true;
        Destroy(gameObject);
    }

    protected void FixedUpdate()
    {
        _rigidbody2D.velocity = _velocity;
    }
}
