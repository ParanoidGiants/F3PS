using System;
using UnityEngine;

namespace Enemy.States
{
    [Serializable]
    public abstract class State
    {
        protected string name;
        protected BaseEnemy enemy;
        protected EnemyStateManager stateManager;
        
        [Header("Base Settings")]
        public float enemySpeed;
        public float enemyStoppingDistance;
        public Material material;
        
        public string Name => name;
        
        public virtual void Update() {}
        public virtual void OnExit() {}
        
        public virtual void Initialize(BaseEnemy enemy, EnemyStateManager stateManager, string name)
        {
            this.name = name;
            this.enemy = enemy;
            this.stateManager = stateManager;
        }
        
        public virtual void OnEnter()
        {
            enemy.navMeshAgent.speed = enemySpeed;
            enemy.navMeshAgent.stoppingDistance = enemyStoppingDistance;
            enemy.headMeshRenderer.sharedMaterial = material;
        }
        
        public void AggressiveSwitchWhenTargetIsInSight()
        {
            if (stateManager.isTargetInSight)
            {
                stateManager.aggressive.SetTarget(stateManager.target);
                stateManager.SwitchState(stateManager.aggressive);
            }
        }
    }
}
