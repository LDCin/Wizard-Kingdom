using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace UI
{
    public static class SceneLoader
    {
        private static bool _isLoading;

        public static void LoadScene(string sceneName, params string[] panelNames)
        {
            if (_isLoading) return;

            UIManager.Instance.CloseAllPanelExcept("Panel - Transition");
            UIManager.Instance.StartCoroutine(TransitionAndLoad(sceneName, panelNames));
        }

        private static IEnumerator TransitionAndLoad(string sceneName, string[] panelNames)
        {
            _isLoading = true;

            string pName = "Panel - Transition";

            yield return UIManager.Instance.LoadPanel(pName);

            TransitionPanel trans = UIManager.Instance.GetPanel(pName) as TransitionPanel;

            if (trans == null)
            {
                Debug.LogError("TransitionPanel not found.");
                _isLoading = false;
                yield break;
            }

            bool isDone = false;
            trans.ShowTransition(() => isDone = true);
            yield return new WaitUntil(() => isDone);

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            foreach (string panelName in panelNames)
            {
                if (string.IsNullOrEmpty(panelName))
                {
                    continue;
                }

                yield return UIManager.Instance.LoadPanel(panelName);

                Panel panel = UIManager.Instance.GetPanel(panelName);

                if (panel != null)
                {
                    panel.Open();
                }
                else
                {
                    Debug.LogWarning($"Panel not found: {panelName}");
                }
            }

            trans.HideTransition(() =>
            {
                _isLoading = false;
            });
        }
    }
}