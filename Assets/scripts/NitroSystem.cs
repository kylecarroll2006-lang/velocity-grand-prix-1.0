using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NitroSystem : MonoBehaviour
{
    [Header("References")]
    public ArcadeCarController carController;

    [Tooltip("Shows progress toward earning the next boost.")]
    public Slider chargeBar;

    [Tooltip("Optional text such as BOOST 2 / 3.")]
    public TextMeshProUGUI boostCountText;

    [Tooltip("Three UI Images used as boost boxes.")]
    public Image[] boostBoxes;

    [Header("Boost Box Colors")]
    public Color filledColor = Color.cyan;
    public Color emptyColor = Color.gray;

    [Header("Boost Charges")]
    public int maximumBoosts = 3;
    public int storedBoosts = 0;

    public float chargeNeededPerBoost = 100f;
    public float currentCharge = 0f;
    public float driftChargeRate = 35f;

    [Header("Boost")]
    public float boostForce = 85f;
    public float boostDuration = 1.25f;

    private float boostTimer;
    private bool wasNitroPressed;

    public bool IsBoosting { get; private set; }

    private void Start()
    {
        storedBoosts = Mathf.Clamp(
            storedBoosts,
            0,
            maximumBoosts
        );

        currentCharge = Mathf.Clamp(
            currentCharge,
            0f,
            chargeNeededPerBoost
        );

        if (chargeBar != null)
        {
            chargeBar.minValue = 0f;
            chargeBar.maxValue = chargeNeededPerBoost;
            chargeBar.value = currentCharge;
        }

        UpdateHUD();
    }

    private void Update()
    {
        if (carController == null)
        {
            IsBoosting = false;
            return;
        }

        ChargeFromDrifting();
        ReadBoostButton();
        UpdateBoostTimer();
        UpdateHUD();
    }

    private void FixedUpdate()
    {
        if (!IsBoosting || carController == null)
        {
            return;
        }

        carController.ApplyNitroForce(boostForce);
    }

    private void ChargeFromDrifting()
    {
        if (!carController.IsDrifting)
        {
            return;
        }

        if (IsBoosting)
        {
            return;
        }

        if (storedBoosts >= maximumBoosts)
        {
            currentCharge = 0f;
            return;
        }

        currentCharge += driftChargeRate * Time.deltaTime;

        while (
            currentCharge >= chargeNeededPerBoost &&
            storedBoosts < maximumBoosts
        )
        {
            currentCharge -= chargeNeededPerBoost;
            storedBoosts++;

            Debug.Log(
                "Boost earned! Stored boosts: " +
                storedBoosts
            );
        }

        if (storedBoosts >= maximumBoosts)
        {
            storedBoosts = maximumBoosts;
            currentCharge = 0f;
        }
    }

    private void ReadBoostButton()
    {
        bool nitroPressed =
            carController.IsNitroPressed;

        bool buttonWasJustPressed =
            nitroPressed && !wasNitroPressed;

        if (
            buttonWasJustPressed &&
            !IsBoosting &&
            storedBoosts > 0
        )
        {
            StartBoost();
        }

        wasNitroPressed = nitroPressed;
    }

    private void StartBoost()
    {
        storedBoosts--;
        boostTimer = boostDuration;
        IsBoosting = true;

        Debug.Log(
            "Boost used! Remaining boosts: " +
            storedBoosts
        );
    }

    private void UpdateBoostTimer()
    {
        if (!IsBoosting)
        {
            return;
        }

        boostTimer -= Time.deltaTime;

        if (boostTimer <= 0f)
        {
            boostTimer = 0f;
            IsBoosting = false;
        }
    }

    private void UpdateHUD()
    {
        if (chargeBar != null)
        {
            chargeBar.value = currentCharge;
        }

        if (boostCountText != null)
        {
            boostCountText.text =
                "BOOST " +
                storedBoosts +
                " / " +
                maximumBoosts;
        }

        if (boostBoxes == null)
        {
            return;
        }

        for (int i = 0; i < boostBoxes.Length; i++)
        {
            if (boostBoxes[i] == null)
            {
                continue;
            }

            boostBoxes[i].color =
                i < storedBoosts
                    ? filledColor
                    : emptyColor;
        }
    }
}