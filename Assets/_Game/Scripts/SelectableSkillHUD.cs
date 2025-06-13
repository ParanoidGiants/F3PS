using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SelectableSkillHUD : MonoBehaviour
{
    public CanvasGroup[] associatedCanvasGroups;
    public RectTransform _rectTransform;

    private Sequence _selectAnimation;

    public void Select()
    {
        if (_selectAnimation != null && _selectAnimation.IsActive() && _selectAnimation.IsPlaying())
        {
            _selectAnimation.Kill();
        }
        _selectAnimation = DOTween.Sequence();
        _selectAnimation.Append(_rectTransform.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack));

        foreach (var canvasGroup in associatedCanvasGroups)
        {
            _selectAnimation.Insert(0, canvasGroup.DOFade(1f, 0.2f));
        }
    }

    public void Deselect()
    {
        if (_selectAnimation != null && _selectAnimation.IsActive() && _selectAnimation.IsPlaying())
        {
            _selectAnimation.Kill();
        }
        _selectAnimation = DOTween.Sequence();
        _selectAnimation.Append(_rectTransform.DOScale(1f, 0.2f).SetEase(Ease.OutBack));

        foreach (var canvasGroup in associatedCanvasGroups)
        {
            _selectAnimation.Insert(0, canvasGroup.DOFade(1f, 0.2f));
            _selectAnimation.Insert(0, canvasGroup.DOFade(0.2f, 0.2f));
        }
    }

    private void OnDestroy()
    {
        if (_selectAnimation != null && _selectAnimation.IsActive() && _selectAnimation.IsPlaying())
        {
            _selectAnimation.Kill();
        }
    }
}
