using System;

namespace Enemy.States
{
    public class Rush : Attack
    {
        public float rushStrength;
        public BaseEnemy enemy;
        public bool wasEarlyHit;

        private void Start()
        {
            enemy = navMeshAgent.GetComponent<BaseEnemy>();
        }

        private void EarlyHit()
        {
            OnRecover();
            wasEarlyHit = true;
        }

        override
        protected void OnHit()
        {
            wasEarlyHit = false;
            base.OnHit();
            enemy.Rush(rushStrength, damage, () => EarlyHit());
        }

        override
        protected void HandleHitting()
        {
            if (wasEarlyHit)
            {
                hitTime = hitTimer;
                return;
            }
            base.HandleHitting();
        }

        override
        protected void OnRecover()
        {
            base.OnRecover();
            enemy.StopRush();
        }
    }
}
