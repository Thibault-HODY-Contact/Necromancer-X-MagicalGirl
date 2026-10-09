using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCommander : MonoBehaviour
{

    [SerializeField] StateGraphAsset _allyStateGraph;
    List<PNJAlly> _allies = new List<PNJAlly>();


    public List<PNJAlly> Allies
    {
        get { return _allies; }

        private set { _allies = value; }
    }

    public void SendAllyTo(InteractInterface target) { 
    
        foreach (PNJAlly ally in Allies)
        {
            if (!ally.CurrentTarget)
            { 
                bool ok = ally.OrderInteract(target); 
                Debug.Log("OrderInteract -> " + ok);    
            }
        }
    }

    public PNJAlly Convert(GameObject enemy)
    {

        PNJAlly ally = enemy.GetComponent<PNJAlly>();
       
        if (!ally)
        {
            ally = enemy.AddComponent<PNJAlly>();
        
        }else if ( ally.PlayerCommander != null && ally.PlayerCommander != this)
        {
            ally.PlayerCommander.RemoveAlly(ally);
        }
         

        ally.SetOwner(transform.root.gameObject);
        ally.SetCommander(this);

        if (!_allies.Contains(ally))
        {
            _allies.Add(ally);
        }
        Variables.Object(enemy).Set("HasInteractTarget", false);
        enemy.GetComponent<StateMachine>().nest.SwitchToMacro(_allyStateGraph);
        return ally;
    }

    public void RemoveAlly(PNJAlly ally)
    {
        if (_allies.Contains(ally))
        {
            _allies.Remove(ally);
        }
    }

}
