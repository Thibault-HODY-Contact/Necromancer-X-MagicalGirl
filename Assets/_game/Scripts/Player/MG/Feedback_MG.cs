using UnityEngine;
using UnityEngine.Events;

public class Feedback_MG : MonoBehaviour
{
    [SerializeField] Brain_MG _brain;
    [SerializeField] Animator _animator;

    bool _isAttacking1 = false;
    bool _isAttackingRight = true;

    bool _isAttacking2 = false;


    [SerializeField] UnityEvent _onAttack1Started = new UnityEvent();
    [SerializeField] UnityEvent _onAttack1Stoped = new UnityEvent();

    [SerializeField] UnityEvent _onAttack2Started = new UnityEvent();  
    [SerializeField] UnityEvent _onAttack2Stoped = new UnityEvent();



    void Start()
    {
        _brain.OnAttack1Started += OnAttack1Started;
        _brain.OnAttack1Stoped += OnAttack1Stoped;

        _brain.OnAttack2Started += OnAttack2Started;
        _brain.OnAttack2Stoped += OnAttack2Stoped;
    }

    // Update is called once per frame
    void Update()
    {
        _animator.SetBool("isAttacking1", _isAttacking1);
        _animator.SetBool("isAttackingRight", _isAttackingRight);
    }

    public void StartAttack1()
    {
        _isAttacking1 = true;
        _isAttackingRight = !_isAttackingRight;
    }
    public void StopAttack1()
    {
        _isAttacking1 = false;
    }

    // Events

    void OnAttack1Started()
    {
        _onAttack1Started.Invoke();
    }

    void OnAttack1Stoped()
    {
        _onAttack1Stoped.Invoke();
    }

    void OnAttack2Started()
    {
        _onAttack2Started.Invoke();
    }

    void OnAttack2Stoped()
    {
        _onAttack2Stoped.Invoke();
    }
}
