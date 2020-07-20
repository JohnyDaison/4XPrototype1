using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StructureNameplate : MonoBehaviour, IPointerClickHandler {

    public SurfaceStructure MyStructure;

    public void OnPointerClick(PointerEventData eventData)
    {
        //MapObjectNamePlate monp = GetComponent<MapObjectNamePlate>();

        //GameObject.FindObjectOfType<SelectionController>().SelectedStructure = MyStructure;
    }

    void LateUpdate () {
        Text textComponent = this.GetComponentInChildren<Text>();    
        textComponent.text = MyStructure.GetNamePlateText();
    }
}
