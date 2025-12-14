using DG.Tweening;
using F3PS;
using StarterAssets;
using UnityEngine;

public class OnTouchHeal : MonoBehaviour
{
    private Sequence _spawnSequence;
    private void OnEnable()
    {
        transform.parent = null;
        transform.localScale = Vector3.zero;
        var startPosition = transform.position;
        var inTime = 0.5f;
        var outTime = 0.2f;
        _spawnSequence = DOTween.Sequence();
        _spawnSequence.Insert(0f, transform.DOScale(1.2f * Vector3.one, inTime).SetEase(Ease.InCubic))
            .Insert(0f, transform.DOMoveY(startPosition.y + 1f, inTime).SetEase(Ease.InCubic))
            .Insert(inTime, transform.DOScale(Vector3.one, outTime).SetEase(Ease.OutBack))
            .Insert(inTime, transform.DOMoveY(startPosition.y, outTime).SetEase(Ease.OutBack))
            .Insert(inTime + outTime, transform.DOMoveY(startPosition.y, 1f))
            .Play();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<ThirdPersonController>(out var player))
        {
            GameManager.Instance.saveGameManager.PlayerEventController.UpdateCurrentHealth(GameManager.Instance.saveGameManager.GameData.PlayerData.MaxHealth);
            _spawnSequence.Kill();
            Destroy(gameObject);
        }
    }
}
