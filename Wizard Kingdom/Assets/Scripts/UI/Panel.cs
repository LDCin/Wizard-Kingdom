using UnityEngine;

namespace UI
{
    public class Panel : MonoBehaviour
    {
        [SerializeField] private bool _destroyOnClose = false;
        [SerializeField] private UILayer uiLayer = UILayer.Overlay;

        public UILayer UILayer => uiLayer;
        public void Open()
        {
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}