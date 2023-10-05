using TMPro;
using UnityEngine;

public class KeyValueText : MonoBehaviour 
{
    TextMeshProUGUI labelText;
    TextMeshProUGUI valueText;

    private void Awake() 
    {
        labelText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        valueText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
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