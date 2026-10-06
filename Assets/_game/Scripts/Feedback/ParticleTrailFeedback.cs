using UnityEngine;

[DefaultExecutionOrder(100)]
public class ParticleTrailFeedback : MonoBehaviour
{
    [SerializeField] ParticleSystem _particlePrefab;

    ParticleSystem _particles;

    void OnEnable()
    {
        if (_particlePrefab == null) return;

        _particles = Instantiate(_particlePrefab, transform.position, transform.rotation);
    }

    void LateUpdate()
    {
        if (_particles == null) return;

        _particles.transform.SetPositionAndRotation(transform.position, transform.rotation);
    }

    void OnDisable()
    {
        if (_particles == null) return;

        ParticleSystem.MainModule main = _particles.main;
        main.stopAction = ParticleSystemStopAction.Destroy;
        _particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        _particles = null;
    }
}
