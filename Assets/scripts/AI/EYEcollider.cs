using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EYEcollider : MonoBehaviour {

    public bool active = false;

    private void OnTriggerStay2D(Collider2D collision)
    {
        active = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        active = true;
    }

    private void OnTriggereLeave2D(Collider2D collision)
    {
        active = true;
    }

}
