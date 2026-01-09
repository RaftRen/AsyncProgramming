using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BootstrapEntryPoint : MonoBehaviour
{
    [Header("Image parameters")]
    [SerializeField] private RawImage _rawImageOut;
    [SerializeField] private string _logoURL;
    private Texture2D _downloadedTextureFromWeb;
    [Header("Text parameters")]
    [SerializeField] private Text _outputProjectNameText;
    private string _textFilePath = "Info/ProjectName";
    private TextAsset _loadedTextAssetFromResources;
    [Header("Audio parameters")]
    [SerializeField] private AudioSource _outputAudioSource;
    private string _audioFilePath = "Audio/BootstrapSceneAudio";
    [Header("Progress bar")]
    [SerializeField] private Slider _progressBar;
    [SerializeField] private TextMeshProUGUI _outputProgressBarText;

    private async Task<Texture2D> DownloadLogoFromWebAsync()
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(_logoURL);
        UnityWebRequestAsyncOperation asyncOperation = request.SendWebRequest();
        while (!asyncOperation.isDone)
        {
            await Task.Yield();
        }
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
            return null;
        }
        else
        {
            StepProgressBar();
            return DownloadHandlerTexture.GetContent(request);
        }
    }
    private async Task SetAndPrintTextWithLogo()
    {
        if (_downloadedTextureFromWeb != null)
        {
            _rawImageOut.texture = _downloadedTextureFromWeb;
            await PrintTextWithLogoSmoothly();
            StepProgressBar();
  
        }
        else
        {
            Debug.Log("Error: _downloadedTextureFromWeb == null");
        }
    }
    private async Task PrintTextWithLogoSmoothly()
    {
        float alpha = 0;
        float durationSec = 3f;
        float pauseBeforeShowText = 0.75f;
        Color currentColor = _rawImageOut.color;
        while (alpha <= 1)
        {
            alpha += Time.deltaTime / durationSec;
            _rawImageOut.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
            await Task.Yield();
        }
        while (pauseBeforeShowText > 0)
        {
            pauseBeforeShowText -= Time.deltaTime;
            await Task.Yield();
        }
        _outputProjectNameText.text = _loadedTextAssetFromResources.text;
        StepProgressBar();
    }
    private async Task<TextAsset> LoadTxtResourcesAsync()
    {
        TextAsset textAsset = Resources.Load<TextAsset>(_textFilePath);
        while (textAsset == null)
        {
            await Task.Yield();
        }
        StepProgressBar();
        return textAsset;
    }
    private async Task<AudioClip> LoadAudioResourcesAsync()
    {
        AudioClip audioClip = Resources.Load<AudioClip>(_audioFilePath);
        while (audioClip == null)
        {
            await Task.Yield();
        }
        StepProgressBar();
        return audioClip;
    }
    private async Task LoadGameSceneAsync()
    {
        float loadDuration = 6f;
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("GameScene");
        asyncOperation.allowSceneActivation = false;
        while (loadDuration > 0f) //вставил заглушку, чтобы сцена не загружалась мнгновенно
        {
            loadDuration -= Time.deltaTime;
            await Task.Yield();
        }
        while (!asyncOperation.isDone)
        {
            if (asyncOperation.progress >= 0.9f)
            {
                asyncOperation.allowSceneActivation = true;
            }
            await Task.Yield();
        }
    }
    private void StepProgressBar()
    {
        float progressBarStepValue = 0.2f;
        _progressBar.value += progressBarStepValue;
        _outputProgressBarText.text = $"{_progressBar.value * 100}%";
    }
    private async void Start()
    {
        Task loadGameSceneAsyncTask = LoadGameSceneAsync();
        Task<AudioClip> loadAudioResourcesAsyncTask = LoadAudioResourcesAsync();
        Task<Texture2D> downloadTextureFromWebTask = DownloadLogoFromWebAsync();
        Task<TextAsset> loadTxtResourcesAsyncTask = LoadTxtResourcesAsync();

        _downloadedTextureFromWeb = await downloadTextureFromWebTask;
        _loadedTextAssetFromResources = await loadTxtResourcesAsyncTask;
        
        _outputAudioSource.clip = await loadAudioResourcesAsyncTask;
        _outputAudioSource.Play();
        await SetAndPrintTextWithLogo();
        await loadGameSceneAsyncTask;
    }
    private void OnDestroy()
    {
        Destroy(_downloadedTextureFromWeb);
    }
}
