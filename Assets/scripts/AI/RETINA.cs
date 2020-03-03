using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RETINA : MonoBehaviour {
    
    [SerializeField]
    GameObject EYEprefab;
    GameObject[] EYES;
    

    public void Init() {
        EYES = new GameObject[AI.IN];

        for (int i = 0; i < AI.IN/20; i++)
        {
            for(int j = 0; j < 20; j++)
            {
                EYES[i*20+j] = Instantiate(EYEprefab, new Vector2(-250 + i * 55, 525 - j * 55),Quaternion.identity);
                EYES[i * 20 + j].transform.parent = gameObject.transform;
            }
        }
        
	}
	

    public bool[] Look()
    {
        bool[] VI = new bool[EYES.Length];
        for (int i = 0; i <EYES.Length ; i++)
        {
            if (EYES[i].GetComponent<EYEcollider>().active == true)
            {
                VI[i] = true; 
            }
            else { VI[i] = false; }
            EYES[i].GetComponent<EYEcollider>().active = false;
            
        }

            return VI;
    }
}
