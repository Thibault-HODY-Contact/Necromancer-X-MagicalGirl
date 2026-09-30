using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class UiCharacterSelection : MonoBehaviour
{
    [SerializeField] TMP_Text _DeviceJ1;
    [SerializeField] TMP_Text _DeviceJ2;

    [SerializeField] GameObject[] _rootList;

    int J1_characterChoose = 0; // -1 = necromancer 1 = MagicalGirl
    int J2_characterChoose = 0; // -1 = necromancer 1 = MagicalGirl

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


    void Start()
    {


    }

    void Update()
    {

        for (int i = 0; i < _CharacterChoose.Count; i++)
        {
            if (_CharacterChoose[i] == 1)
            {
                _rootList[i].transform.position = new Vector3(_rootList[i].transform.parent.transform.position.x+500, _rootList[i].transform.position.y, 0);

            } else if(_CharacterChoose[i] == -1)
            {
                _rootList[i].transform.position = new Vector3(_rootList[i].transform.parent.transform.position.x +- 500, _rootList[i].transform.position.y, 0);

            }
            else
            {
                _rootList[i].transform.position = new Vector3(_rootList[i].transform.parent.transform.position.x +- 46, _rootList[i].transform.position.y, 0);

            }
        }


    }
}
