using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// This class is meant to be attached to every player and will handle picking up, dropping, stealing, and delivering orders in the game, as well as player score.
/// </summary>
public class OrderHandler : MonoBehaviour
{
    private int score; // score of the player
    public int Score { get { return score; } set { score = value; } }
    private int placement = 0;
    public int Placement { get { return placement; }set { placement = value; } }
    [SerializeField] NumberHandler numberHandler;    

    private Order order1 = null; // first order the player is holding
    private Order order2 = null; // second order the player is holding

    [Tooltip("Positions the orders will snap to on the back of the scooter")]
    [SerializeField] private Transform order1Position;
    [SerializeField] private Transform order2Position;

    private BallDriving ball;
    public bool IsBoosting { get { return ball.Boosting; } }
    private bool hasGoldenOrder;
    public bool HasGoldenOrder { get { return hasGoldenOrder; } set { hasGoldenOrder = value; } }

    private SoundPool soundPool;
    private QAHandler qa;

    public delegate void GotHitDelegate();
    public event GotHitDelegate GotHit;

    public delegate void ClashDelegate(OrderHandler other);
    public event ClashDelegate Clash;


    private void Start()
    {
        score = 0; // init score to 0
        numberHandler.UpdateScoreUI(score.ToString());

        ScoreManager.Instance.AddOrderHandler(this);
        ball = transform.parent.GetComponentInChildren<BallDriving>();
        soundPool = GetComponent<SoundPool>();
        qa = GetComponent<QAHandler>();
    }

    private void OnEnable()
    {
        GameManager.Instance.OnSwapMenu += ResetHandler;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnSwapMenu -= ResetHandler;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
            DropEverything(order1Position.position, order2Position.position);
    }

    /// <summary>
    /// This method checks if the player can carry an order and adds it to their bike if they can.
    /// </summary>
    /// <param name="inOrder">Order the player is trying to pick up</param>
    public void AddOrder(Order inOrder)
    {
        if (inOrder.CanPickup || inOrder.PlayerDropped != this)
        {
            // will add order if it fits, elsewise will not do anything
            if (order1 == null || order2 == null)
            {
                soundPool.PlayOrderPickup();
                if (order2 == null)
                {
                    order2 = inOrder;
                    order2.transform.position = order2Position.position;
                    order2.transform.parent = order2Position;
                }
                else
                {
                    order1 = inOrder;
                    order1.transform.position = order1Position.position;
                    order1.transform.parent = order1Position;
                }
                inOrder.Pickup(this);
            }
        }
    }

    /// <summary>
    /// This method is called when the player delivers an order to the dropoff location. If the player has the right order it will drop it off and award the player.
    /// </summary>
    /// <param name="rightOrder">The correct order of the dropoff spot</param>
    public void DeliverOrder(Order rightOrder)
    {
        string key = rightOrder.Value == Constants.OrderValue.Golden ? "final_dropoff" : "dropoff";
        if (order1 == rightOrder)
        {
            soundPool.PlayOrderDropoff(key);
            score += (int)order1.Value;
            qa.Deliver(order1.Value);
            order1.DeliverOrder();
            order1 = null;
            ScoreManager.Instance.UpdatePlacement();
        }
        else if(order2 == rightOrder)
        {
            soundPool.PlayOrderDropoff(key);
            score += (int)order2.Value;
            qa.Deliver(order2.Value);
            order2.DeliverOrder();
            order2 = null;
            ScoreManager.Instance.UpdatePlacement();
        }

        numberHandler.UpdateScoreUI(score.ToString());
    }

    /// <summary>
    /// This method is for when the player "spins" out. It will drop all the orders the player is currently holding.
    /// </summary>
    /// <param name="basePos">The position of the order before adding the offset of the specific orderPosition.</param>
    public void DropEverything(Vector3 order1NewPos, Vector3 order2NewPos, bool shouldSpinout = true)
    {
        if (order1 != null)
        {
            order1.Drop(order1NewPos);
            order1 = null;
        }
        if (order2 != null)
        {
            order2.Drop(order2NewPos);
            order2 = null;
        }
        if(shouldSpinout) { GotHit(); }
    }

    /// <summary>
    /// This method returns the order of the highest value the player is holding. If the player isn't holidng anything it returns null.
    /// </summary>
    /// <returns></returns>
    public Order GetBestOrder()
    {
        if(order1 != null && order2 != null)
        {
            if(order2.Value > order1.Value)
            {
                return order2;
            }
            else
            {
                return order1;
            }
        }
        else if(order1 != null)
        {
            return order1;
        }
        else if(order2 != null)
        {
            return order2;
        }
        else
        {
            return null;
        }

    }

    /// <summary>
    /// This method accepts an order as a parameter and checks if the player is holding it. If they are it will remove it from their possession. Doesn't "drop" the order, intended for stealing.
    /// </summary>
    /// <param name="inOrder">Order to be removed</param>
    public void LoseOrder(Order inOrder)
    {
        if(inOrder == order1)
        {
            order1.RemovePlayerHolding();
            order1 = null;
        }
        else if(inOrder == order2)
        {
            order2.RemovePlayerHolding();
            order2 = null;
        }
    }

    /// <summary>
    /// This method steals the best order from a victim player. It will then make the victim drop their other order if they have one.
    /// </summary>
    /// <param name="victimPlayer">Player being stolen from</param>
    private void StealOrder(OrderHandler victimPlayer)
    {
        Order newOrder = victimPlayer.GetBestOrder();
        if (newOrder != null && (order1 == null || order2 == null))
        {
            soundPool.PlayOrderTheft();
            victimPlayer.LoseOrder(newOrder);
            AddOrder(newOrder);
            qa.Steals++;
        }
        victimPlayer.DropEverything(victimPlayer.order1Position.position, victimPlayer.order2Position.position);
    }

    /// <summary>
    /// This method resets this handler.
    /// </summary>
    private void ResetHandler()
    {
        if(order1 != null)
        {
            order1.EraseOrder();
        }
        if(order2 != null)
        {
            order2.EraseOrder();
        }

        hasGoldenOrder = false;
        placement = 0;
        score = 0;
    }

    /// <summary>
    /// Typical onCollisionEnter. Handles stealing orders from other players.
    /// </summary>
    /// <param name="other">Collision player has hit. Will attempt to steal if this hitbox is another player</param>
    private void OnTriggerEnter(Collider other)
    {
        OrderHandler otherHandler;
        try
        {
            otherHandler = other.gameObject.transform.parent.GetComponentInChildren<OrderHandler>();

            if(otherHandler == this)
            {
                return;
            }
        }
        catch
        {
            return;
        }
        if (ball.Boosting)
        {
            if (!otherHandler.IsBoosting)
            {
                StealOrder(otherHandler);
            }
            else
            {
                Clash(otherHandler);
                otherHandler.Clash(this);
            }
        }
    }
}
