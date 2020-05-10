using UnityEngine;

public class CameraMotion : MonoBehaviour {

    float moveSpeed = 3.5f;
    float panStopDistance = 0.1f;

    Transform panTarget;
    Vector3 targetPanningPos;

    // Use this for initialization
	void Start () {
        oldPosition = this.transform.position;
	}

    Vector3 oldPosition;
	
	// Update is called once per frame
	void Update () {
		
        // TODO: Code to click-and-drag camera
        //          WASD
        //          Zoom in and out

        if(panTarget != null) {
            DoPanningStep();

            if(Vector3.Distance(this.transform.position, GetFinalPanningPos()) <= panStopDistance)
            {
                panTarget = null;
            }
        }

        CheckIfCameraMoved();
	}

    public void PanToHex( Hex hex )
    {
        panTarget = hex.HexMap.GetHexGO(hex).transform;
        targetPanningPos = GetFinalPanningPos();
    }


    Vector3 GetFinalPanningPos() {
        float coefficient = Mathf.Abs(this.transform.position.y / this.transform.forward.y);
        Vector3 offset = Vector3.Project(coefficient * this.transform.forward, new Vector3(0f, 0f , 1f));

        Vector3 result = panTarget.transform.position - offset;
        result.y = 5;

        return result;
    }

    void DoPanningStep() {
        Vector3 translate = GetFinalPanningPos() - this.transform.position;
        translate.Normalize();

        this.transform.Translate( translate * moveSpeed * Time.deltaTime * (1 + this.transform.position.y / 2), Space.World);
    }

    HexComponent[] hexes;

    void CheckIfCameraMoved()
    {
        if(oldPosition != this.transform.position)
        {
            // SOMETHING moved the camera.
            oldPosition = this.transform.position;

            // TODO: Probably HexMap will have a dictionary of all these later
            if(hexes == null)
                hexes = GameObject.FindObjectsOfType<HexComponent>();

            // TODO: Maybe there's a better way to cull what hexes get updated?

            foreach(HexComponent hex in hexes)
            {
                hex.UpdatePosition();
            }
        }
    }
}
