using System.Collections.Generic;
using UnityEngine;

class Hitbox
{
    public Collider collider;
    public float damage;
    public float knockback;
}

public class AttackSystem : MonoBehaviour
{

    [SerializeField] List<Hitbox> _hitboxes;

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
