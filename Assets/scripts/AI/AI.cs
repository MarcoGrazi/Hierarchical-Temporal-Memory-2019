using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour
{
    GameObject Representer;
    GameObject retina;
    //constants: number of neurons in INput,Number of Coloumns,Number of Neurons per Coloumn,number of OUTputs,
    //Prediction Treshold,Proximal Connection Increment,Proximal Connection Decrement,
    //Proximal Connection Treshold,Distal Connection Treshold,Active Coloumns per Timestep;
    public static int IN = 300, NC = 150, NNC = 5, OUT = 5, PCI = 50, PCD = -30,DCI=100,DCD=-20, PCT =200,DCT=200,ACT=(int)(NC*0.07);
    //Input Proximal Connections Ratio, Distal Connections Ratio
    static float IPCR = 0.6f, DCR = 0.6f, PT=NC*NNC*DCR*0.05f;
    //Boost Inhibition Ratio array
    int[,] BIR = new int[NC,2];
    //Spatial Pooler Neurons (0=active state, 1=predictive state)
    public bool[,,] SPN = new bool[NC, NNC, 2];
    // Spatial Pooler Neurons in previous timestep
    public bool[,,] PRESPN = null;
    //Spatial Pooler Proximal Connections
    public int[,,] SPPC = new int[NC, (int)(IN * IPCR), 2];
    //Spatial Pooler Distal Connections
    int[,,,] SPDC = new int[NC, NNC, (int)(NC * NNC * DCR), 3];
    //Output Proximal Connections
    int[,,] OPC = new int[OUT,(int) (NC * IPCR), 2];
    //Visual Input Presence
    bool VIP;
    //Visual Input Array
    public bool[] VI;
    //AIdatabase to store the connections data, with savecont and saveperiod
    AIdatabase d;
    int savecont = 0;
    static int saveperiod = 2000;
    //number of updates from start
    [SerializeField]
    int timestep = 0;



    void Start()
    {
        //finds the gameobjects associated with the visual input and representation. then it initialize the AI arrays
        retina = gameObject.transform.Find("RETINA").gameObject;
        retina.GetComponent<RETINA>().Init();
        Representer = gameObject.transform.Find("AI-Animator").gameObject;
        Representer.GetComponent<Representer>().Init();
        InitAI();
    }


    void InitAI()
    {

        for (int i = 0; i < BIR.GetLength(0); i++)
        {
            BIR[i,0] = 0;
            BIR[i,1] = 0;
        }

        //Initialize Random Proximal connections between SP coloumns and input neurons
        for (int i = 0; i < NC; i++)
        {

                for (int j = 0; j < SPPC.GetLength(1); j++)
                {
                    int c = Random.Range(0, IN - 1);
                    int v = Random.Range(0, 1000);
                    SPPC[i, j, 0] = c;
                    SPPC[i, j, 1] = v;

                }
            
        }

        //Initialize random Distal Connections between SP neurons from different coloumns
        for (int i = 0; i < NC; i++)
        {
            for (int j = 0; j < NNC; j++)
            {
                for (int k = 0; k < SPDC.GetLength(2); k++)
                {
                    int c = Random.Range(0, NC-1);
                    int n = Random.Range(0, NNC-1);
                    int v = Random.Range(0, 1000);
                    SPDC[i, j, k, 0] = c;
                    SPDC[i, j, k, 1] = n;
                    SPDC[i, j, k, 2] = v;

                }
            }
        }

        //retrieve data from the database
        d = gameObject.GetComponent<AIdatabase>();
        if (d.SPPC == null)
        {
            d.SPPC = SPPC;
        }
        else
        {
            SPPC = d.SPPC;
        }

        if (d.SPDC == null)
        {
            d.SPDC = SPDC;
        }
        else
        {
            SPDC = d.SPDC;
        }

        if (d.BIR == null)
        {
            d.BIR = BIR;
        }
        else
        {
            BIR = d.BIR;
        }

        if (d.OPC == null)
        {
            d.OPC = OPC;
        }
        else
        {
            OPC = d.OPC;
        }

    }


    private void FixedUpdate()
    {
        // tunes the refresh rate for the AI thinking activity
        if (timestep % 20 == 0)
        {
            Think();
            Debug.Log("thought");

        }
        if (savecont == 0)
        {
            savecont = saveperiod;
            d.SPPC = SPPC;
            d.SPDC = SPDC;
            d.BIR = BIR;
            d.OPC = OPC;
        }
        savecont--;
        timestep++;
    }


    private void Clean()
    {//Resets the SPN array,so that all neurons are reset to an inactive and not predicting state
        for (int i = 0; i < SPN.GetLength(0); i++)
        {
            for (int j = 0; j < SPN.GetLength(1); j++)
            {
                PRESPN[i, j, 0] = SPN[i,j,0];
                PRESPN[i, j, 1] = SPN[i,j,1];
                SPN[i, j, 0]=false;
                SPN[i, j, 1]=false;
            }

        }
    }
   

    public void Think()
    {//Thinking activity work cycle
        Activate();
        Representer.GetComponent<Representer>().Represent(VI, SPN, PRESPN, SPPC);
        if (PRESPN != null)
        {
            Learn("SP");
        }
        PRESPN = new bool[NC, NNC, 2];
        Clean();

        Predict();


    }


    public void Activate()
    {   
        VI = retina.GetComponent<RETINA>().Look();
        VIP= false;
        for(int i = 0; i < VI.Length; i++)
        {
            if (VI[i] == true)
            {
                VIP = true;
            }
        }
        if (VIP == true)
        {
            float[,] Ovp = new float[NC, 2];
            float somma = 0;
            for (int i = 0; i < NC; i++)
            {
                Ovp[i, 0] = i;
                Ovp[i, 1] = Overlap(i);
                somma += Ovp[i, 1];
            }

            if (BIR[0, 0] > 0)
            {
                for (int i = 0; i < NC; i++)
                {
                    Ovp[i, 1] = Ovp[i, 1] + 10 * (1 - BIR[i, 1] / BIR[i, 0]);
                }
            }

            for (int i = 0; i < NC; i++)
            {
                int max = i;
                for (int j = i; j < NC; j++)
                {
                    if (Ovp[max, 1] < Ovp[j, 1])
                    {
                        max = j;
                    }
                }
                float TC = Ovp[i, 0];
                float TV = Ovp[i, 1];
                Ovp[i, 0] = Ovp[max, 0];
                Ovp[i, 1] = Ovp[max, 1];
                Ovp[max, 0] = TC;
                Ovp[max, 1] = TV;
            }

            for (int i = 0; i < ACT; i++)
            {

                bool p = false;
                for (int j = 0; j < SPN.GetLength(1); j++)
                {
                    if (SPN[(int)Ovp[i, 0], j, 1] == true)
                    {
                        SPN[(int)Ovp[i, 0], j, 0] = true;
                        p = true;
                        Debug.Log("predicted");
                        break;
                       
                    }

                }
                if (p == false)
                {
                    Debug.Log("Bursting");
                    //if PRESPN=null this means that it is the first think cycle and the model has no predicted cell and no context
                    //so the winner cell is chosen randomly
                    if (PRESPN == null)
                    {
                        int W = Random.Range(0, NNC - 1);
                        SPN[(int)Ovp[i, 0], W, 0] = true;

                    }
                    else {
                        //winner cell selection process: it involves finding the one with more connections to preactive cells (maxp,PC)
                        //in case maxp=0, the winner cell will be the one with the least number of active synapsis(mina,AC)
                        int maxp = 0;
                        int mina = 0;
                        int[] PC = new int[NNC];
                        int[] AC = new int[NNC];
                        for (int j = 0; j < NNC; j++)
                        {
                            for (int k = 0; k < SPDC.GetLength(2); k++)
                            {
                                if (PRESPN[SPDC[(int)Ovp[i, 0], j, k, 0], SPDC[(int)Ovp[i, 0], j, k, 1], 0] == true) { PC[j]++; }
                                if (SPDC[(int)Ovp[i, 0], j, k, 2] > DCT) { AC[j]++; }
                            }
                            if (PC[maxp] < PC[j]) { maxp = j; }
                            if (AC[mina] > AC[j]) { mina = j; }
                        }
                        if (PC[maxp] > 0) { SPN[(int)Ovp[i, 0], maxp, 0] = true; }
                        else { SPN[(int)Ovp[i, 0], mina, 0] = true; }

                    }
                }
            }
        }

    }


    public void Learn(string name)
    {
        if (name == "SP")
        {
            //Hebbian Learning for proximal connections
            for (int i = 0; i < SPN.GetLength(0); i++)
            {
                bool act = false;
                for(int j = 0; j < SPN.GetLength(1); j++)
                {
                    if (SPN[i, j, 0] == true)
                    {
                        act = true;
                        break;
                        
                    }
                }
                BIR[i, 0]++;
                if (act == true)
                {
                    BIR[i, 1]++;
                    for (int j = 0; j < SPPC.GetLength(1); j++)
                    {
                        if (VI[SPPC[i, j, 0]] == true)
                        {
                            if (SPPC[i, j, 1] < 1000)
                            {
                                SPPC[i, j, 1] += PCI;
                            }

                        }
                        else
                        {
                            if (SPPC[i, j, 1] > 0)
                            {
                                SPPC[i, j, 1] += PCD;
                            }
                        }
                    }
                }
                    
            }

            //Hebbian learning for distal connections
            int Right = 0;
            int Almost = 0;
            int Redirected = 0;
            int Wrong = 0;
            for(int i = 0; i < NC; i++)
            {
                for(int j = 0; j < NNC; j++)
                {
                    //if the cell is both predicted and active, then its distal connections to preactive cells are incremented
                    //while the other connections are decremanted
                    if(SPN[i,j,0]==true && SPN[i, j, 1] == true)
                    {
                        Right++;
                        for (int k = 0; k < SPDC.GetLength(2); k++)
                        {
                            if (PRESPN[SPDC[i, j, k, 0], SPDC[i, j, k, 1], 0] == true)
                            {
                                if (SPDC[i, j, k, 2] < 1000) { SPDC[i, j, k, 2] += DCI; }
                            }
                            else
                            {
                                if (SPDC[i, j, k, 2] > 0) { SPDC[i, j, k, 2] += DCD; }
                            }
                        }
                    }
                    //if the cell is active but wasn't predicted the distal connections to preactive cells are incremented
                    //while a randomly chosen number of the other connections are redirected to different preactive cells
                    if (SPN[i, j, 0] == true && SPN[i, j, 1] == false)
                    {
                        Almost++;
                        int CCC = 0;
                        int CC = 0;
                        for (int k = 0; k < SPDC.GetLength(2); k++)
                        {
                            if (PRESPN[SPDC[i, j, k, 0], SPDC[i, j, k, 1], 0] == true)
                            {
                                if (SPDC[i, j, k, 2] < 1000) { SPDC[i, j, k, 2] += DCI; }
                            }
                            else
                            {
                                int R = Random.Range(1, 5);
                                if (R == 3)
                                {
                                    Redirected++;
                                    for(int c = 0; c < NC; c++)
                                    {
                                        for (int n = 0; n < NNC; n++)
                                        {
                                            if (PRESPN[c, n, 0] == true && CC==CCC)
                                            {
                                                SPDC[i, j, k, 0] = c;
                                                SPDC[i, j, k, 1] = n;
                                                SPDC[i, j, k, 2] = Random.Range(DCT, 1000);
                                                CCC++;
                                                CC = 0;
                                            }
                                            else if (CC < CCC) { CC++; }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    //if the cell was predited but it has not become active, this means that the coloumn wasn't 
                    //activated so the prediction was wrong and the distal conections to preactive cells are to be decremented
                    if (SPN[i, j, 0] == false && SPN[i, j, 1] == true)
                    {
                        Wrong++;
                        for (int k = 0; k < SPDC.GetLength(2); k++)
                        {
                            if (PRESPN[SPDC[i, j, k, 0], SPDC[i, j, k, 1], 0] == true)
                            {
                                Redirected++;
                                SPDC[i, j, k, 0] = Random.Range(0, NC - 1);
                                SPDC[i, j, k, 1] = Random.Range(0, NNC - 1);
                                SPDC[i, j, k, 2] = Random.Range(0, 1000);
                            }
                        }
                    }
                }
            }

            Debug.Log(timestep + ": Right= " + Right + " Almost= " + Almost + " Wrong= " + Wrong + " Redirected= " + Redirected);


        }
    }
    

    public void Predict()
    {
        for (int i = 0; i < NC; i++)
        {

            for (int j = 0; j < NNC; j++)
            {
                int DIST = 0;
                for (int k = 0; k < SPDC.GetLength(2); k++)
                {
                    if (PRESPN[SPDC[i, j, k, 0], SPDC[i, j, k, 1], 0] == true && SPDC[i, j, k, 2] > DCT) { DIST++; };
                }
                
                if (DIST > PT)
                {
                    SPN[i, j, 1] = true;
                    break;
                }
            }

        }

    }


    private float Overlap(int name)
    {
        /*calculates the overlap value by counting the number of connections to one of the active input neurons that 
         * exceed the proximal connection treshold (PCT) */
        float overlap = 0;
        for (int i = 0; i < SPPC.GetLength(1); i++)
        {
            if (VI[SPPC[name,i,0]]==true && SPPC[name, i, 1] >= PCT) { overlap += 1; }

        }
        return overlap;
    }













}


