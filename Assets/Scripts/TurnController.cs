using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnController : MonoBehaviour {

	// Use this for initialization
	void Start () {
        hexMap = GameObject.FindObjectOfType<HexMap>();
        selectionController = GameObject.FindObjectOfType<SelectionController>();
	}

    HexMap hexMap;
    SelectionController selectionController;

    public GameObject EndTurnButton;
    public GameObject NextUnitButton;

    public delegate void TurnEndBusyChangeDelegate ( bool turnEndBusy );
    public event TurnEndBusyChangeDelegate TurnEndBusyChange;

    private bool _turnEndBusy = false;
    public bool TurnEndBusy {
        get {
            return _turnEndBusy;
        }
        protected set {
            if(_turnEndBusy == value) {
                return;
            } else {
                _turnEndBusy = value;
                if(TurnEndBusyChange != null) {
                    TurnEndBusyChange(value);
                }
            }
        }
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            RequestTurnEnd();
        }
        
        // Is the current player an AI?
        // If so, instruct AI to do its move.
        if(hexMap.CurrentPlayer.Type == Player.PlayerType.AI)
        {
            // Call AI logic function whatever here.
            hexMap.AdvanceToNextPlayer();
            return;
        }

        // Which button should be visible in the bottom-right?
        // Are any units waiting for commands?
        Unit[] units = hexMap.CurrentPlayer.Units;
        EndTurnButton.SetActive(!TurnEndBusy);
        NextUnitButton.SetActive(false);
        foreach(Unit u in units)
        {
            if(u.UnitWaitingForOrders())
            {
                EndTurnButton.SetActive(false);
                NextUnitButton.SetActive(true);
                break;
            }
        }

        // Is a city waiting with an empty production queue?
    }

    public void MoveUnitDuringTurn(Unit unit) {
        if (TurnEndBusy) {
            return;
        }

        TurnEndBusy = true;
        StartCoroutine(DoUnitMoves(unit, (finished) => {
            TurnEndBusy = false;
        }));
    }

    public void RequestTurnEnd() {
        Unit[] units = hexMap.CurrentPlayer.Units;

        if (TurnEndBusy) {
            return;
        }
        
        // Now are any units waiting for orders? If so, halt
        foreach(Unit u in units)
        {
            if(u.UnitWaitingForOrders())
            {
                // Select the unit
                selectionController.SelectedUnit = u;

                // Stop processing the end of turn
                return;
            }
        }

        TurnEndBusy = true;
        StartCoroutine(PrepareForTurnEnd((finished) => {
            if(finished) {
                EndTurn();
            }
            TurnEndBusy = false;
        }));
    }

    private IEnumerator PrepareForTurnEnd(Action<bool> callback) {
        Unit[] units = hexMap.CurrentPlayer.Units;

        foreach(Unit u in units)
        {
            yield return DoUnitMoves(u, (finished) => {});
        }
        callback(true);
    }

    
    private IEnumerator DoUnitMoves(Unit u, Action<bool> callback)
    {
        // Is there any reason we should check HERE if a unit should be moving?
        // I think the answer is no -- DoMove should just check to see if it needs
        // to do anything, or just return immediately.
        while( u.DoMove() )
        {
            Debug.Log("DoMove returned true -- will be called again.");
            // TODO: Check to see if an animation is playing, if so
            // wait for it to finish. 
            while(u.AnimationIsPlaying) {
                yield return null; // Wait one frame
            }

        }

        callback(true);
    }

    private void EndTurn()
    {
        Debug.Log("EndTurn");
        Unit[] units = hexMap.CurrentPlayer.Units;
        City[] cities = hexMap.CurrentPlayer.Cities;
        SurfaceStructure[] structures = hexMap.CurrentPlayer.Structures;

        // Heal units that are resting

        // Reset unit movement
        foreach(Unit u in units)
        {
            u.RefreshMovement();
        }

        // If we get to this point, no units are waiting for orders, so process cities

        foreach(City c in cities)
        {
            c.DoTurn();
        }

        foreach(SurfaceStructure structure in structures)
        {
            structure.DoTurn();
        }


        // Go to next player
        selectionController.SelectedUnit = null;
        selectionController.SelectedCity = null;
        hexMap.AdvanceToNextPlayer();

    }
}
