using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelWelcomeUI : MonoBehaviour
{
    [SerializeField] private int LevelNumber;
    [SerializeField] private string LevelPurpose;
    [SerializeField] private Text TextBox;

    private void Start() 
    {
        TextBox.text = $"Level {LevelNumber}: {LevelPurpose}";
    }
}
