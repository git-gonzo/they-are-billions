using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class PopupControllerBase : MonoBehaviour
{ 
    [SerializeField] protected Button _btnClose;
    protected virtual void Start()
    {
        _btnClose.onClick.AddListener(ClosePopup);
    }

    public virtual void ShowAnim() 
    {
        transform.DOScale(Vector3.one * 0.5f, 0);
        transform.DOScale(Vector3.one * 1f, 0.3f).SetEase(Ease.OutBack);
    }
    
    public virtual void HideAnim(Action OnComplete) 
    {
        transform.DOScale(Vector3.one * 0.5f, 0.3f).SetEase(Ease.InBack).OnComplete(()=>OnComplete?.Invoke());
    }

    private void ClosePopup()
    {
        HideAnim(() => gameObject.SetActive(false));
    }
}