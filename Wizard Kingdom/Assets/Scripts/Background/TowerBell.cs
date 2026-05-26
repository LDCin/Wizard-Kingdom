using Managers;
using UnityEngine;

namespace BackgroundSystem
{
    public class TowerBell : MonoBehaviour
    {
        [SerializeField] private Animator _bellAnimator;

        private void OnEnable()
        {
            GameManager.OnTimeExpired += HandleTimeExpired;
        }

        private void OnDisable()
        {
            GameManager.OnTimeExpired -= HandleTimeExpired;
        }

        private void HandleTimeExpired()
        {
            if (_bellAnimator == null) return;
            _bellAnimator.SetBool("Ring", true);
        }
    }
}

