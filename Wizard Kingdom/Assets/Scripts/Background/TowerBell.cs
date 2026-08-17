using UnityEngine;
using Utils;

namespace BackgroundSystem
{
    public class TowerBell : MonoBehaviour
    {
        [SerializeField] private Animator _bellAnimator;

        private void OnEnable()
        {
            Observer.Subscribe(ObserverEvent.TimeExpired, HandleTimeExpired);
        }

        private void OnDisable()
        {
            Observer.Unsubscribe(ObserverEvent.TimeExpired, HandleTimeExpired);
        }

        private void HandleTimeExpired()
        {
            if (_bellAnimator == null) return;
            _bellAnimator.SetBool("Ring", true);
        }
    }
}
