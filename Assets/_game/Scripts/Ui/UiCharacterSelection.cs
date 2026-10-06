using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class UiCharacterSelection : MonoBehaviour
{
    [SerializeField] TMP_Text _DeviceJ1;
    [SerializeField] TMP_Text _DeviceJ2;

    [SerializeField] GameObject[] _rootList;
    [SerializeField] InputSystemUIInputModule[] _uiModules; // Les différents EventSystem pour que chaque joueur control son ui de séléction


    [SerializeField] ControllerDetector _detector;

    [SerializeField] float _countdownDuration = 3f; 
    [SerializeField] TMP_Text _countdownText;
    [SerializeField] string _gameSceneName = "Game";

    float _countdownTimer;
    bool _gameStarted = false;

    InputDevice[] _playerDevices = new InputDevice[2]; // [0] = J1, [1] = J2

    List<int> _CharacterChoose = new List<int> { 0, 0 }; // -1 = necromancer, 1 = MagicalGirl

    int[] _lastDirection = new int[2];
    private void onLeftButton(int _playerId)
    {
        if (_CharacterChoose[_playerId] == -1) return;

        for (int i = 0; i < _CharacterChoose.Count; i++)
        {
            if (_CharacterChoose[i] == -1)
            {
                _CharacterChoose[_playerId] = 0;
                return;
            }
        }
        _CharacterChoose[_playerId] = -1;
    }

    private void onRightButton(int _playerId)
    {
        if (_CharacterChoose[_playerId] == 1) return;

        for (int i = 0; i < _CharacterChoose.Count; i++)
        {
            if (_CharacterChoose[i] == 1)
            {
                _CharacterChoose[_playerId] = 0;
                return;
            }
        }
        _CharacterChoose[_playerId] = 1;
    }
    void ReadMoveInput(int playerId)
    {
        float x = _uiModules[playerId].move.action.ReadValue<Vector2>().x;
        if(x < 0) x = -1;
        else if(x > 0) x = 1;
        if (x!=0 && x != _lastDirection[playerId])
        {
            if (x < 0)
                onLeftButton(playerId);
            else
                onRightButton(playerId);
            _lastDirection[playerId] = (int)x;
        }
        else if(x == 0)
        {
            _lastDirection[playerId] = 0;
        }


    }

    // On verifie si le joueur peux se mettre en prêt 
    bool HasChosen(int playerId)
    {
        if (_playerDevices[playerId] == null)
            return false; // Pas de périphérique

        if (_CharacterChoose[playerId] == 0)
            return false; // Pas de personnage

        return true;
    }

    public void OnChangeDeviceButton(int playerId)
    {
        List<InputDevice> available = new List<InputDevice>();
        if (_detector.Keyboard != null) available.Add(_detector.Keyboard);
        available.AddRange(_detector.ManettesList);

        // On retire celui déjà pris par l'autre joueur
        int otherPlayerId = 1 - playerId;
        available.Remove(_playerDevices[otherPlayerId]);

        if (available.Count > 0)
        {
            // Position du périphérique actuel dans la liste (-1 si le joueur n'en a pas)
            int currentIndex = available.IndexOf(_playerDevices[playerId]);

            // On passe au suivant
            int nextIndex = currentIndex + 1;

            // Si on dépasse la fin de la liste, on revient au premier
            if (nextIndex >= available.Count)
                nextIndex = 0;

            _playerDevices[playerId] = available[nextIndex];
        }

        RestrictUIToDevice(playerId);
    }
    bool IsFree(InputDevice device)
    {
        if(_playerDevices[0] == device || _playerDevices[1] == device)
            return false;
        else { 
            return true;
        }
    }

    void SetDevice(int playerId, InputDevice device)
    {
        _playerDevices[playerId] = device;
        RestrictUIToDevice(playerId);
    }

    void AutoAssignDevices()
    {
        for (int i = 0; i < _playerDevices.Length; i++)
        {
            if (_playerDevices[i] == null)
            {
                foreach (var manette in _detector.ManettesList)
                {
                    if (IsFree(manette))
                    {
                        SetDevice(i, manette);
                        break;
                    }
                }
                if (_playerDevices[i] == null && _detector.Keyboard != null && IsFree(_detector.Keyboard))
                {
                    SetDevice(i, _detector.Keyboard);
                }
                if (_playerDevices[i] == null && _detector.Keyboard != null && IsFree(_detector.Keyboard))
                {
                    SetDevice(i, _detector.Keyboard);
                }
            }
        }
    }
    void Start()
    {
        for (int i = 0; i < _uiModules.Length; i++)
        {
            // Chaque joueur a sa propre copie des actions UI pour pas écraser les bindings de l'autre joueur
            _uiModules[i].actionsAsset = Instantiate(_uiModules[i].actionsAsset);

        }
        AutoAssignDevices();                                // après la boucle
        _detector.OnManettesChanged += AutoAssignDevices;   // en dernier
    }
    private void OnDestroy()
    {
        _detector.OnManettesChanged -= AutoAssignDevices; ;
    }

    void Update()
    {
        for (int i = 0; i < _playerDevices.Length; i++)
        {
            // Si le périphérique du joueur a été débranché, il le perd
            if (_playerDevices[i] != null && !_playerDevices[i].added)
            {
                _playerDevices[i] = null;
                RestrictUIToDevice(i);
            }
            ReadMoveInput(i);


        }

        if (_playerDevices[0] != null)
            _DeviceJ1.text = _playerDevices[0].displayName;
        else
            _DeviceJ1.text = "Aucun périphérique";

        if (_playerDevices[1] != null)
            _DeviceJ2.text = _playerDevices[1].displayName;
        else
            _DeviceJ2.text = "Aucun périphérique";

        for (int i = 0; i < _CharacterChoose.Count; i++)
        {
            if (_CharacterChoose[i] == 1)
            {
                _rootList[i].transform.position = new Vector3(transform.position.x + 500, _rootList[i].transform.position.y, 0);
            }
            else if (_CharacterChoose[i] == -1)
            {
                _rootList[i].transform.position = new Vector3(transform.position.x - 500, _rootList[i].transform.position.y, 0);
            }
            else
            {
                _rootList[i].transform.position = new Vector3(transform.position.x - 46, _rootList[i].transform.position.y, 0);
            }
        }
        UpdateCountdown();

    }

    void UpdateCountdown()
    {
        if (_gameStarted) return;

        bool bothChose = HasChosen(0) && HasChosen(1);

        // Si un joueur n'est pas prêt, le compte à rebours repart du début
        if (!bothChose)
        {
            _countdownTimer = _countdownDuration;
            _countdownText.text = "";
            return;
        }

        _countdownTimer -= Time.deltaTime;
        _countdownText.text = Mathf.CeilToInt(_countdownTimer).ToString();

        // si après le timer les deux sont toujours prêts, on lance
        if (_countdownTimer <= 0f)
        {
            _gameStarted = true;

            ConnectPlayer.SetPlayers(_playerDevices, _CharacterChoose);
            SceneManager.LoadScene(_gameSceneName);
        }
    }

    void RestrictUIToDevice(int playerId)
    {
        List<InputDevice> devices = new List<InputDevice>();

        if (_playerDevices[playerId] != null) devices.Add(_playerDevices[playerId]);
        if (Mouse.current != null) devices.Add(Mouse.current);

        _uiModules[playerId].actionsAsset.devices = devices.ToArray();
    }
}