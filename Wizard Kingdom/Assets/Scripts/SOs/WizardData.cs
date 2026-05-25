using UnityEngine;

namespace SOs
{
    /// <summary>
    /// Gameplay data của một wizard. Áp lên Player trong scene Game khi user equip wizard này.
    /// id phải khớp với WizardItemData.id và id lưu trong UserData.inventory.
    /// </summary>
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
