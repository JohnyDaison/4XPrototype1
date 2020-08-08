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
    public GameObject StartingTurnText;
    public GameObject EndingTurnText;

    public delegate void TurnStartBusyChangeDelegate ( bool turnStartBusy );
    public event TurnStartBusyChangeDelegate TurnStartBusyChange;

    private bool _turnStartBusy = false;
    public bool TurnStartBusy {
        get {
            return _turnStartBusy;
        }
        protected set {
            if(_turnStartBusy == value) {
                return;
            } else {
                _turnStartBusy = value;
                if(TurnStartBusyChange != null) {
                    TurnStartBusyChange(value);
                }
            }
        }
    }

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

    public delegate void UnitIsMovingChangeDelegate ( bool UnitIsMoving );
    public event UnitIsMovingChangeDelegate UnitIsMovingChange;

    private bool _unitIsMoving = false;
    public bool UnitIsMoving {
        get {
            return _unitIsMoving;
        }
        protected set {
            if(_unitIsMoving == value) {
                return;
            } else {
                _unitIsMoving = value;
                if(UnitIsMovingChange != null) {
                    UnitIsMovingChange(value);
                }
            }
        }
    }

    public bool IsBusy() {
        return TurnStartBusy || TurnEndBusy;
    }

    public void Update()
    {
        StartingTurnText.SetActive(TurnStartBusy);
        EndingTurnText.SetActive(TurnEndBusy);

        // Which button should be visible in the bottom-right?
        EndTurnButton.SetActive(EndingTurnIsAllowed());
        NextUnitButton.SetActive(false);
        
        if(IsBusy()) {
            return;
        }

        // Are any units waiting for commands?
        Unit[] units = hexMap.CurrentPlayer.Units;
        
        foreach (Unit u in units)
        {
            if (u.UnitWaitingForOrders())
            {
                EndTurnButton.SetActive(false);
                NextUnitButton.SetActive(true);
                break;
            }
        }

        // Is a city waiting with an empty production queue?
                
        // Is the current player an AI?
        // If so, instruct AI to do its move.
        if (hexMap.CurrentPlayer.Type == Player.PlayerType.AI)
        {
            EndTurnButton.SetActive(false);
            NextUnitButton.SetActive(false);
            
            // Call AI logic function whatever here.
            RequestTurnEnd();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            RequestTurnEnd();
        }
    }

    public void MoveUnitDuringTurn(Unit unit) {
        if (!MovingUnitIsAllowed()) {
            return;
        }

        UnitIsMoving = true;
        StartCoroutine(DoUnitMoves(unit, (finished) => {
            UnitIsMoving = false;
        }));
    }

    public void RequestTurnEnd() {
        Unit[] units = hexMap.CurrentPlayer.Units;

        if (!EndingTurnIsAllowed()) {
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
                TurnEndBusy = false;
                GoToNextPlayer();
                RequestTurnStart();
            }
            TurnEndBusy = false;
        }));
    }

    private bool EndingTurnIsAllowed()
    {
        return !IsBusy() && !UnitIsMoving;
    }

    private bool MovingUnitIsAllowed()
    {
        return !IsBusy() && !UnitIsMoving;
    }

    private IEnumerator PrepareForTurnStart(Action<bool> callback) {
        Unit[] units = hexMap.CurrentPlayer.Units;

        foreach(Unit u in units)
        {
            yield return DoUnfinishedUnitMoves(u, (finished) => {});
        }
        callback(true);
    }

    private IEnumerator PrepareForTurnEnd(Action<bool> callback) {
        Unit[] units = hexMap.CurrentPlayer.Units;

        foreach(Unit u in units)
        {
            u.autoMove = true;
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

    private IEnumerator DoUnfinishedUnitMoves(Unit u, Action<bool> callback)
    {
        if( u.IsMidActionAtTurnStart() && u.DoMove() )
        {
            Debug.Log("DoMove returned true -- turn start.");
            // TODO: Check to see if an animation is playing, if so
            // wait for it to finish. 
            while(u.AnimationIsPlaying) {
                yield return null; // Wait one frame
            }

        }

        callback(true);
    }

    private void RequestTurnStart() {
        Unit[] units = hexMap.CurrentPlayer.Units;

        TurnStartBusy = true;
        StartCoroutine(PrepareForTurnStart((finished) => {
            if(finished) {
                StartTurn();
                TurnStartBusy = false;
            }
            TurnStartBusy = false;
        }));
    }

    private void StartTurn() {
        Unit[] units = hexMap.CurrentPlayer.Units;
        foreach (Unit u in units)
        {
            u.HandleStructureInteraction();
        }
    }

    private void EndTurn()
    {
        Debug.Log("EndTurn");
        Unit[] units = hexMap.CurrentPlayer.Units;
        City[] cities = hexMap.CurrentPlayer.Cities;
        SurfaceStructure[] structures = hexMap.CurrentPlayer.Structures;

        // Heal units that are resting

        // Reset unit movement
        foreach (Unit u in units)
        {
            u.RefreshMovement();
        }

        // If we get to this point, no units are waiting for orders, so process cities

        foreach (City c in cities)
        {
            c.DoTurn();
        }

        foreach (SurfaceStructure structure in structures)
        {
            structure.DoTurn();
        }
    }

    private void GoToNextPlayer()
    {
        selectionController.SelectedUnit = null;
        selectionController.SelectedCity = null;
        hexMap.AdvanceToNextPlayer();
    }
}
