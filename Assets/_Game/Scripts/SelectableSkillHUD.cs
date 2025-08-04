using DG.Tweening;
using UnityEngine;

public class SelectableSkillHUD : MonoBehaviour
{
    public Skill skillType;
    public RectTransform _rectTransform;
    public CanvasGroup canvasGroup;

    private Sequence _selectAnimation;

    public void Select()
    {
        if (_selectAnimation != null && _selectAnimation.IsActive() && _selectAnimation.IsPlaying())
        {
            _selectAnimation.Kill();
        }
        _selectAnimation = DOTween.Sequence();
        _selectAnimation.Append(_rectTransform.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack));

        _selectAnimation.Insert(0, canvasGroup.DOFade(1f, 0.2f));
    }

    public void Deselect()
    {
        if (_selectAnimation != null && _selectAnimation.IsActive() && _selectAnimation.IsPlaying())
        {
            _selectAnimation.Kill();
        }
        _selectAnimation = DOTween.Sequence();
        _selectAnimation.Append(_rectTransform.DOScale(1f, .2f).SetEase(Ease.OutBack));

        _selectAnimation.Insert(0, canvasGroup.DOFade(.2f, .2f));
    }
}
