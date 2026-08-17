using UnityEngine;

namespace UI
{
    public class UIEffect : MonoBehaviour
    {
        [SerializeField] protected bool autoPlay = true;
        [SerializeField] protected bool deltaTimeIndependent = false;
        [SerializeField] protected bool useClosePanelEffect = true;
        [SerializeField] protected float showDelayTime = 0f;
        [SerializeField] protected float hidePanelDelayTime = 0f;

        public bool UseCloseEffect => useClosePanelEffect;
        public bool DeltaTimeIndependent => deltaTimeIndependent;
        public float HidePanelDelayTime => hidePanelDelayTime;
        public virtual float ClosePanelDuration => 0f;

        private void OnEnable()
        {
            Open();
        }

        private void Open()
        {
            if (autoPlay)
            {
                ShowEffect(showDelayTime);
            }
        }

        public virtual void ShowEffect(float delayTime)
        {
        }

        public virtual void HideEffect(float delayTime)
        {
        }

        public virtual void FinishShowEffect()
        {
        }

        public void Close()
        {
            if (useClosePanelEffect)
            {
                HideEffect(0f);
            }
        }
    }
}
