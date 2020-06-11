using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using Newtonsoft.Json;
//webSocket.OnFunc += (sender, e) => your function;
public class Client : MonoBehaviour
{
    private WebSocket webSocket;
    private Dictionary<string, GameObject> roomPlayers;
    private RoomInfo currentRoomInfo;
    private string myId = null;
    public string myNickname = "Hello boi";
    public Transform player;
    public GameObject playerPrefab;

    public bool logState = false;
    public float logTime = 3f;

    private void Awake()
    {
        roomPlayers = new Dictionary<string, GameObject>();
        currentRoomInfo = null;
        InitializeWebSocket();
    }

    private void InitializeWebSocket()
    {
        webSocket = new WebSocket("ws://localhost:5000/");
        webSocket.OnMessage += OnMessageHandler;
        webSocket.OnOpen += OnOpenHandler;
        webSocket.OnClose += OnCloseHandler;
        webSocket.OnError += OnErrorHandler;
        webSocket.ConnectAsync();

        if (logState)
            StartCoroutine(LogState());
    }

    private void Update()
    {
        SendData();

        if(currentRoomInfo != null)
        {
            if (myId == null)
                myId = currentRoomInfo.id;
            foreach(var key in currentRoomInfo.info.Keys )
            {
                if (key == myId) continue;
                if (!roomPlayers.ContainsKey(key))
                    roomPlayers.Add(key, Instantiate(playerPrefab, Vector3.zero, Quaternion.identity));
                var currentPlayerInfo = JsonConvert.DeserializeObject<PlayerInfo>(currentRoomInfo.info[key]);
                roomPlayers[key].transform.position = new Vector2(currentPlayerInfo.position[0], currentPlayerInfo.position[1]);
            }
            foreach(var key in new List<string>(roomPlayers.Keys) )
            {
                if (key == myId) continue;
                if (!currentRoomInfo.info.ContainsKey(key))
                {
                    Destroy(roomPlayers[key]);
                    roomPlayers.Remove(key);
                }
            }


        }
    }

    private void SendData()
    {
        if (!webSocket.IsAlive)
        {
            print("disconnected");
            return;
        }
        var toSendPackage = new PlayerInfo();
        toSendPackage.nickname = myNickname;
        toSendPackage.position = new float[2] { player.position.x, player.position.y };
        var stringified = JsonConvert.SerializeObject(toSendPackage);
        webSocket.Send(stringified);
    }

    private void OnDestroy()
    {
        webSocket.Close();
    }
    private void OnDisable()
    {
        webSocket.Close();
    }

    #region Event Handlers
    private void OnMessageHandler(object sender, MessageEventArgs e)
    {
        var jsonStr = e.Data;
        var currentRoom = JsonConvert.DeserializeObject<RoomInfo>(jsonStr);
        currentRoomInfo = currentRoom;
    }

    private void OnOpenHandler(object sender,  EventArgs e)
    {
        Debug.LogWarning("connected to the web-socket server");
    }

    private void OnCloseHandler(object sender, CloseEventArgs e)
    {
        Debug.LogWarning("connection closed");
    }

    private void OnErrorHandler(object sender, ErrorEventArgs e)
    {
        Debug.LogError($"{e.Message} : {e.Exception}");
    }
    #endregion

    private IEnumerator LogState()
    {
        for(; ; )
        {
            if (webSocket.IsAlive)
            {
                //log state
                var log = "Players idents:\n";
                foreach (var key in currentRoomInfo.info.Keys)
                    log += key + "\n";
                log += "My id:\n";
                log += currentRoomInfo.id;
                Debug.LogWarning(log);
            }
            else
            {
                Debug.LogWarning("Unable to log -> webSocket is closed...");
            }

            yield return new WaitForSeconds(logTime);
        }
    }
}

[Serializable]
public class RoomInfo
{
    public string id;
    public int count;
    public Dictionary<string, string> info;
}

[Serializable]
public class PlayerInfo {
    public string nickname;
    public float[] position;
}
