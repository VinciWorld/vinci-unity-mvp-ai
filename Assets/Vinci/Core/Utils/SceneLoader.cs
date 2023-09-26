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
            await Task.Delay(1000);
            _progressBar.fillAmount = scene.progress;


        } while (scene.progress < 0.9f);

        scene.allowSceneActivation = true;
        await Task.Yield();

        _loaderCanvas.SetActive(false);
    }
    
}
