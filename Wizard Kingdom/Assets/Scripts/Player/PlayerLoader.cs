using System.Collections;
using Managers;
using SOs;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using Utils;

namespace Players
{
    /// <summary>
    /// Gắn trên GameObject Wizard trong scene Game.
    /// Khi Start, đọc equipped wizard id từ DataManager,
    /// lookup WizardData trong GameplayCatalog, gán AnimatorOverrideController
    /// cho Body và Head animator.
    /// </summary>
    public class PlayerLoader : MonoBehaviour
    {
        [Header("Animators (drag từ scene)")]
        [SerializeField] private Animator _bodyAnimator;
        [SerializeField] private Animator _headAnimator;

        private AsyncOperationHandle<GameplayCatalog> _catalogHandle;
        private bool _hasCatalogHandle;

        private void Start()
        {
            StartCoroutine(LoadAndApplyWizardSkin());
        }

        private void OnDestroy()
        {
            if (_hasCatalogHandle)
            {
                GameplayCatalogLoader.Release(_catalogHandle);
                _hasCatalogHandle = false;
            }
        }

        private IEnumerator LoadAndApplyWizardSkin()
        {
            yield return GameplayCatalogLoader.Load((catalog, handle) =>
            {
                _catalogHandle = handle;
                _hasCatalogHandle = true;

                if (catalog == null) return;
                if (DataManager.Instance == null) return;

                string id = DataManager.Instance.GetEquippedWizard();
                WizardData data = catalog.FindWizard(id);

                if (data == null)
                {
                    Debug.LogWarning($"PlayerLoader: không tìm thấy WizardData cho id '{id}' trong GameplayCatalog.");
                    return;
                }

                if (_bodyAnimator != null && data.bodyController != null)
                    _bodyAnimator.runtimeAnimatorController = data.bodyController;

                if (_headAnimator != null && data.headController != null)
                    _headAnimator.runtimeAnimatorController = data.headController;
            });
        }
    }
}
