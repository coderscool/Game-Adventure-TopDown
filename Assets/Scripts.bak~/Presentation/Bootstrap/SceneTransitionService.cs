using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class SceneTransitionService
{
    private readonly Animator transitionAnimator;
    //private readonly float transitionDuration;

    public SceneTransitionService(Animator transitionAnimator, float transitionDuration)
    {
        this.transitionAnimator = transitionAnimator;
        //this.transitionDuration = transitionDuration;
    }

    public IEnumerator LoadSceneWithTransition(
        string sceneName,
        Func<IEnumerator> onSceneLoaded = null
    )
    {
        LoadingUI.Instance.Show();

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (operation.progress < 0.9f)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            LoadingUI.Instance.UpdateProgress(progress);

            yield return null;
        }

        operation.allowSceneActivation = true;

        while (!operation.isDone)
        {
            yield return null;
        }

        LoadingUI.Instance.UpdateProgress(1f);

        yield return null;

        if (onSceneLoaded != null)
        {
            yield return onSceneLoaded();
        }

        if (LoadingUI.Instance != null)
        {
            LoadingUI.Instance.Hide();
        }
    }
}
