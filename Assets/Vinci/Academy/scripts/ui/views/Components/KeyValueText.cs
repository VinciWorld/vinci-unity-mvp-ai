using TMPro;
using UnityEngine;

public class KeyValueText : MonoBehaviour 
{
    [SerializeField]
    TextMeshProUGUI labelText;
    [SerializeField]
    TextMeshProUGUI valueText;

    private void Start() 
    {
        //labelText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        //valueText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
    }

    public void SetKeyAndValue(string label, string value)
    {
        labelText.text = label;
        valueText.text = value;
    }

    public void SetValue(string value)
    {
        valueText.text = value;
    }
}