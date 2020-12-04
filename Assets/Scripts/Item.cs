using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Item : MonoBehaviour
{
    [Header("Stats to add")]
    [SerializeField] private int hpModifier;
    [SerializeField] private int armorModifier;
    [SerializeField] private int mpModifier;
    private bool _used;

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (_used)
            return;

        var health = collider.GetComponent<Health>();
        var player = collider.GetComponent<PlayerController>();

        if(health != null)
        {
            if(hpModifier != 0)
                health.AffectHealth(hpModifier);

            if(armorModifier != 0)
                health.AffectArmor(armorModifier);
        }
        if(player != null)
        {
            if(mpModifier != 0)
                player.ChangeMP(mpModifier);
        }

        _used = true;
        Destroy(gameObject);
    }
}
