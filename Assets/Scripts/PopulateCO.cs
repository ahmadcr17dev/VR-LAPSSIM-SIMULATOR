using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class PopulateCO : MonoBehaviour
{
    void Start()
    {
        TMP_Dropdown dropdown = GetComponent<TMP_Dropdown>();

        dropdown.ClearOptions();

        List<string> options = new List<string>();
        for (int i = 1; i <= 15; i++)
        {
            options.Add(i.ToString());
        }

        dropdown.AddOptions(options);
        dropdown.value = 0; // default = 1
        dropdown.RefreshShownValue();
    }
}