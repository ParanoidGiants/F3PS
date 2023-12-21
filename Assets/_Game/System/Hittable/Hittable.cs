using System.Collections;
using UnityEngine;

namespace F3PS.Damage.Take
{
    public class Hittable : MonoBehaviour
    {
        [SerializeField] protected Collider _collider;
        public float damageMultiplier;
        public int hittableId;
        
        void Awake()
        {
            _collider = GetComponent<Collider>();
        }
        
        public Vector3 Center()
        {
            return _collider.bounds.center;
        }

        public virtual void OnHit(HitBox hitBy) { }
    }
}
