using DG.Tweening;
using UnityEngine;

public class SelectableAttackHUD : MonoBehaviour
{
    public Attack attackType;

    private CanvasGroup _canvasGroup;
    private RectTransform _rectTransform;
    private Sequence _selectAnimation;

    public void Select()
    {
        if (_selectAnimation != null && _selectAnimation.IsActive() && _selectAnimation.IsPlaying())
        {
            _selectAnimation.Kill();
        }
        _selectAnimation = DOTween.Sequence();
        _selectAnimation.Append(_rectTransform.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack));

        _selectAnimation.Insert(0, _canvasGroup.DOFade(1f, 0.2f));
    }

    public void Deselect()
    {
        if (_selectAnimation != null && _selectAnimation.IsActive() && _selectAnimation.IsPlaying())
        {
            _selectAnimation.Kill();
        }
        _selectAnimation = DOTween.Sequence();
        _selectAnimation.Append(_rectTransform.DOScale(1f, 0.2f).SetEase(Ease.OutBack));

        _selectAnimation.Insert(0, _canvasGroup.DOFade(1f, 0.2f));
        _selectAnimation.Insert(0, _canvasGroup.DOFade(0.2f, 0.2f));
    }
}
