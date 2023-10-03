using TMPro;
using UnityEngine;

public class KeyValueText : MonoBehaviour 
{
    TextMeshProUGUI labelText;
    TextMeshProUGUI valueText;

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