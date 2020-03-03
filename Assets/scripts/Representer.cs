using System;
using System.Collections.Generic;
using UnityEngine;

public class Representer : MonoBehaviour {


    [SerializeField]
	GameObject Neuronprefab;
    [SerializeField]
    int coloumn = 0;
    GameObject[] INEURONS;
    GameObject[,] SPN;

	public void Init() {
        INEURONS = new GameObject[AI.IN];
        SPN = new GameObject[AI.NC , AI.NNC];
        for(int i=0; i< AI.IN / 20; i++)
        {
            for(int j = 0; j < 20; j++)
            {
                INEURONS[i * 20 + j]= Instantiate(Neuronprefab, new Vector3(-250 + i * 55, 525 - j * 55,-100), Quaternion.identity);
                INEURONS[i * 20 + j].transform.parent = gameObject.transform;
            }
        }
        for(int i = 0; i <  AI.NC/10; i++)
        {
            for(int j = 0; j < 10; j++)
            {
                for(int k = 0; k < AI.NNC; k++)
                {
                    SPN[i*10+j,k]= Instantiate(Neuronprefab, new Vector3(-250+ i * 55, 200 - j * 55, -350-k*40), Quaternion.identity);
                    SPN[i * 10 + j,k].transform.parent = gameObject.transform;
                }
            }
        }
	}


    public void Represent(bool[] VI, bool[,,] spn, bool[,,] Prespn, int[,,] SPPC)
    {
        for (int i = 0; i < INEURONS.Length; i++)
        {
            INEURONS[i].GetComponent<Renderer>().material.color = Color.white;
        }
        for(int i = 0; i < SPN.GetLength(0); i++)
        {
            for(int j = 0; j < SPN.GetLength(1); j++)
            {
                SPN[i, j].GetComponent<Renderer>().material.color = Color.white;
            }
        }
         

        for (int i = 0; i < VI.Length; i++)
        {
            if (VI[i] == true)
            {
                INEURONS[i].GetComponent<Renderer>().material.color = Color.blue;
            }
        }

        for (int i = 0; i < AI.NC; i++)
        {
            for(int j = 0; j < AI.NNC; j++)
            {
                if (spn[i, j, 0] == true)
                {
                    SPN[i, j].GetComponent<Renderer>().material.color = Color.blue;
                    for (int k = 0; k < SPPC.GetLength(1); k++)
                    {

                        if (SPPC[i, k, 0] < AI.IN && SPPC[i, k, 0] >= 0 && SPPC[i, k, 1] > AI.PCT)
                        {
                            INEURONS[SPPC[i, k, 0]].GetComponent<Renderer>().material.color = Color.green;
                        }

                    }

                }
                if (spn[i, j, 1] == true)
                {
                    SPN[i, j].GetComponent<Renderer>().material.color = Color.red;

                }
                if (spn[i, j, 0] == true&& spn[i, j, 1] == true)
                {
                    SPN[i, j].GetComponent<Renderer>().material.color = Color.yellow;
                    

                }

            }
        }
        for (int k = 0; k < SPPC.GetLength(1); k++)
        {

            if (SPPC[coloumn, k, 0] < AI.IN && SPPC[coloumn, k, 0] >= 0 && SPPC[coloumn, k, 1] > AI.PCT)
            {
                INEURONS[SPPC[coloumn, k, 0]].GetComponent<Renderer>().material.color = Color.black;
            }

        }
    }
}
