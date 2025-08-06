using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using Unity.Netcode;

public class EntityAnimator : NetworkBehaviour
{
    #region Network

    [ClientRpc]
    public void PlayAttackAnimationClientRpc()
    {
        Debug.Log($"PlayAttackAnimationClientRpc");
        PlayAnimator("Attack", _effectCallback, _completeCallback); 
    }

    [ClientRpc]
    public void PlayDeathAnimationClientRpc()
    {
        Debug.Log($"PlayDeathAnimationClientRpc");
        PlayAnimator("Death", _effectCallback, _completeCallback);

    }

    [ClientRpc]
    public void PlayWalkAnimationClientRpc()
    {
        Debug.Log($"PlayDeathAnimationClientRpc");
        PlayAnimator("Walk", _effectCallback, _completeCallback);

    }

    [ClientRpc]
    public void PlayIdleAnimationClientRpc()
    {
        Debug.Log($"PlayDeathAnimationClientRpc");
        PlayAnimator("Idle", _effectCallback, _completeCallback);

    }

    public void PlayAnimation(string inAnimName, Action inEffectCallback = null, Action inCompleteCallback = null)
    {
        _effectCallback = inEffectCallback;
        _completeCallback = inCompleteCallback;
        switch (inAnimName)
        {
            case "Idle":
                PlayIdleAnimationClientRpc();
                break;

            case "Walk":
                PlayWalkAnimationClientRpc();
                break;

            case "Attack":
                PlayAttackAnimationClientRpc();
                break;

            case "Death":
                PlayDeathAnimationClientRpc();
                break;
        }
    }

    [ClientRpc]
    public void PlayHitEffectClientRpc(EffectDataComponent inEffectDataComp)
    {
        string effectPath = ResourceEffectPath.GetEffectResourcePath(inEffectDataComp.effectNameKey);

        float particleDuration = 0f;
        GameObject effectObject = null;
        ObjectPoolManager.Instance.GetPoolingObjects(effectPath).Dequeue(
            (effect) =>
            {
                effectObject = effect;
                effectObject.transform.position = inEffectDataComp.position;
                particleDuration = effectObject.GetComponent<Effect>()?.GetEffectDuration ?? 0f;
            });

        ObjectPoolManager.Instance.ReleasePoolingObjects(particleDuration, effectObject, effectPath,
            inComplete: () =>
            {
                inEffectDataComp.completeCallback?.Invoke();
            });
    }

    #endregion

    public Animator Animator => _animator;
    [SerializeField] private Animator _animator;

    /// <summary>
    /// 이펙트 재생 이벤트
    /// </summary>
    private Action _effectCallback;
    private Action _completeCallback;
    private Coroutine _animatorCoroutine;
    public Animator GetAnimator()
    {
        return _animator;
    }

    public void Rebind()
    {
        _animator.Rebind();
    }
    public void PlayAnimator(string inAniName, Action inEffectCallback)
    {
        _effectCallback = inEffectCallback;

        if (_animator != null && _animator.gameObject.activeSelf && _animator.gameObject.activeInHierarchy)
        {
            _animator.enabled = true;
            _animator.Play(inAniName, 0, 0f);
        }
    }

    public void PlayAnimator(string inAniName, Action inEffectCallback = null, Action inCompleteCallback = null)
    {
        if (_animatorCoroutine != null)
        {
            StopCoroutine(_animatorCoroutine);
        }

        _animatorCoroutine = StartCoroutine(Co_PlayAnimator(inAniName, inEffectCallback, inCompleteCallback));
    }

    private IEnumerator Co_PlayAnimator(string inAniNames, Action inEffectCallback = null, Action inCompleteCallback = null)
    {
        PlayAnimator(inAniNames, inEffectCallback);
        yield return new WaitForSeconds(GetAnimatorLength(inAniNames));
        inCompleteCallback?.Invoke();
    }

    public void PlayAnimator(string[] inAniNames, Action inCompleteCallback = null)
    {
        if (_animatorCoroutine != null)
        {
            StopCoroutine(_animatorCoroutine);
        }

        _animatorCoroutine = StartCoroutine(Co_PlayAnimator(inAniNames, inCompleteCallback));
    }

    private IEnumerator Co_PlayAnimator(string[] inAniNames, Action inCompleteCallback)
    {
        yield return null;
        int count = inAniNames.Length;
        for (int i = 0; i < count; i++)
        {
            string aniName = inAniNames[i];
            _animator.enabled = true;
            _animator.Play(inAniNames[i], 0, 0f);
            yield return new WaitForSeconds(GetAnimatorLength(inAniNames[i]));
        }

        inCompleteCallback?.Invoke();
    }

    public float GetAnimatorLength(string inAnimationName)
    {
        if (_animator != null)
        {
            var clips = _animator.runtimeAnimatorController.animationClips;
            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i].name == inAnimationName)
                {
                    return clips[i].length;
                }
            }
        }
        return 0f;
    }
    public string GetCurrentAnimatorClipName()
    {
        if (_animator != null)
        {
            AnimatorClipInfo[] clipInfo = _animator.GetCurrentAnimatorClipInfo(0);
            if (clipInfo != null && clipInfo.Length > 0)
            {
                return clipInfo[0].clip.name;
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// effect 발동까지의 걸리는 시간.
    /// </summary>
    /// <param name="inAnimationName">애니메이션 이름</param>
    /// <returns></returns>
    public float GetAnimatorEffectEventLength(string inAnimationName)
    {
        float length = 0f;

        if (_animator == null)
        {
            return length;
        }

        if (_animator.runtimeAnimatorController == null)
        {
            return length;
        }

        var clips = _animator.runtimeAnimatorController.animationClips;
        if (clips == null)
        {
            return length;
        }

        int clipCount = clips.Length;
        for (int i = 0; i < clipCount; i++)
        {
            AnimationClip clip = clips[i];
            if (clip.name != inAnimationName)
            {
                continue;
            }

            AnimationEvent[] clipEvents = clip.events;
            if (clipEvents == null)
            {
                return length;
            }

            foreach (var animEvent in clipEvents)
            {
                if (animEvent == null)
                {
                    continue;
                }

                if (animEvent.functionName == "PlayEffect")
                {
                    return animEvent.time;
                }
            }
        }

        return length;
    }
    public void PlayEffect()
    {
        var tmpCallback = _effectCallback;
        _effectCallback = null;
        tmpCallback?.Invoke();
    }

    public void CompleteCallback()
    {
        var tmpCallback = _completeCallback;
        _completeCallback = null;
        tmpCallback?.Invoke();
    }
}
