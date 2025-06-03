using UnityEngine;

public class SelectAttackControllerHUD : MonoBehaviour
{
    public SelectableAttackHUD meleeAttackHud;
    public SelectableAttackHUD longRangeAttackHud;

    public void SelectMeleeAttackHud()
    {
        meleeAttackHud.Select();
        longRangeAttackHud.Deselect();
    }

    public void SelectLongRangeAttackHud()
    {
        longRangeAttackHud.Select();
        meleeAttackHud.Deselect();
    }
}
