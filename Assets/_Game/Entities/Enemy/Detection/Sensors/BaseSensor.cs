using System.Collections.Generic;
using F3PS.Damage.Take;
using UnityEngine;

namespace F3PS.AI.Sensors
{
    public class BaseSensor : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MeshRenderer _meshRenderer;
     
        [Header("General Watchers (can initially be null)")] 
        [SerializeField] private List<Hittable> _targetCandidates;
        private readonly Color _defaultColor = new Color(1,1,1,0.1f);
        private readonly Color _searchingColor = new Color(0.5f,0,1f,0.1f);
        private readonly Color _aggressiveColor = new Color(1,0,0,0.15f);
        
        public bool HasTarget => _targetCandidates.Count > 0;
        public List<Hittable> TargetCandidates => _targetCandidates;
        public Hittable SelectedTarget => TargetCandidates.Count > 0 ? TargetCandidates[0] : null;

        private void OnTriggerEnter(Collider other)
        {
            var hittable = other.GetComponent<PlayerHittable>();
            if (!hittable) return;
            
            // AddHittableCandidate(hittable)
            _targetCandidates.Add(hittable);
            _targetCandidates.Sort((x, y) =>
            {
                if (x.damageMultiplier > y.damageMultiplier) return -1;
                if (x.damageMultiplier < y.damageMultiplier) return 1;
                return 0;
            });
        }

        private void OnTriggerExit(Collider other)
        {
            var hittable = other.GetComponent<PlayerHittable>();
            if (!hittable) return;

            // RemoveHittableCandidate(hittable);
            _targetCandidates.Remove(hittable);
        }

        public void SetActive(bool active)
        {
            _meshRenderer.enabled = active;
        }
        
        public void SetSensorState(SensorState state)
        {
            switch (state)
            {
                case SensorState.IDLE:
                    _meshRenderer.material.color = _defaultColor;
                    return;
                case SensorState.SEARCHING:
                    _meshRenderer.material.color = _searchingColor;
                    return;
                case SensorState.AGGRESSIVE:
                    _meshRenderer.material.color = _aggressiveColor;
                    return;
            }
        }
    }
}