using UnityEngine;

namespace UI
{
    public class Panel : MonoBehaviour
    {
        [SerializeField] private bool _destroyOnClose = false;
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