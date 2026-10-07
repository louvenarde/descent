using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class DebugPlayerInfoUI : MonoBehaviour {

    [SerializeField]
    private Canvas canvas;

    [SerializeField]
    private TextMeshProUGUI speedText;

    [SerializeField]
    private TextMeshProUGUI speedTextPercent;

    [SerializeField]
    private Image speedGauge;

    [SerializeField]
    private TextMeshProUGUI speedMultiplierText;

    [SerializeField]
    private Image speedMultiplierGauge;

    [SerializeField]
    private Image frontStick;

    [SerializeField]
    private Image backStick;

    [SerializeField]
    private Image innerSideIndicator;

    [SerializeField]
    private Image velocityDirectionIndicator;

    [SerializeField]
    private TextMeshProUGUI playerProfileDebugText;

    void Update()
    {
        PlayerController controller = FindObjectOfType<PlayerController>();
        canvas.enabled = controller;

        if (controller && canvas.enabled)
        {

        }
    }

}
