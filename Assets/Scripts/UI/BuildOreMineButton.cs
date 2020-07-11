using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildOreMineButton : MonoBehaviour {

    public void BuildOreMine()
    {
        OreMine oreMine = new OreMine();

        HexMap map = GameObject.FindObjectOfType<HexMap>();
        SelectionController sc = GameObject.FindObjectOfType<SelectionController>();

        map.SpawnStructureAt(
            oreMine,
            map.OreMinePrefab,
            sc.SelectedUnit.Hex.Q,
            sc.SelectedUnit.Hex.R,
            map.CurrentPlayer
        );
    }
	
}
