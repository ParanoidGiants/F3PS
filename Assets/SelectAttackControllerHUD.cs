using UnityEngine;

public class SelectAttackControllerHUD : MonoBehaviour
{
    public SelectableAttackHUD meleeAttackHud;
    public SelectableAttackHUD rangeAttackHud;

    public void SelectMeleeAttackHud()
    {
        meleeAttackHud.Select();
        rangeAttackHud.Deselect();
    }

    public void SelectRangeAttackHud()
    {
        rangeAttackHud.Select();
        meleeAttackHud.Deselect();
    }
}
