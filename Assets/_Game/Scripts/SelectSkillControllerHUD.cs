using UnityEngine;

public class SelectSkillControllerHUD : MonoBehaviour
{
    public SelectableSkillHUD[] skillHuds;

    public void SelectSkillHud(int index)
    {
        for (int i = 0; i < skillHuds.Length; i++)
        {
            if (i == index)
            {
                skillHuds[i].Select();
            }
            else
            {
                skillHuds[i].Deselect();
            }
        }
    }
}
