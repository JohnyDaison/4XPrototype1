using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public UnitTypeDB UnitTypeDB;
    public ResourceTypeDB ResourceTypeDB;
    public static GameController instance;

    public HexMap hexMap;
    public int MinutesPerTurn;
    public int HexDiameter; // in Km
    
    // Start is called before the first frame update
    void Start()
    {
        GameController.instance = this;
        if(UnitTypeDB == null) {
            UnitTypeDB = new UnitTypeDB();
        }
        if(ResourceTypeDB == null) {
            ResourceTypeDB = new ResourceTypeDB();
        }

        hexMap.StartGame();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
