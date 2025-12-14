using F3PS;
using UnityEngine;
    
public class KhonsuSphereController : MonoBehaviour
{
    private KhonsuSphereSkillData KhonsuSphereData => GameManager.Instance.GameData.PlayerData.KhonsuSphereSkillData;

    [Header("References")]
    public Transform userSpace;
    public Collider[] ownColliders;
    public KhonsuSphereProjectile khonsuSphereProjectile;
    public LineRenderer throwLine;
    public Transform spawnTransform;
    public Animator animator;

    [Space(10)]
    [Header("Settings")]
    public int lineResolution = 100;
    public float lineStepSize = 0.1f;
    public LayerMask whatCanCollide;

    [Space(10)]
    [Header("Watchers")]
    public bool isThrown;
    public bool isAimingThisFrame;
    public bool wasAimingLastFrame;
    public bool isDeactivated;
    public Vector3 throwDirection;

    private void Awake()
    {
        throwLine.positionCount = lineResolution;
        khonsuSphereProjectile.Init(ownColliders);
        var projectileCollider = khonsuSphereProjectile.GetComponent<Collider>();
        foreach (var collider in ownColliders)
        {
            Physics.IgnoreCollision(projectileCollider, collider);
        }
    }

    public void OnUpdate(bool isAiming, float bubbleTimeScaleChange, Vector3 targetPosition)
    {
        wasAimingLastFrame = isAimingThisFrame;
        isAimingThisFrame = isAiming;

        throwDirection = (targetPosition - spawnTransform.position).normalized;
        if (khonsuSphereProjectile.isUpAndRunning)
        {
            if (isAimingThisFrame && !wasAimingLastFrame)
            {
                isThrown = false;
                isDeactivated = true;
                khonsuSphereProjectile.DeactivateKhonsuSphere();
            }
        }
        else if (isThrown)
        {
            if (isAimingThisFrame && !wasAimingLastFrame)
            {
                isThrown = false;
                isDeactivated = true;
                khonsuSphereProjectile.InterruptThrow();
            }
        }
        else if (!khonsuSphereProjectile.khonsuSphere.isActiveAndEnabled)
        {
            if (!isDeactivated)
            {
                if (isAimingThisFrame && wasAimingLastFrame)
                {
                    UpdateThrowLine();
                }
                else if (wasAimingLastFrame)
                {
                    ThrowProjectile();
                    HideThrowLine();
                    wasAimingLastFrame = false;
                }
                else if (isAimingThisFrame)
                {
                    UpdateThrowLine();
                    ShowThrowLine();
                    wasAimingLastFrame = true;
                }
            }
            else if (!isAimingThisFrame)
            {
                isDeactivated = false;
            }
        }

        if (bubbleTimeScaleChange != 0f)
        {
            khonsuSphereProjectile.PitchTimeScale(bubbleTimeScaleChange * KhonsuSphereData.ChangeTimeScaleSpeed);
        }
    }

    private void ThrowProjectile()
    {
        isThrown = true;
        khonsuSphereProjectile.gameObject.SetActive(false);
        khonsuSphereProjectile.BeforeSetActive(
            spawnTransform.position,
            spawnTransform.position + throwDirection,
            KhonsuSphereData.ThrowPower
        );
        khonsuSphereProjectile.gameObject.SetActive(true);
        animator.SetTrigger("KhonsuSphere");
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
        var throwPower = KhonsuSphereData.ThrowPower;
        Vector3 spawnPosition = spawnTransform.position;
        float gravity = Physics.gravity.y;
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

    public bool IsAiming()
    {
        return !khonsuSphereProjectile.isUpAndRunning
            && !isThrown
            && !khonsuSphereProjectile.isActiveAndEnabled
            && !isDeactivated
            && isAimingThisFrame
            && wasAimingLastFrame;
    }
}

