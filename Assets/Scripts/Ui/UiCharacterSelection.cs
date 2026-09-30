using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;

public class UiCharacterSelection : MonoBehaviour
{
    [SerializeField] TMP_Text _DeviceJ1;
    [SerializeField] TMP_Text _DeviceJ2;

    [SerializeField] GameObject[] _rootList;

    [SerializeField] ControllerDetector _detector;

    int J1_characterChoose = 0; // -1 = necromancer 1 = MagicalGirl
    int J2_characterChoose = 0; // -1 = necromancer 1 = MagicalGirl

    InputDevice J1Input = null;

    InputDevice J2Input = null;

    bool _j1CanBeReady = false;
    bool _j2CanBeReady = false;


    List<int> _CharacterChoose = new List<int> { 0, 0 };
    public void onLeftButton(int _playerId)
   {
        if (_CharacterChoose[_playerId] == -1) return;

        for (int i = 0; i < _CharacterChoose.Count; i++)
        {
            if (_CharacterChoose[i] == -1 )
            {
                _CharacterChoose[_playerId] = 0;
                return;
            }
        }
        _CharacterChoose[_playerId] = -1;
        
   }
    public void onRightButton(int _playerId)
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
    private void Start()
    {
      
        J1Input = _detector.Keyboard;
        _j1CanBeReady = J1Input != null;

        J2Input = _detector.ManettesList.Count > 0 ? _detector.ManettesList[0] : null;
        _j2CanBeReady = J2Input != null;
    }

    void Update()
    {

        for (int i = 0; i < _CharacterChoose.Count; i++)
        {
            if (_CharacterChoose[i] == 1)
            {
                _rootList[i].transform.position = new Vector3(transform.position.x+500, _rootList[i].transform.position.y, 0);

            } else if(_CharacterChoose[i] == -1)
            {
                _rootList[i].transform.position = new Vector3(transform.position.x + - 500, _rootList[i].transform.position.y, 0);

            }
            else
            {
                _rootList[i].transform.position = new Vector3(transform.position.x + - 46, _rootList[i].transform.position.y, 0);

            }
        }

        if (J1Input != null)
            _DeviceJ1.text = J1Input.displayName;
        if (J2Input != null)
            _DeviceJ2.text = J2Input.displayName;


    }
}
