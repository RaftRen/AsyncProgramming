using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject _windowSettings;
    [SerializeField] private TextMeshProUGUI _windowSettingsText;
    private const int NUMBER_OF_CLICKS_TO_CANCEL = 3;
    private CancellationTokenSource cancellationTokenSource;
    private int _currentClicks = 0;

    public async void SettingsButton()
    {
        if (cancellationTokenSource != null)
        {
            return;
        }
        _currentClicks = 0;
        _windowSettings.SetActive(true);
        cancellationTokenSource = new CancellationTokenSource();
        Coroutine checkClicksCoroutine = StartCoroutine(CheckClicksCoroutine());
        await LoadSettingsWindowAsync(cancellationTokenSource.Token);
        StopCoroutine(checkClicksCoroutine);
        cancellationTokenSource.Dispose();
        cancellationTokenSource = null;
    }
    private async Task LoadSettingsWindowAsync(CancellationToken cancellationToken)
    {
        float windowSettingsLoadingTime = 15f;
        while (windowSettingsLoadingTime >= 0)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _windowSettingsText.text = "";
                _windowSettings.SetActive(false);
                return;
            }
            windowSettingsLoadingTime -= Time.deltaTime;
            _windowSettingsText.text = $"Идёт загрузка окна настроек...\nОсталось: {(byte)windowSettingsLoadingTime} сек\nДля отмены загрузки: 3 клика";
            await Task.Yield();
        }
        _windowSettingsText.text = "Загрузка завершена!";
    }
    private IEnumerator CheckClicksCoroutine()
    {
        while (true)
        {
            if (Input.GetMouseButtonDown(0))
            {
                _currentClicks++;
                if (_currentClicks >= NUMBER_OF_CLICKS_TO_CANCEL)
                {
                    cancellationTokenSource.Cancel();
                }
            }
            yield return null;
        }
    }
}
