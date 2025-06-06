using UnityEngine;
using UnityEngine.AI;

public class AgentTimeObject : TimeObject
{
    public NavMeshAgent agent;
    public Animator animator;
    private float _defaultSpeed;

    private void Awake()
    {
        _defaultSpeed = agent.speed;
    }

    override
    public void PitchTimeScale(float newTimeScale)
    {
        if (!agent) return;
        
        base.PitchTimeScale(newTimeScale);
        agent.speed = _defaultSpeed * newTimeScale;
        animator.speed = newTimeScale;
    }
}
