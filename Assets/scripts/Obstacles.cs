using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacles : MonoBehaviour {

    [SerializeField]
    GameObject Obstacle;
    int timestep = 0;
    int timecont = 0;
    
	// Update is called once per frame
	void FixedUpdate () {
        timecont++;
        if (timecont % 20 == 0)
        {
            Obstacle.transform.Translate(new Vector3(50, 0, 0));
            if (timestep > 17) { timestep=0; Obstacle.transform.Translate(new Vector3(-18*50, 0, 0)); }
            
            timestep ++;
        }
	}
}
