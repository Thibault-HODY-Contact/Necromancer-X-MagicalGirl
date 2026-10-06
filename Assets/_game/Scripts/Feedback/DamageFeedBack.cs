using UnityEngine;
using UnityEngine.Events;

public class DamageFeedBack : MonoBehaviour
{
    [SerializeField] LifeSystem _lifeSystem;

    [SerializeField] UnityEvent _onDamageFeedback = new UnityEvent();

    void Start()
    {
        _lifeSystem.OnDamage += OnDamageFeedback;
    }

    void OnDamageFeedback(int PreviousHealth, int NewHealth)
    {
        _onDamageFeedback.Invoke();
    }
}
