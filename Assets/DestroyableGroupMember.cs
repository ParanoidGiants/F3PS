using UnityEngine;
using UnityEngine.Events;

public class DestroyableGroupMember : MonoBehaviour
{
    private OnGroupDestroyed _onGroupDestroyed;

    public void ReqisterToGroup(OnGroupDestroyed onGroupDestroyed)
    {
        _onGroupDestroyed = onGroupDestroyed;
    }


    private void OnDestroy()
    {
        _onGroupDestroyed.OnGroupMemberDestroyed(this);
    }
}
