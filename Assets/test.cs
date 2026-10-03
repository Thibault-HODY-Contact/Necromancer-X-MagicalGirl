using UnityEngine;
using UnityEngine.InputSystem;

// Script de TEST uniquement : envoie un PNJ précis sur un levier précis.
public class TestOrder : MonoBehaviour
{
    [SerializeField] PNJAlly _pnj;
    [SerializeField] PNJAlly _pnj1;

    [SerializeField] InteractInterface _target;

    void Update()
    {
        if (Keyboard.current == null) return;

        // T : envoyer le PNJ au levier
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            bool ok = _pnj.OrderInteract(_target);
            bool ok1 = _pnj1.OrderInteract(_target);

            Debug.Log("OrderInteract -> " + ok);
        }

        // Y : annuler l'ordre (pour tester le retour en Follow)
        if (Keyboard.current.yKey.wasPressedThisFrame)
        {
            _pnj.CancelOrder();
            _pnj1.CancelOrder();

            Debug.Log("CancelOrder");
        }
    }
}