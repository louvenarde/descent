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
    private RectTransform velocityDirectionIndicator;

    [SerializeField]
    private TextMeshProUGUI playerProfileDebugText;

    void Update()
    {
        PlayerController controller = FindObjectOfType<PlayerController>();
        canvas.enabled = controller;

        if (controller && canvas.enabled)
        {
            innerSideIndicator.localScale =
                Vector3.up +
                Vector3.forward +
                Vector3.right * (controller.IsGoofy ? -1 : 1);

            float stableSpeedAmount = controller.GetStableSpeedAmount();

            speedText.text = controller.GetSpeedKPH().ToString("n0") + " km/h";
            speedTextPercent.text = (stableSpeedAmount * 100).ToString("n0") + "%";
            speedMultiplierText.text = "x1.00"; // TODO

            speedGauge.fillAmount = stableSpeedAmount;

            fallGauge.fillAmount = 0f; // TODO

            float angle = Vector3.Angle(controller.transform.forward, controller.GetSpeedDirection());
            velocityDirectionIndicator.localEulerAngles = Vector3.forward * angle;

            frontStick.rectTransform.localPosition =
                Vector2.Scale(controller.GetFrontFootInput(), frontStick.rectTransform.sizeDelta) * 0.5f;

            frontStickDelayed.rectTransform.localPosition =
                Vector2.Scale(controller.GetFrontFootDelayedInput(), frontStickDelayed.rectTransform.sizeDelta) * 0.5f;

            backStick.rectTransform.localPosition =
                Vector2.Scale(controller.GetBackFootInput(), backStick.rectTransform.sizeDelta) * 0.5f;

            backStickDelayed.rectTransform.localPosition =
                Vector2.Scale(controller.GetBackFootDelayedInput(), backStickDelayed.rectTransform.sizeDelta) * 0.5f;

            playerProfileDebugText.text = controller.HumanoidSheet.name + "\n" + controller.RiderSheet.name;
        }
    }

}
