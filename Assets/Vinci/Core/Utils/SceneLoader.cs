using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Vinci.Core.Utils;

public class SceneLoader : PersistentSingleton<SceneLoader>
{
    [SerializeField] private GameObject _loaderCanvas;
    [SerializeField] private Image _progressBar;

    public async Task LoadScene(string sceneName)
    {
        var scene = SceneManager.LoadSceneAsync(sceneName);
        scene.allowSceneActivation = false;

        _loaderCanvas.SetActive(true);

        do
        {
            await Task.Delay(100);
           // _progressBar.fillAmount = scene.progress;

            Debug.Log("Delay");
        } while (scene.progress < 0.9f);

        scene.allowSceneActivation = true;
        await Task.Yield();

        _loaderCanvas.SetActive(false);
    }

    public void LoadSceneDelay(string sceneName)
    {
        StartCoroutine(_LoadSceneCoroutine(sceneName));
    }

    private IEnumerator _LoadSceneCoroutine(string sceneName)
    {
        var scene = SceneManager.LoadSceneAsync(sceneName);
        scene.allowSceneActivation = false;

        _loaderCanvas.SetActive(true);

        while (scene.progress < 0.9f)
        {
           // _progressBar.fillAmount = scene.progress;
            yield return new WaitForSeconds(2f);
        }

        scene.allowSceneActivation = true;
        _loaderCanvas.SetActive(false);
    }

}
