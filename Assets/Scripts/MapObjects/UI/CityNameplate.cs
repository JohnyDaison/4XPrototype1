using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CityNameplate : MonoBehaviour, IPointerClickHandler {

    public City MyCity;

    public void OnPointerClick(PointerEventData eventData)
    {
        //MapObjectNamePlate monp = GetComponent<MapObjectNamePlate>();

        GameObject.FindObjectOfType<SelectionController>().SelectedCity = MyCity;
    }

    void LateUpdate () {

        Text textComponent = this.GetComponentInChildren<Text>();
        textComponent.text = MyCity.Name;
    }
}
