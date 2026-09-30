using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

// Isole la caméra de chaque joueur en split screen.
public class PlayerCameraSetup : MonoBehaviour
{
    [SerializeField] CinemachineBrain _brain;     
    [SerializeField] CinemachineCamera _cinemachineCamera;

    void Awake()
    {
        // Numéro attribué par le PlayerInputManager 
        int playerIndex = GetComponent<PlayerInput>().playerIndex;
        OutputChannels myChannel = GetChannelForPlayer(playerIndex);

        _cinemachineCamera.OutputChannel = myChannel;
        //le Brain n'écoute que ce channel et ignore les caméras des autres joueurs.
        _brain.ChannelMask = myChannel;
    }

    // Un channel différent par joueur.
    OutputChannels GetChannelForPlayer(int playerIndex)
    {
        OutputChannels[] channels =
        {
            OutputChannels.Channel01,
            OutputChannels.Channel02
        };
        return channels[playerIndex];
    }
}