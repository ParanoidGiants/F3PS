using System;
using UnityEngine;

namespace Weapon
{
    public enum ThrowState
    {
        IDLE,
        AIM
    }
    
    public class ThrowTimeBubbleGrenade : MonoBehaviour
    {
        [Header("References")]
        public HittableManager hittableManager;
        public TimeBubbleGrenadeProjectile timeBubbleGrenadeProjectile;
        public LineRenderer throwLine;
        public Transform spawnTransform;
        
        [Space(10)]
        [Header("Settings")]
        public int lineResolution = 100;
        public float lineStepSize = 0.1f;
        public LayerMask whatCanCollide;
        
        [Space(10)]
        [Header("Watchers")]
        public bool isTimeBubbleActive;
        public ThrowState state = ThrowState.IDLE;
        public Vector3 throwDirection;
        public float throwPower;
        public WeaponUI weaponUI;

        private void Awake()
        {
            timeBubbleGrenadeProjectile.InitReferences();
            throwLine.positionCount = lineResolution;
            
            var projectileCollider = timeBubbleGrenadeProjectile.GetComponent<Collider>();
            foreach (var collider in hittableManager.colliders)
            {
                Physics.IgnoreCollision(projectileCollider, collider);
            }
        }

        private void OnEnable()
        {
            weaponUI.gameObject.SetActive(true);
        }
        
        private void OnDisable()
        {
            weaponUI.gameObject.SetActive(false);
        }

        public bool HandleThrow(bool isAiming, Vector3 targetPosition)
        {
            throwDirection = (targetPosition - spawnTransform.position).normalized;
            weaponUI.UpdateGrenadeEffect(timeBubbleGrenadeProjectile.LifeTimePercentage);

            switch (state)
            {
                case ThrowState.IDLE:
                    if (isAiming)
                    {
                        UpdateThrowLine();
                        ShowThrowLine();
                        state = ThrowState.AIM;
                        return true;
                    }
                    return false;
                case ThrowState.AIM:
                    if (isAiming)
                    {
                        UpdateThrowLine();
                        return true;
                    }
                    ThrowGrenade();
                    HideThrowLine();
                    state = ThrowState.IDLE;
                    return false;
                default:
                    return false;
            }
        }

        private void ThrowGrenade()
        {
            timeBubbleGrenadeProjectile.gameObject.SetActive(false);
            timeBubbleGrenadeProjectile.BeforeSetActive(
                spawnTransform.position,
                Quaternion.LookRotation(throwDirection, transform.up),
                throwPower
            );
            timeBubbleGrenadeProjectile.gameObject.SetActive(true);
        }

        private void ShowThrowLine()
        {
            throwLine.enabled = true;
            throwLine.positionCount = lineResolution;
        }

        private void HideThrowLine()
        {
            throwLine.enabled = false;
        }

        private void UpdateThrowLine()
        {
            Vector3 spawnPosition = spawnTransform.position;
            float gravity = - timeBubbleGrenadeProjectile.Gravity;
            float throwAngleCos = Vector3.Dot(throwDirection, Vector3.up);
            float throwAngle = -Mathf.PI * 0.5f + Mathf.Acos(throwAngleCos);
            
            throwLine.positionCount = lineResolution;
            Vector3 lastPosition = default;

            for (int i = 0; i < lineResolution; i++)
            {
                float simulationTime = i * lineStepSize;
                float displacementZ = throwPower * Mathf.Cos(throwAngle) * simulationTime;
                float displacementY = -0.5f * gravity * simulationTime * simulationTime
                                      + throwPower * Mathf.Sin(throwAngle) * simulationTime;
                var displacement = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0) 
                                   * (new Vector3(0, -displacementY, displacementZ));
                var position = spawnPosition + displacement;
                
                if (i > 0)
                {
                    var rayDirection = position - lastPosition;
                    RaycastHit hit;
                    Ray ray = new Ray(lastPosition, rayDirection);
                    if (Physics.Raycast(ray, out hit, rayDirection.magnitude, whatCanCollide.value))
                    {
                        throwLine.SetPosition(i, hit.point);
                        throwLine.positionCount = i + 1;
                        break;
                    }
                }
                throwLine.SetPosition(i, position);
                lastPosition = position;
            }
        }
    }
}

