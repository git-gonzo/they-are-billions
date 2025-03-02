using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DestinationMark : MonoBehaviour
{
    [SerializeField] private Transform _outer;
    [SerializeField] private Transform _inner;

    public void Init()
    {
        _outer.localScale = Vector3.zero;
        _outer.DOScale(1, 0.4f).SetEase(Ease.OutBack).OnComplete(
            ()=> _outer.DOScale(1.1f,0.25f).SetEase(Ease.OutSine).SetLoops(-1,LoopType.Yoyo));
    }
}