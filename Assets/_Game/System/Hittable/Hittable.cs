using UnityEngine;

namespace F3PS.Damage.Take
{
    public class Hittable : MonoBehaviour
    {
        protected Collider _collider;
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

        protected virtual void OnHit(HitBox hitBy) {}
        
        private void OnCollisionEnter(Collision other)
        {
            var hitBox = other.gameObject.GetComponent<HitBox>();
            if (hitBox == null || hitBox.attackerId == hittableId) return;
            
            OnHit(hitBox);
        }
    }
}
