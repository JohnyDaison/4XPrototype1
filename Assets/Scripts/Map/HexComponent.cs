using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexComponent : MonoBehaviour {

    public Hex Hex;
    public HexMap HexMap;

    public float VerticalOffset = 0; // Map objects on this hex should be rendered higher than usual

    public void UpdatePosition()
    {
        this.transform.position = Hex.PositionFromCamera(
            Camera.main.transform.position,
            HexMap.NumRows,
            HexMap.NumColumns
        );
    }

    public float GetVerticalOffset(float x, float z) {
        float result = 0;
        float rayStartY = 3f;
        Vector3 basePos = this.transform.position;
        Vector3 startingPosition = new Vector3(basePos.x + x, basePos.y + rayStartY, basePos.z + z);
        Vector3 direction = this.transform.TransformDirection(Vector3.down);
        RaycastHit hitInfo;
        int layerMask = GameController.instance.LayerIDForHexTiles.value;

        if( Physics.Raycast(startingPosition, direction, out hitInfo, 2 * rayStartY, layerMask) ) {
            result = hitInfo.point.y - basePos.y;
        }

        return result;
    }

}
