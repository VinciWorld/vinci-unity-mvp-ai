using TMPro;
using UnityEngine;

public class KeyValueTextWithChange : MonoBehaviour 
{
    [SerializeField]
    TextMeshProUGUI labelText;
    [SerializeField]
    TextMeshProUGUI valueText;
    [SerializeField]
    TextMeshProUGUI changeValueText;

    [SerializeField]
    Color changeNeutral;
    [SerializeField]
    Color changeBetter;
    [SerializeField]
    Color changeWorse;

    private void Start() 
    {
        //labelText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        //valueText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
    }

    public void SetKeyAndValue(string label, string value, string changeValue = "", ChangeStatus status = ChangeStatus.Same)
    {
        Color textColor = changeNeutral;
        labelText.text = label;
        valueText.text = value;

        if (status == ChangeStatus.Better)
        {
            textColor = changeBetter;
        }
        else if (status == ChangeStatus.Worse)
        {
            textColor = changeWorse;
        }

        changeValueText.text = changeValue;
        changeValueText.color = textColor;
    }

    public void SetValue(string value, string changeValue = "", ChangeStatus status = ChangeStatus.Same)
    {
        Color textColor = changeNeutral;
        valueText.text = value;

        if(status == ChangeStatus.Better)
        {
            textColor = changeBetter;
        }
        else if(status == ChangeStatus.Worse)
        {
            textColor = changeWorse;
        }

        changeValueText.text = changeValue;
        changeValueText.color = textColor;
    }
}