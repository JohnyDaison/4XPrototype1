using System.Collections.Generic;
using QPath;
using UnityEngine;

public class Unit : MapObject, IQPathUnit {

    public Unit(UnitType unitType, GameObject prefab = null)
    {
        this.unitType = unitType;
        this.prefab = prefab;

        setUnitName(unitType);
        setUnitMovement(unitType);
        setUnitAbilities(unitType);
        setUnitStorage(unitType);
    }

    public UnitType unitType;
    public GameObject prefab;
    public Player player;

    public float BaseMoveSpeed = 0;
    public float NormalHexesPerTurn = 0;
    public int MinutesRemaining = 0;

    public bool CanBuildCities = false;
    public bool isHillWalker = false;
    public bool isForestWalker = false;
    public bool isFlier = false;

    public bool SkipThisUnit = false;
    public bool Guarding = false;

    public StorageContainer storageContainer = new StorageContainer();


    /// <summary>
    /// List of hexes to walk through (from pathfinder).
    /// NOTE: First item is always the hex we are standing in.
    /// </summary>
    List<Hex> hexPath;
    public bool hexPathValid {get; private set;} = false;

    public enum WaypointMode { ONCE, CYCLE };
    List<Hex> waypoints = new List<Hex>();
    private int currentWaypointIndex = 0;
    public WaypointMode waypointMode = WaypointMode.ONCE;
    public bool autoMove = false;

    private void setUnitName(UnitType type) {
        Name = type.Name;
    }

    private void setUnitMovement(UnitType type) {
        BaseMoveSpeed = type.baseMovementSpeed;
        NormalHexesPerTurn = MyUtils.HexesPerTurn(BaseMoveSpeed);
        MinutesRemaining = GameController.instance.MinutesPerTurn;
    }

    private void setUnitAbilities(UnitType type) {
        CanBuildCities = type.canBuildCities;
        isHillWalker = type.isHillWalker;
        isForestWalker = type.isForestWalker;
        isFlier = type.isFlier;
    }

    private void setUnitStorage(UnitType type) {
        storageContainer.TotalStackCount = type.storageStackCount;
        storageContainer.MaxStackVolume = type.storageStackVolume;
    }

    private void SetPathToHex(Hex targetHex)
    {
        Hex[] pathHexes = QPathClass.FindPath<Hex>(
            Hex.HexMap, 
            this,
            Hex, 
            targetHex, 
            Hex.CostEstimate 
        );
            
        Debug.Log("Got pathfinding path of length: " + pathHexes.Length);

        SetHexPath(pathHexes);
    }

    public void ClearHexPath()
    {
        SkipThisUnit = false;
        this.hexPath = new List<Hex>();
    }

    public void SetHexPath( Hex[] hexArray )
    {
        SkipThisUnit = false;
        Guarding = false;

        if (hexArray == null)
        {
            this.hexPath = null;
        }
        else
        {
            this.hexPath = new List<Hex>(hexArray);
        }
    }

    public Hex[] GetHexPath()
    {
        return (this.hexPath == null ) ? null : this.hexPath.ToArray();
    }

    public int GetHexPathLength()
    {
        return this.hexPath.Count;
    }

    public void AddWaypoint(Hex hex) {
        if (!waypoints.Contains(hex)) {
            waypoints.Add(hex);
        }
    }

    public int GetWaypointCount() {
        return waypoints.Count;
    }

    public void ClearWaypoints() {
        waypoints.Clear();
    }

    public void SwitchWaypointMode() {
        if (waypointMode == WaypointMode.ONCE) {
            waypointMode = WaypointMode.CYCLE;
        } else {
            waypointMode = WaypointMode.ONCE;
        }
    }

    public string GetWaypointModeName() {
        switch (waypointMode) {
            case WaypointMode.ONCE:
                return "ONCE";
            case WaypointMode.CYCLE:
                return "CYCLE";
            default:
                return "ERROR";
        }
    }

    public bool UnitWaitingForOrders()
    {
        if(SkipThisUnit || Guarding)
        {
            return false;
        }

        // Returns true if we have movement left but nothing queued
        if( 
            MinutesRemaining > 0 && 
            (hexPath==null || hexPath.Count==0) 
            // TODO: Maybe we've been told to Fortify/Alert/SkipTurn
        )
        {
            return true;
        }

        return false;
    }

    public bool IsMidActionAtTurnStart() {
        return MinutesRemaining > GameController.instance.MinutesPerTurn;
    }

    public void RefreshMovement()
    {
        SkipThisUnit = false;
        if(IsMidActionAtTurnEnd()) {
            MinutesRemaining += GameController.instance.MinutesPerTurn;
        } else {
            MinutesRemaining = GameController.instance.MinutesPerTurn;
        }
    }

