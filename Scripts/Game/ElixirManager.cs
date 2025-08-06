using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
public class ElixirManager : NetworkSingleton<ElixirManager> , IManager
{
    public const float START_ELIXIR_COUNT = 2f;
    public const float MAX_ELIXIR_COUNT = 10f;

    private const float BASE_REGEN_INTERVAL = 2.8f;

    private float _maxElixir;
    private float _startElixir;
    private float _regenInterval; // 2.8초마다 1회 충전


    public float CurrentElixir { get; private set; }
    public float MaxElixir => _maxElixir;

    // 1: 기본, 2: 2배, 3: 3배
    private int elixirMultiplier = 1;

    public event Action<int> OnElixirChanged; // current, max
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
            // 이미 최대치면 대기
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

            // 정수단위로 올림 (1씩 증가, UI이벤트 등)
            CurrentElixir = Mathf.Min(Mathf.Floor(CurrentElixir), _maxElixir);
            OnElixirChanged?.Invoke((int)CurrentElixir);
        }
    }

    /// <summary>
    /// 엘릭서 소모 시도. 성공하면 true 반환
    /// </summary>
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
            // 이미 10이면 1.0f로 무조건 고정
            if (CurrentElixir >= _maxElixir)
                return 1f;
            return CurrentElixir / _maxElixir;
        }
    }

    /// <summary>
    /// 엘릭서 충전 속도 변경 (1x, 2x, 3x)
    /// </summary>
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
