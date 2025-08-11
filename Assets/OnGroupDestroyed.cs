using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnGroupDestroyed : MonoBehaviour
{
    public List<DestroyableGroupMember> members;
    public UnityEvent OnGroupDestroy;

    void Awake()
    {
        foreach (var member in members)
        {
            member.ReqisterToGroup(this);
        }  
    }

    public void OnGroupMemberDestroyed(DestroyableGroupMember member)
    {
        members.Remove(member);

        if (members.Count == 0)
        {
            OnGroupDestroy.Invoke();
            Destroy(gameObject);
        }
    }
}
