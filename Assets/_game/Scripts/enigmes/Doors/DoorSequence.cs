using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Playables;


public class DoorSequence : MonoBehaviour
{
    const int MainPlayerIndex = 0;

    [SerializeField] List<DoorCondition> _conditions = new List<DoorCondition>();

    [SerializeField] PlayableDirector _director;
    [Tooltip("Désactivée dans la scène, Priority = 10, Output Channel = Channel01 + Channel02.")]
    [SerializeField] CinemachineCamera _doorCamera;

    [SerializeField] ScreenFader _fader;
    [SerializeField, Min(0f)] float _fadeDuration = 0.2f;

    [SerializeField] UnityEvent _onFinished = new UnityEvent();

    bool _hasStarted;

    void Awake()
    {
        if (_director == null || _doorCamera == null || _fader == null)
        {
            enabled = false;
        }
    }

    void Update()
    {
        if (_hasStarted || !AreConditionsMet()) return;

        _hasStarted = true;
        StartCoroutine(PlaySequence());
    }

    bool AreConditionsMet()
    {
        if (_conditions.Count == 0) return false;

        foreach (DoorCondition condition in _conditions)
        {
            if (!condition.IsMet) return false;
        }
        return true;
    }

    IEnumerator PlaySequence()
    {
        SetPlayersInputActive(false);

        yield return BlendToDoorCamera();
        yield return FadeTransition(() => SetSplitScreen(false));
        yield return PlayCinematic();
        yield return FadeTransition(() => SetSplitScreen(true));
        yield return BlendBackToPlayers();

        SetPlayersInputActive(true);
        _onFinished.Invoke();
    }

    IEnumerator BlendToDoorCamera()
    {
        _doorCamera.gameObject.SetActive(true);
        yield return new WaitForSeconds(GetPlayerBlendDuration());
    }

    IEnumerator BlendBackToPlayers()
    {
        _doorCamera.gameObject.SetActive(false);
        yield return new WaitForSeconds(GetPlayerBlendDuration());
    }

    IEnumerator PlayCinematic()
    {
        _director.Play();

        yield return new WaitForSeconds((float)_director.duration);
    }

    IEnumerator FadeTransition(Action changeWhileBlack)
    {
        yield return _fader.FadeTo(1f, _fadeDuration);
        changeWhileBlack();
        yield return _fader.FadeTo(0f, _fadeDuration);
    }

    static void SetSplitScreen(bool split)
    {
        foreach (PlayerInput player in PlayerInput.all)
        {
            if (player.playerIndex != MainPlayerIndex && player.camera != null)
            {
                player.camera.enabled = split;
            }
        }

        if (PlayerInputManager.instance != null)
        {
            PlayerInputManager.instance.splitScreen = split;
        }
    }

    static void SetPlayersInputActive(bool active)
    {
        foreach (PlayerInput player in PlayerInput.all)
        {
            if (active) player.ActivateInput();
            else player.DeactivateInput();
        }
    }

    static float GetPlayerBlendDuration()
    {
        foreach (PlayerInput player in PlayerInput.all)
        {
            if (player.camera != null && player.camera.TryGetComponent(out CinemachineBrain brain))
            {
                return brain.DefaultBlend.BlendTime;
            }
        }
        return 0f;
    }
}