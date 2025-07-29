using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
public class ElixirManager : Singleton<ElixirManager> , IManager
{
    public const float START_ELIXIR_COUNT = 2f;
    public const float MAX_ELIXIR_COUNT = 10f;

    private const float BASE_REGEN_INTERVAL = 2.8f;

    private float _maxElixir;
    private float _startElixir;
    private float _regenInterval; 

    public float CurrentElixir { get; private set; }
    public float MaxElixir => _maxElixir;

    private int elixirMultiplier = 1;

    public event Action<int> OnElixirChanged; 
    public IEnumerator Co_Initialzie()
    {
        yield return null;

        _maxElixir = MAX_ELIXIR_COUNT;
        _startElixir = START_ELIXIR_COUNT;
        _regenInterval = BASE_REGEN_INTERVAL;
    }

    private Coroutine regenCoroutine;

    public void GameStart()
    {
        CurrentElixir = _startElixir;
        regenCoroutine = StartCoroutine(Co_ElixirRegenRoutine());
    }

    private IEnumerator Co_ElixirRegenRoutine()
    {
        while (true)
        {
            while (CurrentElixir >= _maxElixir)
                yield return null;

            float elapsed = 0f;
            float interval = _regenInterval / elixirMultiplier;

            while (elapsed < interval && CurrentElixir < _maxElixir)
            {
                float delta = Time.deltaTime / interval; // 0~1
                CurrentElixir = Mathf.Min(CurrentElixir + delta, _maxElixir);
                elapsed += Time.deltaTime;
                yield return null;
            }

            CurrentElixir = Mathf.Min(Mathf.Floor(CurrentElixir), _maxElixir);
            OnElixirChanged?.Invoke((int)CurrentElixir);
        }
    }

    public bool TryUseElixir(float amount)
    {
        if (CurrentElixir < amount)
            return false;

        return true;
    }

    public void UseElixir(float amount)
    {
        CurrentElixir -= amount;
        OnElixirChanged?.Invoke((int)CurrentElixir);
    }

    public float CurrentFillAmount
    {
        get
        {
            return CurrentElixir / _maxElixir;
        }
    }

    public void SetElixirMultiplier(int multiplier)
    {
        multiplier = Mathf.Clamp(multiplier, 1, 3);
        if (elixirMultiplier != multiplier)
        {
            elixirMultiplier = multiplier;

            if (regenCoroutine != null)
            {
                StopCoroutine(regenCoroutine);
                regenCoroutine = StartCoroutine(Co_ElixirRegenRoutine());
            }
        }
    }
}
