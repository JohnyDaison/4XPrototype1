using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public UnitTypeDB UnitTypeDB;
    public static GameController instance;
    
    // Start is called before the first frame update
    void Start()
    {
        GameController.instance = this;
        if(UnitTypeDB == null) {
            UnitTypeDB = new UnitTypeDB();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
