using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CalculationsManager : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private TMP_InputField _firstNumber;
    [SerializeField] private TMP_InputField _secondNumber;
    [SerializeField] private TextMeshProUGUI _resultNumber;
    private async Task<float> SumInAnotherStream()
    {
        Task<float> sumTask = Task.Run(() =>
        {
            float firstNumber = float.TryParse(_firstNumber.text, out firstNumber) ? firstNumber : 0f;
            float secondNumber = float.TryParse(_secondNumber.text, out secondNumber) ? secondNumber : 0f;
            return firstNumber + secondNumber;
        });
        return await sumTask;
    }
    private void Start()
    {
        Debug.Log($"Главный поток имеет ID: {Environment.CurrentManagedThreadId}");
    }
    private async void Update()
    {
        Task<float> sumInAnotherStreamTask = SumInAnotherStream();
        //...
        //Тут какая-либо иная логика, пока в другом потоке выполняются "сложные вычисления" в виде сложения двух чисел
        //...
        float resultNumber = await sumInAnotherStreamTask;
        _resultNumber.text = resultNumber.ToString();
    }
}