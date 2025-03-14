using UnityEngine;
using UnityEngine.AI;

public class AgentTimeObject : TimeObject
{
    public NavMeshAgent agent;
    public Animator animator;
    
    override
    public void PitchTimeScale(float newTimeScale)
    {
        if (!agent) return;
        
        float relation = newTimeScale / currentTimeScale;
        agent.speed *= relation;
        base.PitchTimeScale(newTimeScale);
        animator.speed = newTimeScale;
    }
}
