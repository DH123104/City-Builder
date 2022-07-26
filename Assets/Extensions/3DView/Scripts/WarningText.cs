using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WarningText : MonoBehaviour {

    public string warningName;

    public Toggle showToggle;

    void Start()
    {
        if(warningName != null && showToggle != null)
            showToggle.isOn = bool.Parse(PlayerPrefs.GetString(warningName, "true"));

        if (!showToggle.isOn)
            Close();
    }

	public void Close()
    {
        if (warningName != null && showToggle != null)
            PlayerPrefs.SetString(warningName, showToggle.isOn.ToString());

        gameObject.SetActive(false);
    }
}

