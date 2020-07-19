using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitView : MonoBehaviour {

    void Start()
    {
        newPosition = this.transform.position;
    }

    public Unit Unit;
    public HexMap HexMap;
    public List<GameObject> parts;
    public List<string> partIds;

    Vector3 newPosition;
    Vector3 currentVelocity;
    float smoothTime = 0.5f;

    public void OnUnitMoved( Hex oldHex, Hex newHex )
    {
        // This GameObject is supposed to be a child of the hex we are
        // standing in. This ensures that we are in the correct place
        // in the hierarchy
        // Our correct position when we aren't moving, is to be at
        // 0,0 local position relative to our parent.

        // TODO: Get rid of VerticalOffset and instead use a raycast to determine correct height
        // on each frame.

        Vector3 oldPosition = oldHex.PositionFromCamera();
        newPosition = newHex.PositionFromCamera();
        currentVelocity = Vector3.zero;

        // TODO:  newPosition's Y component needs to be set from HexComponent's VerticalOffset
        oldPosition.y += oldHex.HexMap.GetHexGO(oldHex).GetComponent<HexComponent>().VerticalOffset;
        newPosition.y += newHex.HexMap.GetHexGO(newHex).GetComponent<HexComponent>().VerticalOffset;
        this.transform.position = oldPosition;

        if( Vector3.Distance(this.transform.position, newPosition) > 2 )
        {
            // This OnUnitMoved is considerably more than the expected move
            // between two adjacent tiles -- it's probably a map seam thing,
            // so just teleport
            this.transform.position = newPosition;
        }
        else {
            // TODO: WE need a better signalling system and/or animation queueing
            Unit.AnimationIsPlaying = true;
            setCargoVisibility(true);
        }
    }

    public bool SetPartVisibility(string partId, bool visible) {
        int index = partIds.IndexOf(partId);
        if(index == -1) {
            Debug.LogWarning($"Part \"{partId}\" not found!");
            return false;
        }

        GameObject part = parts[index];
        if(part == null) {
            Debug.LogWarning($"Part \"{partId}\" is missing its object in Inspector!");
            return false;
        }

        part.SetActive(visible);
        
        return true;
    }

    public bool setCargoVisibility(bool visible) {
        string cargoPartId = Unit.unitType.cargoPartId;
        if(cargoPartId != null) {
            return SetPartVisibility(cargoPartId, visible);
        } else {
            return false;
        }
    }

    void Update()
    {
        // This solves the case when Hex gets wrapped to a new position
        if (this.transform.position != newPosition && !Unit.AnimationIsPlaying) {
            newPosition = Unit.Hex.PositionFromCamera();
            newPosition.y += HexMap.GetHexGO(Unit.Hex).GetComponent<HexComponent>().VerticalOffset;
        }

        this.transform.position = Vector3.SmoothDamp( this.transform.position, newPosition, ref currentVelocity, smoothTime );

        // TODO: Figure out the best way to determine the end of our animation
        if( Vector3.Distance( this.transform.position, newPosition ) < 0.1f )
        {
            Unit.AnimationIsPlaying = false;
            setCargoVisibility(false);
        }
    }

}
