using UnityEngine.AI;

public class AgentTimeObject : TimeObject
{
    public NavMeshAgent agent;
    
    override
    public void PitchTimeScale(float newTimeScale)
    {
        float relation = newTimeScale / currentTimeScale;
        agent.speed *= relation;
        
        base.PitchTimeScale(newTimeScale);
    }
}
