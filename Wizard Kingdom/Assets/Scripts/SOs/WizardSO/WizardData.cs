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
    }
}

