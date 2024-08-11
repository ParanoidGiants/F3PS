using System.Collections.Generic;
using System.Linq;
using DarkTonic.MasterAudio;
using F3PS.AI.States.Action;
using F3PS.Enemy.UI;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace F3PS.Enemy
{
    public class BossEnemy : BaseEnemy
    {
        [Header("References")]
        public bool rushersReleased;
        public Transform rusherLayer;
        public List<BaseEnemy> rushers;
        public GameObject rushersHittable;
        
        public bool shootersReleased;
        public Transform shooterLayer;
        public List<BaseEnemy> shooters;
        public GameObject shootersHittable;
        
        public Transform shieldShooterLayer;
        public List<BaseEnemy> shieldShooters;
        public GameObject shieldShootersHittable;
        
        public Transform center;
        private BossStateManager _bossStateManager;
        
        private new void Awake()
        {
            _healthUIPool = FindObjectOfType<EnemyHealthUIPool>();
            rushers = rusherLayer.GetComponentsInChildren<BaseEnemy>().ToList();
            shooters = shooterLayer.GetComponentsInChildren<BaseEnemy>().ToList();
            shieldShooters = shieldShooterLayer.GetComponentsInChildren<BaseEnemy>().ToList();
            _bossStateManager = (BossStateManager) _stateManager;
        }
        
        private void Start()
        {
            Initialize();
            rushersHittable.SetActive(true);
            shootersHittable.SetActive(false);
            shieldShootersHittable.SetActive(false);
            foreach (var rusher in rushers)
            {
                rusher.Deactivate();
            }
            foreach (var shooter in shooters)
            {
                shooter.Deactivate();
            }
            foreach (var shieldShooter in shieldShooters)
            {
                shieldShooter.Deactivate();
            }
        }
        
        override
        public void SetMaterial(Material material)
        {
            foreach (var shieldShooter in shieldShooters)
            {
                shieldShooter.SetMaterial(material);
            }
            
            if (shootersReleased) return;
            foreach (var shooter in shooters)
            {
                shooter.SetMaterial(material);
            }

            if (rushersReleased) return;
            foreach (var rusher in rushers)
            {
                rusher.SetMaterial(material);
            }
        }

        override
        public void Hit(int damage)
        {
            health -= damage;
            health = Mathf.Max(health, 0);
            // Debug.Log("Boss Took " + damage + " damage");
            MasterAudio.PlaySound3DAtTransformAndForget("Hit", body.transform);
            _healthUIPool.OnHitBoss(this);
            if (health <= maxHealth * 2f / 3f && !rushersReleased)
            {
                ReleaseRushers();
            }
            if (health <= maxHealth / 3f && !shootersReleased)
            {
                ReleaseShooters();
            }
            if (health <= 0)
            {
                ReleaseShieldShooters();
                Destroy(gameObject);
            }   
        }

        private void ReleaseRushers()
        {
            foreach (var rusher in rushers)
            {
                rusher.transform.parent = null;
                rusher.Activate();
            }
            rushers.Clear();
            _bossStateManager.SwitchAttack(AttackType.BOSS_SHOOT_SHOOTERS);
            rushersReleased = true;
            rushersHittable.SetActive(false);
            shootersHittable.SetActive(true);
            shieldShootersHittable.SetActive(false);
            shooterLayer.localPosition = new Vector3(
                0f,
                0f,
                0.5f
            );
            shieldShooterLayer.localPosition = new Vector3(
                0f,
                0f,
                -0.5f
            );
            timeObject = shootersHittable.GetComponent<AgentTimeObject>();
        }

        private void ReleaseShooters()
        {
            foreach (var shooter in shooters)
            {
                shooter.transform.parent = null;
                shooter.Activate();
            }
            shooters.Clear();
            _bossStateManager.SwitchAttack(AttackType.BOSS_SHOOT_SHIELD_SHOOTERS);
            shootersReleased = true;
            
            rushersHittable.SetActive(false);
            shootersHittable.SetActive(false);
            shieldShootersHittable.SetActive(true);
            foreach (var shieldShooter in shieldShooters)
            {
                shieldShooter.shield.SetActive(true);
            }
            
            shieldShooterLayer.localPosition = Vector3.zero;
            timeObject = shieldShootersHittable.GetComponent<AgentTimeObject>();
        }

        private void ReleaseShieldShooters()
        {
            foreach (var shieldShooter in shieldShooters)
            {
                shieldShooter.transform.parent = null;
                shieldShooter.Activate();
            }
            shieldShooters.Clear();
        }

        public void EnableHealthUI()
        {
            _healthUIPool.EnableBossUI();
        }

        public void DisableHealthUI()
        {
            _healthUIPool.DisableBossUI();
        }
    }
}
