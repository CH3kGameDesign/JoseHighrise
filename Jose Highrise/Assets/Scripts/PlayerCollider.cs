using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollider : MonoBehaviour
{
    public Movement M_player;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay(Collider other)
    {
        M_player.TriggerStay(other);
    }
    private void OnCollisionStay(Collision collision)
    {
        M_player.CollisionStay(collision);
    }
    private void OnTriggerExit(Collider other)
    {
        M_player.TriggerExit(other);
    }
}
