using System.Collections;
using UnityEngine;

namespace F3PS.Damage.Take
{
    public class Hittable : MonoBehaviour
    {
        protected Collider _collider;
        public MeshRenderer meshRenderer;
        public float damageMultiplier;
        public int hittableId;
        public float flashTimer;
        
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
