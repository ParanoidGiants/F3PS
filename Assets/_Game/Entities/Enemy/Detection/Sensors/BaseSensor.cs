using System.Collections.Generic;
using F3PS.Damage.Take;
using UnityEngine;

namespace F3PS.AI.Sensors
{
    public class BaseSensor : MonoBehaviour
    {
        [Header("General Watchers (can initially be null)")]
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private List<Hittable> _targetCandidates;
        private readonly Color _defaultColor = new Color(1,1,1,0.1f);
        private readonly Color _searchingColor = new Color(0.5f,0,1f,0.1f);
        private readonly Color _aggressiveColor = new Color(1,0,0,0.15f);
        
        public bool HasTarget => _targetCandidates.Count > 0;
        public Hittable SelectedTarget { get; private set; }
        
        private void Awake()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
        }

        private void OnTriggerEnter(Collider other)
        {
            var hittable = other.GetComponent<PlayerHittable>();
            if (!hittable) return;
            
            // AddHittableCandidate(hittable)
            _targetCandidates.Add(hittable);
            SelectedTarget = SelectBestTarget();
        }

        private void OnTriggerExit(Collider other)
        {
            var hittable = other.GetComponent<PlayerHittable>();
            if (!hittable) return;
            
            // RemoveHittableCandidate(hittable);
            _targetCandidates.Remove(hittable);
            SelectedTarget = SelectBestTarget();
        }

        private Hittable SelectBestTarget()
        {
            if (_targetCandidates.Count == 0)
            {
                return null;
            }

            Hittable bestTarget = _targetCandidates[0];
            for (int i = 1; i < _targetCandidates.Count; i++)
            {
                if (_targetCandidates[i].damageMultiplier > SelectedTarget.damageMultiplier)
                {
                    bestTarget = _targetCandidates[i];
                }
            }
            return bestTarget;
        }
        
        public void SetActive(bool active)
        {
            _meshRenderer.enabled = active;
        }
        
        public void SetAggressive(SensorState state)
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