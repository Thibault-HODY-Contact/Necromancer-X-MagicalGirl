using UnityEditor;
using UnityEngine;

public class ConvertSystem : MonoBehaviour
{

    [SerializeField] GameObject _ModelNEC_ally;
    [SerializeField] GameObject _ModelMG_ally;

    void Start()
    {
    }
    public void onDeathConvert(GameObject Killer )
    {
        Debug.Log(Killer.name);

        if (Killer  && Killer == GameManager.Instance.Necromancian )
        {
            Transform actualTransform = transform.root;
            GameObject NewModel = Instantiate(_ModelNEC_ally, actualTransform.position , actualTransform.rotation);
            GameManager.Instance.Necromancian.GetComponentInChildren<PlayerCommander>().Convert(NewModel);
            Destroy(transform.root.gameObject);
        }
        else if (Killer != null && Killer == GameManager.Instance.MagicalGirl )
        {
            Transform actualTransform = transform.root;
            GameObject NewModel = Instantiate(_ModelMG_ally, actualTransform.position, actualTransform.rotation);
            GameManager.Instance.MagicalGirl.GetComponentInChildren<PlayerCommander>().Convert(NewModel);

            Destroy(transform.root.gameObject);

        }
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }
}
