using System.Collections.Generic;
using System.Linq;
using F3PS.Enemy.UI;
using Unity.VisualScripting;
using UnityEngine;

namespace F3PS.Enemy
{
    public class BossEnemy : BaseEnemy
    {
        [Header("References")]
        public List<BaseEnemy> minions;
        
        private new void Awake()
        {
            _healthUIPool = FindObjectOfType<EnemyHealthUIPool>();
            minions = GetComponentsInChildren<BaseEnemy>().ToList();
            minions.Remove(this);
            foreach (var minion in minions)
            {
                minion.Deactivate();
            }
            _stateManager.Initialize();
        }
        
        override
        public void SetMaterial(Material material)
        {
            foreach (var minion in minions)
            {
                minion.SetMaterial(material);
            }
        }

        private new void OnDisable()
        {
            
        }
    }
}
