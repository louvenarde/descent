using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class DebugPlayerInfoUI : MonoBehaviour
{

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
    private Image fallGauge;

    [SerializeField]
    private Image frontStick;

    [SerializeField]
    private Image backStick;

    [SerializeField]
    private Image frontStickDelayed;

    [SerializeField]
    private Image backStickDelayed;

    [SerializeField]
    private RectTransform innerSideIndicator;

    [SerializeField]
    private TextMeshProUGUI velocityAngleIndicator;

    [SerializeField]
    private RectTransform velocityDirectionIndicator;

    [SerializeField]
    private TextMeshProUGUI boardDebugText;

    [SerializeField]
    private TextMeshProUGUI playerProfileDebugText;

    void FixedUpdate()
    {
        PlayerController controller = FindObjectOfType<PlayerController>();
        canvas.enabled = controller;

        if (controller && canvas.enabled)
        {
            innerSideIndicator.localScale =
                Vector3.up +
                Vector3.forward +
                Vector3.right * (controller.IsGoofy ? -1 : 1);

            float stableSpeedAmount = controller.smoothStableSpeedAmount;

            speedText.text = ((float)controller.smoothSpeedKPH).ToString("n0") + " km/h";
            speedTextPercent.text = (stableSpeedAmount * 100).ToString("n0") + "%";
            speedMultiplierText.text = "x" + controller.GetSpeedMultiplierFromWeightDistribution().ToString("n2"); 

            speedGauge.fillAmount = stableSpeedAmount;

			fallGauge.rectTransform.sizeDelta = new Vector2 (
				fallGauge.rectTransform.sizeDelta.x,
				controller.GetFallAmount ()
			);

            float angle = MathsToolkit.SignedAngle(controller.Board.forward, controller.smoothSpeedDirection, -controller.Board.up);
            velocityDirectionIndicator.localEulerAngles = Vector3.forward * angle;
            velocityAngleIndicator.text = angle.ToString("n0") + "°";

            frontStick.rectTransform.localPosition =
                Vector2.Scale(controller.GetFrontFootInput(), frontStick.rectTransform.sizeDelta) * 0.5f;

            frontStickDelayed.rectTransform.localPosition =
                Vector2.Scale(controller.GetFrontFootDelayedInput(), frontStickDelayed.rectTransform.sizeDelta) * 0.5f;

            backStick.rectTransform.localPosition =
                Vector2.Scale(controller.GetBackFootInput(), backStick.rectTransform.sizeDelta) * 0.5f;

            backStickDelayed.rectTransform.localPosition =
                Vector2.Scale(controller.GetBackFootDelayedInput(), backStickDelayed.rectTransform.sizeDelta) * 0.5f;

            boardDebugText.text = controller.GetBoardDump() + "\n" + controller.HumanoidProfile.name + "\n" + controller.RiderProfile.name;
            playerProfileDebugText.text = controller.GetInputDump() ;
        }
    }

}
