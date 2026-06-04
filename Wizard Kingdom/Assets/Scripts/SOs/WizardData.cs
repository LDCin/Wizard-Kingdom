using UnityEngine;

namespace SOs
{
    [CreateAssetMenu(fileName = "Wizard Data", menuName = "Gameplay/Wizard Data")]
    public class WizardData : ScriptableObject
    {
        public string id;

        [Header("Animator overrides")]
        [Tooltip("Override controller for wizard body animator.")]
        public AnimatorOverrideController bodyController;

        [Tooltip("Override controller for wizard head animator.")]
        public AnimatorOverrideController headController;

        #region Analysis And Design Methods

        public WizardData Get() => this;
        public WizardData Set() => this;

        public WizardData Init(Animator bodyAnimator, Animator headAnimator)
        {
            if (bodyAnimator != null && bodyController != null)
            {
                bodyAnimator.runtimeAnimatorController = bodyController;
            }

            if (headAnimator != null && headController != null)
            {
                headAnimator.runtimeAnimatorController = headController;
            }

            return this;
        }

        public WizardData get() => Get();
        public WizardData set() => Set();
        public WizardData init(Animator bodyAnimator, Animator headAnimator)
        {
            return Init(bodyAnimator, headAnimator);
        }

        #endregion
    }
}

