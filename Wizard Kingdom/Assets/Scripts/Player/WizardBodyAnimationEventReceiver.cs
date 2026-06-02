using UnityEngine;

namespace Wizards
{
    public class WizardBodyAnimationEventReceiver : MonoBehaviour
    {
        [SerializeField] private Wizard _wizard;

        private void Awake()
        {
            if (_wizard == null)
            {
                _wizard = GetComponentInParent<Wizard>();
            }
        }

        public void OnSnapAnimationFinished()
        {
            _wizard.OnSnapAnimationFinished();
        }

        public void OnDeadAnimationFinished()
        {
            _wizard.OnDeadAnimationFinished();
        }
    }
}
