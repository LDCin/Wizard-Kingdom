using System.Collections;
using System.Collections.Generic;
using Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UI
{
    public class UIManager : Singleton<UIManager>
    {
        [Header("UI Roots")]
        [SerializeField] private Transform overlayCanvasRoot;
        [SerializeField] private Transform cameraCanvasRoot;

        private Dictionary<string, Panel> _panelDict = new();
        private HashSet<string> _loadingPanels = new();

        private void Awake()
        {
            base.Awake();

            var existPanelList = GetComponentsInChildren<Panel>(true);

            foreach (var panel in existPanelList)
            {
                if (!_panelDict.ContainsKey(panel.name))
                {
                    _panelDict.Add(panel.name, panel);
                }
                else
                {
                    Debug.LogWarning($"Duplicate panel name: {panel.name}");
                }
            }
        }

        private Transform GetRoot(UILayer layer)
        {
            return layer switch
            {
                UILayer.Overlay => overlayCanvasRoot,
                UILayer.Camera => cameraCanvasRoot,
            };
        }

        public IEnumerator LoadPanel(string panelName)
        {
            if (_panelDict.ContainsKey(panelName))
            {
                yield break;
            }

            if (_loadingPanels.Contains(panelName))
            {
                yield return new WaitUntil(() => !_loadingPanels.Contains(panelName));
                yield break;
            }

            _loadingPanels.Add(panelName);

            var panelHandle = Addressables.InstantiateAsync(panelName, transform);
            yield return panelHandle;

            _loadingPanels.Remove(panelName);

            if (panelHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"Failed to load panel: {panelName}");
                yield break;
            }

            Panel newPanel = panelHandle.Result.GetComponent<Panel>();

            if (newPanel == null)
            {
                Debug.LogError($"Loaded object does not have Panel component: {panelName}");
                Addressables.ReleaseInstance(panelHandle.Result);
                yield break;
            }

            Transform root = GetRoot(newPanel.UILayer);

            if (root == null)
            {
                Debug.LogError($"Root is missing for UI layer: {newPanel.UILayer}");
                Addressables.ReleaseInstance(panelHandle.Result);
                yield break;
            }

            newPanel.transform.SetParent(root, false);
            newPanel.transform.SetAsLastSibling();

            _panelDict[panelName] = newPanel;
        }

        public Panel GetPanel(string panelName)
        {
            _panelDict.TryGetValue(panelName, out Panel panel);
            return panel;
        }

        public void OpenPanel(string panelName)
        {
            StartCoroutine(OpenPanelRoutine(panelName));
        }

        private IEnumerator OpenPanelRoutine(string panelName)
        {
            yield return StartCoroutine(LoadPanel(panelName));

            Panel panel = GetPanel(panelName);

            if (panel != null)
            {
                panel.Open();
            }
            else
            {
                Debug.LogError($"Panel not found: {panelName}");
            }
        }

        public void ClosePanel(string panelName)
        {
            Panel panel = GetPanel(panelName);

            if (panel != null)
            {
                panel.Close();
            }
            else
            {
                Debug.LogWarning($"Cannot close unloaded panel: {panelName}");
            }
        }

        public void CloseAllPanel()
        {
            foreach (var panel in _panelDict.Values)
            {
                panel.Close();
            }
        }
    }
}