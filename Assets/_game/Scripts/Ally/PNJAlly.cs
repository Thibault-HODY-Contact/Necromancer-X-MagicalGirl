using UnityEngine;
using UnityEngine.AI;
using Unity.VisualScripting;
public class PNJAlly : MonoBehaviour
{
    [SerializeField] NavMeshAgent _navAgent;

    public NavMeshAgent NavAgent
        { get { return _navAgent; } private set { _navAgent = value; } }

    InteractInterface _CurrentTarget;
    public InteractInterface CurrentTarget
    { get { return _CurrentTarget; } private set { _CurrentTarget = value; } }

    Transform _TargetPoint;
    public Transform TargetPoint
    { get { return _TargetPoint; } private set { _TargetPoint = value; } }

    void Start()
    {
        
    }
    void Update()
    {
        
    }
    private void OnDisable()
    {
        if (CurrentTarget != null)
        {
            CurrentTarget.CancelReservation(this);
        }
        CurrentTarget = null;
        TargetPoint = null;
    }

    public void InteractionFinished() 
    {
        CancelOrder();
    }

    public bool OrderInteract(InteractInterface target)
    {
        if(target == null) return false;
        if(CurrentTarget == target) return true;

        if(target.TryReserve(this, out Transform point))
        {
            TargetPoint = point;    

            if( CurrentTarget != null)
            {
                CurrentTarget.CancelReservation(this);
            }
            CurrentTarget = target;
            Variables.Object(gameObject).Set("HasInteractTarget", true);
            return true;

        }
        return false;
    }

    public void CancelOrder()
    {
        if (CurrentTarget != null)
        {
            CurrentTarget.CancelReservation(this);
        }
        Variables.Object(gameObject).Set("HasInteractTarget", false); 
        CurrentTarget = null;
        TargetPoint = null;
    }

}