    /// <summary>
    /// Processes one tile worth of movement for the unit
    /// </summary>
    /// <returns>Returns true if this should be called immediately again.</returns>
    public bool DoMove()
    {
        Debug.Log("DoMove");
        // Do queued move

        if (!CheckHexPathValid()) {
            return false;
        }

        if (MinutesRemaining <= 0) {
            return false;
        }

        // Grab the first hex from our queue
        Hex hexWeAreLeaving = hexPath[0];
        Hex newHex = hexPath[1];

        int costToEnter = MovementCostToEnterHex(newHex);
        int minutesCost = MyUtils.MinutesPerHex(BaseMoveSpeed);
        int totalMinutesCost = costToEnter * minutesCost;

        if (totalMinutesCost > MinutesRemaining)
        {
            // We can't enter the hex this turn
            return false;
        }

        hexPath.RemoveAt(0);

        // Move to the new Hex
        SetHex(newHex);
        MinutesRemaining = Mathf.Max(MinutesRemaining - totalMinutesCost, 0);

        CheckHexPathValid();

        return true;
    }

    private bool CheckHexPathValid()
    {
        bool result = true;
        
        if (hexPath == null) {
            result = false;
        } else if (hexPath.Count <= 1) {
            // The only hex left in the list, is the one we are moving to now,
            // therefore we have no more path to follow, so let's just clear
            // the queue completely to avoid confusion.
            hexPath = null;
            result = false;
        }

        if(ShouldGoToNextWaypoint()) {
            HandleWaypoints();
            result = CheckHexPathValid();
        }

        hexPathValid = result;
        return result;
    }

    private bool ShouldGoToNextWaypoint() {
        bool waypointExists = waypoints.Count > 0;
        bool wantNewDestination = autoMove && hexPath == null;
        if (!waypointExists || !wantNewDestination) {
            return false;
        }

        if (currentWaypointIndex > waypoints.Count - 1) {
            currentWaypointIndex = 0;
        }

        Hex currentWaypoint = waypoints[currentWaypointIndex];

        return waypoints.Count > 1 || currentWaypoint != Hex;
    }

    private void HandleWaypoints() {
        if (ShouldGoToNextWaypoint()) {
            Hex currentWaypoint = waypoints[currentWaypointIndex];

            if (currentWaypoint == Hex) {
                switch(waypointMode) {
                    case WaypointMode.ONCE:
                        waypoints.Remove(currentWaypoint);
                        if (waypoints.Count == 0) {
                            return;
                        }
                        break;

                    case WaypointMode.CYCLE:
                        currentWaypointIndex++;
                        break;
                }

                currentWaypointIndex = currentWaypointIndex % waypoints.Count;
            }

            currentWaypoint = waypoints[currentWaypointIndex];

            if (currentWaypoint != Hex) {
                SetPathToHex(currentWaypoint);
            }
        }
    }

    private bool IsMidActionAtTurnEnd() {
        return (hexPath != null && hexPath.Count > 1);
    }

    public int MovementCostToEnterHex( Hex hex )
    {
        return hex.BaseMovementCost( isHillWalker, isForestWalker, isFlier);
    }

    public float AggregateTurnsToEnterHex( Hex hex, float turnsToDate )
    {
        // The issue at hand is that if you are trying to enter a tile
        // with a movement cost greater than your current remaining movement
        // points, this will either result in a cheaper-than expected
        // turn cost (Civ5) or a more-expensive-than expected turn cost (Civ6)
        // or exactly the expected turn cost by continuing the move at the start of next turn (our system)

        int costToEnter = MovementCostToEnterHex(hex);

        if (costToEnter < 0)
        {
            // Impassible terrain
            //Debug.Log("Impassible terrain at:" + hex.ToString());
            return -99999;
        }

        int minutesCost = MyUtils.MinutesPerHex(BaseMoveSpeed);
        int totalMinutesCost = costToEnter * minutesCost;
        int minutesPerTurn = GameController.instance.MinutesPerTurn;

        float turnsToEnterHex = (float)totalMinutesCost / minutesPerTurn;

        // Do we return the number of turns THIS move is going to take?
        // I say no, this an an "aggregate" function, so return the total
        // turn cost of turnsToDate + turns for this move.

        return turnsToDate + turnsToEnterHex;

    }

    override public void SetHex( Hex newHex )
    {
        if(Hex != null)
        {
            Hex.RemoveUnit(this);
        }

        base.SetHex( newHex );

        Hex.AddUnit(this);

        HandleStructureInteraction();
    }

    public void HandleStructureInteraction() {
        if (Hex.SurfaceStructure == null) {
            return;
        }

        SurfaceStructure structure = Hex.SurfaceStructure;

        if (structure.isGeneralStorage) {
            storageContainer.TransferAll(structure.storageContainer);
        }

        if (structure.outputResources.Count > 0) {
            foreach (ResourceType resource in structure.outputResources) {
                structure.storageContainer.TransferResource(resource, storageContainer);
            }
        }
    }

    override public void Destroy(  )
    {
        base.Destroy(  );

        Hex.RemoveUnit(this);
    }

    /// <summary>
    /// Turn cost to enter a hex (i.e. 0.5 turns if a movement cost is 1 and we have 2 max movement)
    /// </summary>
    public float CostToEnterHex( IQPathTile sourceTile, IQPathTile destinationTile )
    {
        return 1;
    }

    public string GetNamePlateText() {
         string text = $"{player.PlayerNumber} | {Name}";
         if (storageContainer.GetTotalVolume() > 0) {
            string cargo = storageContainer.GetOccupiedVolume().ToString("0.00");
            text += $" ({cargo})";
         }
         return text;
    }
}
