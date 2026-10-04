using UnityEngine;
using UnityEngine.AI;
using Unity.VisualScripting;
public class PNJAlly : MonoBehaviour
{
    [SerializeField] NavMeshAgent _navAgent;

    GameObject _Owner;
    PlayerCommander _PlayerCommander;

    public GameObject Owner
    { get { return _Owner; }  private set { _Owner = value; } }

    public PlayerCommander PlayerCommander
    { get { return _PlayerCommander; } private set { _PlayerCommander = value; } }

    public NavMeshAgent NavAgent
        { get { return _navAgent; } private set { _navAgent = value; } }

    InteractInterface _CurrentTarget;
    public InteractInterface CurrentTarget
    { get { return _CurrentTarget; } private set { _CurrentTarget = value; } }

    Transform _TargetPoint;
    public Transform TargetPoint
    { get { return _TargetPoint; } private set { _TargetPoint = value; } }

    private void Awake()
    {
        if (_navAgent == null)
        {
            _navAgent = GetComponent<NavMeshAgent>();
        }

    }
    public void SetOwner(GameObject owner)
    {
        Owner = owner;
    }
    public void SetCommander(PlayerCommander commander)
    {
        _PlayerCommander = commander;
    }

    void Start()
    {
        
    }
    void Update()
    {
        
    }
    private void OnEnable()
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
        if(Owner != null && PlayerCommander != null)
        {
            PlayerCommander.RemoveAlly(this);
        }

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
