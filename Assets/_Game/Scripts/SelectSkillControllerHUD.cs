using UnityEngine;

public class SelectSkillControllerHUD : MonoBehaviour
{
    public SelectableSkillHUD timeBubbleHud;
    public SelectableSkillHUD rewindHud;
    public SelectableSkillHUD telekinesisHud;

    public void SelectTimeBubbleHud()
    {
        timeBubbleHud.Select();
        rewindHud.Deselect();
        telekinesisHud.Deselect();
    }

    public void SelectRewindHud()
    {
        rewindHud.Select();
        timeBubbleHud.Deselect();
        telekinesisHud.Deselect();
    }

    public void SelectTelekinesisHud()
    {
        telekinesisHud.Select();
        timeBubbleHud.Deselect();
        rewindHud.Deselect();
    }

}
