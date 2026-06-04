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
            Init();
        }

        public void Init()
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

                data.Init(_bodyAnimator, _headAnimator);

                completed = true;
            });

            yield return new WaitUntil(() => completed);
        }
    }
}
