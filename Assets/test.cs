using UnityEngine;
using UnityEngine.InputSystem;

// C : convertir l'ennemi   T : envoyer les alliés au levier   Y : annuler les ordres
public class TestConvert : MonoBehaviour
{
    [SerializeField] GameObject _enemy;
    [SerializeField] InteractInterface _target;
    [SerializeField] bool _useMagicalGirl;   // décoché = nécromancien

    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            PlayerCommander commander = GetCommander();
            if (commander == null || _enemy == null) return;

            PNJAlly ally = commander.Convert(_enemy);
            Debug.Log("Converti : " + ally.name + " -> owner = " + ally.Owner.name);
        }

        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            PlayerCommander commander = GetCommander();
            if (commander == null) return;

            Debug.Log("Envoi de " + commander.Allies.Count + " allié(s) vers " + _target.name);
            commander.SendAllyTo(_target);
        }

        if (Keyboard.current.yKey.wasPressedThisFrame)
        {
            PlayerCommander commander = GetCommander();
            if (commander == null) return;

            foreach (PNJAlly ally in commander.Allies)
                ally.CancelOrder();
            Debug.Log("Ordres annulés");
        }
    }

    PlayerCommander GetCommander()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("Pas de GameManager dans la scène.");
            return null;
        }

        GameObject player = _useMagicalGirl ? GameManager.Instance.MagicalGirl
                                            : GameManager.Instance.Necromancian;
        if (player == null)
        {
            Debug.LogWarning("Joueur pas encore apparu (lance depuis le menu de sélection ?).");
            return null;
        }

        PlayerCommander commander = player.GetComponentInChildren<PlayerCommander>();
        if (commander == null)
            Debug.LogWarning("Pas de PlayerCommander sur " + player.name);
        return commander;
    }
}