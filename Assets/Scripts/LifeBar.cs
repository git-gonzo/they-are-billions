using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class LifeBar : MonoBehaviour
{
    [SerializeField] Image _bar;

    private Transform _followTarget;

    public void Init(Transform t) 
    {
        _followTarget = t;
    }

    private void Update()
    {
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        if (_followTarget == null) return;
        // Convertir la posición del objeto 3D a coordenadas de pantalla
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(_followTarget.position);

        // Si el objeto está detrás de la cámara, ocultarlo
        if (screenPosition.z < 0)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
            transform.position = screenPosition; // Mover el UI al punto en pantalla
        }
    }

    public void SetLife(float amount) 
    {
        gameObject.SetActive(amount <= 0 ? false : true);
        _bar.DOFillAmount(amount > 0 ? amount : 0, 0.1f).SetLink(gameObject);
    }

    public void Show(bool value) 
    {
        if (value)
        {
            Debug.Log("Update Lifebar Position on Show");
            UpdatePosition();
            gameObject.SetActive(true);
        }
    }
}