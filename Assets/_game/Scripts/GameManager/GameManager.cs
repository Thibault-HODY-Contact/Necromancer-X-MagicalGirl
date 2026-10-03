using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    private GameObject _necromancian;
    private GameObject _magicalGirl;

    private PlayerInput _necromancianInput;
    private PlayerInput _magicalGirlInput;

    public static GameManager Instance => _instance;

    public GameObject Necromancian
    {
        get => _necromancian;
        set => _necromancian = value;
    }

    public GameObject MagicalGirl
    {
        get => _magicalGirl;
        set => _magicalGirl = value;
    }

    public PlayerInput necromancianInput
    {
        get => _necromancianInput;
        set => _necromancianInput = value;
    }

    public PlayerInput magicalGirlInput
    {
        get => _magicalGirlInput;
        set => _magicalGirlInput = value;
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;

        DontDestroyOnLoad(gameObject);
    }
}