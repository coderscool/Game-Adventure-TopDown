using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Features.GameFlow
{
    public static class SceneTransitionService
    {
        // AsyncOperation.progress stops at 0.9 until allowSceneActivation is enabled.
        private const float ActivationProgress = 0.9f;

        public static IEnumerator LoadSceneWithTransition(
            string sceneName,
            Func<IEnumerator> onSceneLoaded = null)
        {
            LoadingUI loadingUI = LoadingUI.Instance;
            if (loadingUI != null)
                loadingUI.Show();

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            if (operation == null)
            {
                if (loadingUI != null)
                    loadingUI.Hide();
                yield break;
            }

            operation.allowSceneActivation = false;

            while (operation.progress < ActivationProgress)
            {
                if (loadingUI != null)
                    loadingUI.UpdateProgress(Mathf.Clamp01(operation.progress / ActivationProgress));

                yield return null;
            }

            operation.allowSceneActivation = true;

            while (!operation.isDone)
                yield return null;

            if (loadingUI != null)
                loadingUI.UpdateProgress(1f);

            yield return null;

            if (onSceneLoaded != null)
                yield return onSceneLoaded();

            if (loadingUI != null)
                loadingUI.Hide();
        }
    }
}
