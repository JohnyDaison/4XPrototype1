using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UnitNameplate : MonoBehaviour, IPointerClickHandler {

    public Unit MyUnit;

    public void OnPointerClick(PointerEventData eventData)
    {
        //MapObjectNamePlate monp = GetComponent<MapObjectNamePlate>();

        GameObject.FindObjectOfType<SelectionController>().SelectedUnit = MyUnit;
    }

    void LateUpdate () {

        Text textComponent = this.GetComponentInChildren<Text>();
        if(MyUnit != null)
        {
            textComponent.text = MyUnit.player.PlayerName;
            textComponent.text = MyUnit.GetNamePlateText();
        }
    }
}
