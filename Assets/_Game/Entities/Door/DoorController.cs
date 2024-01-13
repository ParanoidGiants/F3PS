using UnityEngine;

public class DoorController : MonoBehaviour
{
    public Animator door;
    public bool isOpen = false;
    public bool open = false;
    public bool close = false;
    
    private void Update()
    {
        if (!isOpen && open)
        {
            close = false;
            OnOpenDoor();
        }
        
        if (isOpen && close)
        {
            open = false;
            OnCloseDoor();
        }
    }

    private void OnOpenDoor()
    {
        isOpen = true;
        door.SetBool("Open", isOpen);
    }
    
    private void OnCloseDoor()
    {
        isOpen = false;
        door.SetBool("Open", isOpen);
    }
}
