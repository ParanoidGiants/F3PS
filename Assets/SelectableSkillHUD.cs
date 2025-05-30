using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SelectableSkillHUD : MonoBehaviour
{
    private RectTransform _rectTransform;
    private Image _icon;
    private Sequence _selectAnimation;
    public CanvasGroup associatedBar;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _icon = GetComponent<Image>();
    }

    public void Select()
    {
        if (_selectAnimation != null && _selectAnimation.IsActive() && _selectAnimation.IsPlaying())
        {
            _selectAnimation.Kill();
        }
        _selectAnimation = DOTween.Sequence();
        _selectAnimation.Append(_rectTransform.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack));
        _selectAnimation.Join(_icon.DOFade(1f, 0.2f));

        if (associatedBar != null)
        {
            _selectAnimation.Insert(0, associatedBar.DOFade(1f, 0.2f));
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
        _selectAnimation.Join(_icon.DOFade(0.5f, 0.2f));
        if (associatedBar != null)
        {
            _selectAnimation.Insert(0, associatedBar.DOFade(0.2f, 0.2f));
        }
    }
}
