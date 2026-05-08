using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace UI
{
    public static class SceneLoader
    {
        private static bool _isLoading;

        public static void LoadScene(string sceneName, string panelName)
        {
            if (_isLoading) return;
            UIManager.Instance.CloseAllPanel();
            UIManager.Instance.StartCoroutine(TransitionAndLoad(sceneName, panelName));
        }

        private static IEnumerator TransitionAndLoad(string sceneName, string panelName)
        {
            _isLoading = true;

            string pName = "Panel - Transition";

            yield return UIManager.Instance.LoadPanel(pName);

            TransitionPanel trans = UIManager.Instance.GetPanel(pName) as TransitionPanel;

            bool isDone = false;
            trans.ShowTransition(() => isDone = true);
            yield return new WaitUntil(() => isDone);

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            trans.HideTransition(() =>
            {
                _isLoading = false;
                UIManager.Instance.OpenPanel(panelName);
            });
        }
    }
}