using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;


public class AIdatabase : MonoBehaviour
{
    public int savecont = 2000;
    static AIdatabase d;
    public int[,,] SPPC;
    public int[,,,] SPDC;
    public int[,] BIR;
    public int[,,] OPC;


    public void Save()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file;
        if (File.Exists(Application.persistentDataPath + "/playerInfo.dat"))
        {
            file = File.Open(Application.persistentDataPath + "/playerInfo.dat", FileMode.Open);
        }
        else { file = File.Create(Application.persistentDataPath + "/playerInfo.dat"); }

        playerData data = new playerData();

        data.SPPC = SPPC;
        data.SPDC = SPDC;
        data.BIR = BIR;
        data.OPC = OPC;

        bf.Serialize(file, data);
        file.Close();
    }


    public void Load()
    {
        if (File.Exists(Application.persistentDataPath + "/playerInfo.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/playerInfo.dat", FileMode.Open);

            playerData data = (playerData)bf.Deserialize(file);

            SPPC = data.SPPC;
            SPDC = data.SPDC;
            BIR = data.BIR;
            OPC = data.OPC;
        }
    }


    void Start()
    {
        if (d == null)
        {
            DontDestroyOnLoad(gameObject);
            d = this;
        }
        else if (d != this) { Destroy(gameObject); }
        Debug.Log("loaded");
        Load();
    }


    private void Update()
    {
        if (savecont == 0)
        {
            savecont = 2000;
            Save();
            Debug.Log("saved");
        }
        savecont--;
    }
}


[Serializable]
class playerData
{
    public int[,,] SPPC;
    public int[,,,] SPDC;
    public int[,] BIR;
    public int[,,] OPC;
}
