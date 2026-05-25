using UnityEngine;

namespace SOs
{
    [CreateAssetMenu(fileName = "Wizard Data", menuName = "Gameplay/Wizard Data")]
    public class WizardData : ScriptableObject
    {
        public string id;

        [Header("Animator overrides")]
        [Tooltip("Override controller dùng cho Body animator của Player.")]
        public AnimatorOverrideController bodyController;

        [Tooltip("Override controller dùng cho Head animator của Player.")]
        public AnimatorOverrideController headController;
    }
}
