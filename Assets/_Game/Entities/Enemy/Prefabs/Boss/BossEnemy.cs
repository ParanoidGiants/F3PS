using System.Collections.Generic;
using System.Linq;
using DarkTonic.MasterAudio;
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
        }
        
        private void Start()
        {
            Initialize();
            foreach (var minion in minions)
            {
                minion.Deactivate();
            }
        }
        
        override
        public void SetMaterial(Material material)
        {
            foreach (var minion in minions)
            {
                minion.SetMaterial(material);
            }
        }

        override
        public void Hit(int damage)
        {
            health -= damage;
            health = Mathf.Max(health, 0);
            Debug.Log("Boss Took " + damage + " damage");
            MasterAudio.PlaySound3DAtTransformAndForget("Hit", body.transform);
            _healthUIPool.OnHitBoss(this);
            if (health <= 0)
            {
                _healthUIPool.DisableBossUI();
                Destroy(gameObject);
            }
        }

        public void EnableHealthUI()
        {
            _healthUIPool.EnableBossUI();
        }
    }
}
