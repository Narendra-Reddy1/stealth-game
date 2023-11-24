using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamerasHandler : MonoBehaviour
{
    [SerializeField] private GameObject _exitFocusedCamera;
    [SerializeField] private GameObject _topdownCamera;
    [SerializeField] private GameObject _playerCamera;

    private WaitForSeconds _cameraDelay = new WaitForSeconds(3f);

    private void OnEnable()
    {
        GlobalEventHandler.OnGameStartRequested += OnGameStartRequested;
    }
    private void OnDisable()
    {
        GlobalEventHandler.OnGameStartRequested -= OnGameStartRequested;
    }

    private void OnGameStartRequested()
    {
        StartCoroutine(ShowCameraMovement());
    }
    private IEnumerator ShowCameraMovement()
    {
          yield return new WaitForSeconds(1.5f);
        _exitFocusedCamera.SetActive(false);
        yield return _cameraDelay;
        _topdownCamera.SetActive(false);
        yield return _cameraDelay;
        GlobalEventHandler.OnGameStarted?.Invoke();
    }
}
