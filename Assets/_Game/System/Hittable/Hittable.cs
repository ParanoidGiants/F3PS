using System.Collections;
using UnityEngine;

namespace F3PS.Damage.Take
{
    public class Hittable : MonoBehaviour
    {
        [Header("References")]
        public HittableFlash hittableFlash;

        [Header("Settings")]
        public float damageMultiplier;
        
        protected Collider _collider;
        protected int _hittableId;

        public int HittableId => _hittableId;
        
        public Vector3 Center()
        {
            return _collider.bounds.center;
        }

        public virtual void OnHit(HitBox hitBy) { }
    }
}
