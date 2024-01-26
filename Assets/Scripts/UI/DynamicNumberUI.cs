using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// This is a testing class to update the score text on the player's UI for the Package Scene. This is all the commenting I'm going to bother to do for this class and it already took me longer to write this comment than to write the script.
/// </summary>
public class DynamicNumberUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI waveText;
    [SerializeField] TextMeshProUGUI centerText;
    [SerializeField] TextMeshProUGUI finalOrderText;
    [SerializeField] TextMeshProUGUI placementText;
    private OrderHandler player;

    private void Start()
    {
        player = GetComponent<OrderHandler>();
    }

    private void Update()
    {
        // I figure in the future the placements will be individual sprites in an array or something so I didn't bother adding "st","nd","rd", etc...
        placementText.text = "" + player.Placement;
        if (OrderManager.Instance != null)
        {
            if (!OrderManager.Instance.FinalOrderActive)
            {
                if (OrderManager.Instance.Wave + 1 == OrderManager.Instance.MaxWave)
                {
                    if(OrderManager.Instance.WaveTimer <= 10)
                    {
                        centerText.text = OrderManager.Instance.WaveTimer.ToString("Final Order In: 00.00");
                        waveText.text = "";
                    }
                    else
                    {
                        centerText.text = "";
                    }
                    waveText.color = Color.red;
                }
                else
                {
                    centerText.text = "";
                    waveText.color = Color.white;
                }
                if (OrderManager.Instance.Wave+1 != OrderManager.Instance.MaxWave || OrderManager.Instance.WaveTimer > 10)
                {
                    waveText.text = OrderManager.Instance.WaveTimer.ToString($"Wave {OrderManager.Instance.Wave + 1}/{OrderManager.Instance.MaxWave}\n00.00");
                    finalOrderText.text = "";
                }
            }
            else
            {
                waveText.text = "";
                waveText.color = Color.white;
                centerText.text = "";
                finalOrderText.text = $"Gold Order Value: ${OrderManager.Instance.FinalOrderValue}";
            }
        }
    }
}
