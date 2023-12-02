using System.Collections.Generic;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    [SerializeField]
    private float timeBetweenBalloons = 1f; // Time interval between balloon spawns
    private float durationUntilNextBalloon;
    private HashSet<int> players = new HashSet<int>();
    private Dictionary<int, Balloon> balloons = new Dictionary<int, Balloon>();
    private int balloonID = 0;

    void Start()
    {
        NetworkServerProcessing.SetGameLogic(this);
    }

    void Update()
    {
        if (players.Count > 0)
        {
            durationUntilNextBalloon -= Time.deltaTime;

            if (durationUntilNextBalloon <= 0f)
            {
                SpawnBalloon();
                durationUntilNextBalloon = timeBetweenBalloons; // Reset the spawn timer
            }
        }
        else if (players.Count == 0) // Clear balloons only if there are any and no players are connected
        {
            balloonID = 0;
            balloons.Clear();
        }
    }

    private void SpawnBalloon()
    {
        Debug.Log("Creating new Balloon");
        float positionX = Random.Range(0.0f, 1.0f);
        float positionY = Random.Range(0.0f, 1.0f);

        Balloon newBalloon = new Balloon(balloonID, positionX, positionY);
        balloons.Add(balloonID, newBalloon);

        SendBalloonToClients(balloonID, positionX, positionY);

        balloonID++;
    }

    private void SendBalloonToClients(int balloonID, float positionX, float positionY)
    {
        string msg = $"{ServerToClientSignifiers.addNewBalloonCommand},{balloonID},{positionX},{positionY}";
        foreach (int id in players)
        {
            NetworkServerProcessing.SendMessageToClient(msg, id, TransportPipeline.FireAndForget);
        }
    }

    public void AddPlayer(int playerID)
    {
        players.Add(playerID);
        if (balloons.Count > 0)
        {
            foreach (Balloon balloon in balloons.Values)
            {
                SendBalloonToClients (balloon.id, balloon.screenPositionXPercent, balloon.screenPositionYPercent);
            }
        }
    }

    public void RemovePlayer(int playerID)
    {
        players.Remove(playerID);
    }

    public void RemoveBalloon(int id)
    {
        if (balloons.TryGetValue(id, out Balloon _))
        {
            string msg = $"{ServerToClientSignifiers.removeBalloonCommand},{id}";
            foreach (int playerID in players)
            {
                NetworkServerProcessing.SendMessageToClient(msg, playerID, TransportPipeline.FireAndForget);
            }
            balloons.Remove(id);
        }
    }
}

public struct Balloon
{
    public int id { get; private set; }
    public float screenPositionXPercent { get; private set; }
    public float screenPositionYPercent { get; private set; }

    public Balloon(int id, float positionX, float positionY)
    {
        this.id = id;
        screenPositionXPercent = positionX;
        screenPositionYPercent = positionY;
    }
}
