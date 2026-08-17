using System.Collections;
using Managers;
using SOs;
using UnityEngine;

namespace Wizards
{
    public class WizardLoader : MonoBehaviour
    {
        [Header("Animators (drag từ scene)")]
        [SerializeField] private Animator _bodyAnimator;
        [SerializeField] private Animator _headAnimator;

        private void Start()
        {
            StartCoroutine(LoadAndApplyWizardSkin());
        }

        private IEnumerator LoadAndApplyWizardSkin()
        {
            bool completed = false;
            DataManager.Instance.LoadGameplayCatalog(catalog =>
            {
                if (catalog == null || DataManager.Instance == null)
                {
                    completed = true;
                    return;
                }

                string id = DataManager.Instance.GetEquippedWizard();
                WizardData data = DataManager.Instance.FindWizardData(id);

                if (data == null)
                {
                    Debug.LogWarning($"WizardLoader: not found WizardData for id '{id}' in GameplayCatalog.");
                    completed = true;
                    return;
                }

                if (_bodyAnimator != null && data.bodyController != null)
                    _bodyAnimator.runtimeAnimatorController = data.bodyController;

                if (_headAnimator != null && data.headController != null)
                    _headAnimator.runtimeAnimatorController = data.headController;

                completed = true;
            });

            yield return new WaitUntil(() => completed);
        }
    }
}
