using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ConnectPlayer : MonoBehaviour
{
    [SerializeField] PlayerInputManager _playerInputManager;
    [SerializeField] GameObject _necromancerPrefab;
    [SerializeField] GameObject _magicalGirlPrefab;

    [SerializeField] Transform _SpawnNEC ;
    [SerializeField] Transform _SpawnMG;

    // Garder les références pour le changement de scène
    static InputDevice[] s_devices;
    static List<int> s_characters;

    // transmet les périphériques et les personnages choisis par les joueurs 
    public static void SetPlayers(InputDevice[] devices, List<int> characters)
    {
        s_devices = devices;
        s_characters = characters;
    }

    void Start()
    {
        if (s_devices == null)
        {
            if (_playerInputManager != null && _playerInputManager.joiningEnabled
                && _playerInputManager.joinBehavior != PlayerJoinBehavior.JoinPlayersManually)
            {
                return;
            }

            Debug.LogWarning("Aucun joueur choisi : lance le jeu depuis le menu de sélection.");
            return;
        }

        JoinPlayers();
    }

    void JoinPlayers()
    {
        for (int i = 0; i < s_devices.Length; i++)
        {
            // on change le préfab du manager pour faire apparaitre le bon perso du joueur
            if (s_characters[i] == -1)
            {
                _necromancerPrefab.transform.position = _SpawnNEC.position;

                _playerInputManager.playerPrefab = _necromancerPrefab;
            }
            else
            {
                _magicalGirlPrefab.transform.position = _SpawnMG.position;

                _playerInputManager.playerPrefab = _magicalGirlPrefab;

            }
            if (s_devices[i] is Keyboard)
                _playerInputManager.JoinPlayer(i, -1, "Keyboard&Mouse", s_devices[i], Mouse.current);
            else
                _playerInputManager.JoinPlayer(i, -1, "Gamepad", s_devices[i]);
        }
    }
}