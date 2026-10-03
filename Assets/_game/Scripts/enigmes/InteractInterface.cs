using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractInterface : MonoBehaviour
{
    [SerializeField] InputActionReference _Interact;
    [SerializeField] int _numberOfPNJRequiredToInteract;
    [SerializeField] List<Transform> _interactionPointList = new List<Transform>();

    Dictionary<PNJAlly, Transform> _AllyReservationPoint = new Dictionary<PNJAlly, Transform>();
    List<PNJAlly> _AllyArrived = new List<PNJAlly>();

    Dictionary<PlayerInput, InputAction> _PlayerActionDict = new Dictionary<PlayerInput, InputAction>();


    public InputActionReference Interact { get { return _Interact; } }

    virtual public void OnInteract(InputAction.CallbackContext context)
    {

    }

    virtual public void OnPNJInteract()
    {

    }

    private void OnDisable()
    {
       foreach (var action in _PlayerActionDict.Values)
        {
            action.performed -= OnInteract;
        }
        _PlayerActionDict.Clear();
    }
    public bool TryReserve(PNJAlly pnj, out Transform point)
    {
        if (_AllyReservationPoint.ContainsKey(pnj))
        {
            point = _AllyReservationPoint[pnj];
            return true;
        }

        if (_AllyReservationPoint.Count < _numberOfPNJRequiredToInteract)
        {
            foreach (Transform interactionPoint in _interactionPointList)
            {
                if (!_AllyReservationPoint.ContainsValue(interactionPoint))
                {
                    point = interactionPoint;
                    _AllyReservationPoint.Add(pnj, point);
                    return true;
                }
            }
        }
        point = null;
        return false;
    }
    public void PNJArrived(PNJAlly pnj)
    {
        if(_AllyReservationPoint.ContainsKey(pnj) && !_AllyArrived.Contains(pnj))
        {
            _AllyArrived.Add(pnj);
        }
        else
        {
            return;
        }

        if (_AllyArrived.Count >= _numberOfPNJRequiredToInteract)
        {
            OnPNJInteract();
            List<PNJAlly>  templist = new List<PNJAlly>(_AllyArrived);
            foreach (PNJAlly ally in templist)
            {
                ally.InteractionFinished();
                CancelReservation(ally);
            }
        }

    }


    public void CancelReservation(PNJAlly pnj)
    {
        if (_AllyReservationPoint.ContainsKey(pnj))
        {
            _AllyReservationPoint.Remove(pnj);
        }
        if(_AllyArrived.Contains(pnj))
        {
            _AllyArrived.Remove(pnj);
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerInteractor"))
        {
            Debug.Log("enter");

            Debug.Log(other.transform.root.gameObject.name);
            if (other.transform.root.gameObject == GameManager.Instance.Necromancian)
            {
                if (!_PlayerActionDict.ContainsKey(GameManager.Instance.necromancianInput))
                {
                    _PlayerActionDict[GameManager.Instance.necromancianInput] = GameManager.Instance.necromancianInput.actions.FindAction(Interact.action.id);
                    _PlayerActionDict[GameManager.Instance.necromancianInput].performed += OnInteract;

                }

            }
            else if (other.transform.root.gameObject == GameManager.Instance.MagicalGirl)
            {
                if (!_PlayerActionDict.ContainsKey(GameManager.Instance.magicalGirlInput))
                {
                    _PlayerActionDict[GameManager.Instance.magicalGirlInput] = GameManager.Instance.magicalGirlInput.actions.FindAction(Interact.action.id);
                    _PlayerActionDict[GameManager.Instance.magicalGirlInput].performed += OnInteract;
                }

            }
        }
     
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerInteractor"))
        {
            Debug.Log("exit");
            Debug.Log(other.transform.root.gameObject.name);
            if (other.transform.root.gameObject == GameManager.Instance.Necromancian)
            {
                if (_PlayerActionDict.ContainsKey(GameManager.Instance.necromancianInput))
                {
                    _PlayerActionDict[GameManager.Instance.necromancianInput].performed -= OnInteract;
                    _PlayerActionDict.Remove(GameManager.Instance.necromancianInput);
                }
            }
            else if (other.transform.root.gameObject == GameManager.Instance.MagicalGirl)
            {
                if (_PlayerActionDict.ContainsKey(GameManager.Instance.magicalGirlInput))
                {
                    _PlayerActionDict[GameManager.Instance.magicalGirlInput].performed -= OnInteract;
                    _PlayerActionDict.Remove(GameManager.Instance.magicalGirlInput);

                }
            }
        }
    }
}
