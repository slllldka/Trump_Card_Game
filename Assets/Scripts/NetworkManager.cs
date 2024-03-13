using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using TMPro;

public class NetworkManager : MonoBehaviour
{

    public static readonly int General = 0, Attack = 1, CounterClockwise = 0, Clockwise = 1, Spade = 0, Diamond = 1, Heart = 2, Club = 3, totalCardsNum = 54;

    public static NetworkManager nm = null;

    IPAddress serverIP;
    IPEndPoint serverEndPoint;
    Socket clientSocket = null, serverSocket = null;
    public bool connected = false;
    public bool opened = false;

    byte[] getByte;

    public Queue<int> receivedCardDrawActionQueue, receivedCardPlayActionQueue, receivedDrawQueue
        , receivedStartTurnQueue, receivedOpenedCardQueue, receivedCardPenaltyQueue, receivedDirectionQueue;
    public Queue<string> receivedWaitWindowQueue;
    public Queue<string> sendQueue;
    Thread receiveThread, sendThread;
    List<Thread> runningThreads;

    public List<ClientInfo> oneCardPlayerList;

    // Start is called before the first frame update

    void Awake() {
        if(nm == null) {
            nm = this;
            DontDestroyOnLoad(this);
        } else {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        getByte = new byte[1024];
        receivedCardDrawActionQueue = new Queue<int>();
        receivedCardPlayActionQueue = new Queue<int>();
        receivedDrawQueue = new Queue<int>();
        receivedStartTurnQueue = new Queue<int>();
        receivedOpenedCardQueue = new Queue<int>();
        receivedCardPenaltyQueue = new Queue<int>();
        receivedDirectionQueue = new Queue<int>();
        receivedWaitWindowQueue = new Queue<string>();
        sendQueue = new Queue<string>();

        runningThreads = new List<Thread>();

        oneCardPlayerList = new List<ClientInfo>();
    }

    // Update is called once per frame
    void Update() {

    }
    public void openServer(object port) {
        int playerIdx = 0;
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint serverEP = new IPEndPoint(IPAddress.Any, (int)port);
        serverSocket.Bind(serverEP);
        serverSocket.Listen(10);
        Debug.Log(Dns.GetHostEntry(Dns.GetHostName()).AddressList[1].ToString());
        opened = true;

        while (opened) {
            Debug.Log("waiting for access...");
            Socket socket = serverSocket.Accept();
            Queue<string> serverSendQueue = new Queue<string>();
            Queue<string> serverReceiveQueue = new Queue<string>();
            ClientInfo clientInfo = new ClientInfo(socket, serverSendQueue, serverReceiveQueue);
            socket.SendTimeout = 600000;
            socket.ReceiveTimeout = 600000;
            Thread serverSendThread = new Thread(new ParameterizedThreadStart(NetworkManager.nm.serverSend));
            Thread serverReceiveThread = new Thread(new ParameterizedThreadStart(NetworkManager.nm.serverReceive));
            Thread serverReceiveProcessThread = new Thread(new ParameterizedThreadStart(NetworkManager.nm.serverReceiveProcess));
            runningThreads.Add(serverSendThread);
            runningThreads.Add(serverReceiveThread);
            runningThreads.Add(serverReceiveProcessThread);
            serverSendThread.Start(clientInfo);
            serverReceiveThread.Start(clientInfo);
            serverReceiveProcessThread.Start(clientInfo);

            playerIdx++;
        }
    }

    public void serverSend(object obj) {
        Socket socket = ((ClientInfo)obj).socket;
        Queue<string> serverSendQueue = ((ClientInfo)obj).sendQueue;
        while (opened) {
            while (serverSendQueue.Count > 0) {
                string sendString = serverSendQueue.Dequeue() + ",";
                socket.Send(Encoding.UTF8.GetBytes(sendString));
                Debug.Log("server " + sendString);
            }
        }
        Debug.Log("Done");
    }
    public void serverReceive(object obj) {
        Socket socket = ((ClientInfo)obj).socket;
        Queue<string> serverReceiveQueue = ((ClientInfo)obj).receiveQueue;
        byte[] receive = new byte[1024];
        int length;
        while (opened) {
            length = socket.Receive(receive);
            string[] recArray = Encoding.UTF8.GetString(receive, 0, length).Split(',');
            foreach (string rec in recArray) {
                if (rec.Length > 0) {
                    Debug.Log(length + rec);
                    serverReceiveQueue.Enqueue(rec);
                }
            }
        }
        Debug.Log("Done");
    }
    public void serverReceiveProcess(object obj) {
        ClientInfo clientInfo = (ClientInfo)obj;
        Socket socket = clientInfo.socket;
        Queue<string> serverSendQueue = ((ClientInfo)obj).sendQueue;
        Queue<string> serverReceiveQueue = clientInfo.receiveQueue;
        while (opened) {
            if (serverReceiveQueue.Count > 0) {
                string rec = serverReceiveQueue.Dequeue();
                Debug.Log("received " + clientInfo.Name + " " + rec);
                char startChar = rec[0];
                if (rec.Equals("CloseSocket")) {
                    Debug.Log("close");
                    socket.Close();
                    break;
                }
                // draw
                else if (startChar == 'D') {
                    lock (clientInfo.room) {
                        serverSendQueue.Enqueue("D" + clientInfo.room.drawCard());
                        clientInfo.room.broadcast("AD" + clientInfo.room.players.IndexOf(clientInfo), clientInfo);
                    }
                }
                // OneCard
                else if (startChar == 'O') {
                    rec = rec[1..];
                    // ready
                    if (rec.Equals("Ready")) {
                        clientInfo.room.readyCount++;
                        if (clientInfo.room.readyCount == 4) {
                            clientInfo.room.broadcast("OStartTurn0", null);
                        }
                    } else if (rec.StartsWith("EndTurn")) {
                        int card = -2;
                        rec = rec[7..];
                        card = int.Parse(rec);
                        Debug.Log(card);
                        //didn't play card
                        if (card == -1) {
                            clientInfo.room.passTurnNum = 1;
                            if (clientInfo.room.gameStatus == General) {
                                serverSendQueue.Enqueue("D" + clientInfo.room.drawCard());
                                clientInfo.room.broadcast("AD" + clientInfo.room.players.IndexOf(clientInfo), clientInfo);
                            } else if (clientInfo.room.gameStatus == Attack) {
                                for (int i = 0; i < clientInfo.room.cardPenalty; i++) {
                                    serverSendQueue.Enqueue("D" + clientInfo.room.drawCard());
                                    clientInfo.room.broadcast("AD" + clientInfo.room.players.IndexOf(clientInfo), clientInfo);
                                }
                                clientInfo.room.gameStatus = General;
                                clientInfo.room.cardPenalty = 0;
                            }

                            clientInfo.room.broadcast("OOpenedCard-1", null);
                            clientInfo.room.broadcast("OCardPenalty" + clientInfo.room.cardPenalty, null);
                        }
                        //played card
                        else {
                            clientInfo.room.usedCards.Insert(0, card);
                            clientInfo.room.openedCard = card;
                            clientInfo.room.broadcast("AP" + clientInfo.room.players.IndexOf(clientInfo), clientInfo);
                            clientInfo.room.broadcast("OOpenedCard" + card, null);

                            //seven
                            if ((card % 13 + 1) == 7) {
                                serverSendQueue.Enqueue("OChooseSuit");
                                continue;
                            } else {
                                clientInfo.room.cardEffect(card);
                                clientInfo.room.broadcast("OCardPenalty" + clientInfo.room.cardPenalty, null);
                                clientInfo.room.broadcast("ODirection" + clientInfo.room.direction, null);
                            }
                        }

                        if (clientInfo.room.direction == CounterClockwise) {
                            clientInfo.room.nextTurn = (clientInfo.room.players.IndexOf(clientInfo) + clientInfo.room.passTurnNum) % 4;
                        } else {
                            clientInfo.room.nextTurn = (clientInfo.room.players.IndexOf(clientInfo) - clientInfo.room.passTurnNum + 4) % 4;
                        }

                        clientInfo.room.broadcast("OStartTurn" + clientInfo.room.nextTurn, null);

                    } else if (rec.StartsWith("ChooseSuit")) {
                        int suit = -1;
                        rec = rec[10..];
                        suit = int.Parse(rec);
                        clientInfo.room.openedCard = suit * 13 + 6;
                        clientInfo.room.broadcast("OOpenedCard" + clientInfo.room.openedCard, null);
                        clientInfo.room.broadcast("OCardPenalty" + clientInfo.room.cardPenalty, null);

                        if (clientInfo.room.direction == CounterClockwise) {
                            clientInfo.room.nextTurn = (clientInfo.room.players.IndexOf(clientInfo) + clientInfo.room.passTurnNum) % 4;
                        } else {
                            clientInfo.room.nextTurn = (clientInfo.room.players.IndexOf(clientInfo) - clientInfo.room.passTurnNum + 4) % 4;
                        }
                        clientInfo.room.broadcast("OStartTurn" + clientInfo.room.nextTurn, null);
                    }
                }
                // wait
                else if (startChar == 'W') {
                    rec = rec[1..];
                    // exit
                    if (rec.StartsWith("Exit")) {
                        rec = rec[4..];
                        foreach (ClientInfo data in oneCardPlayerList) {
                            if (data.Name.Equals(rec)) {
                                Debug.Log("remove");
                                oneCardPlayerList.Remove(data);
                                break;
                            }
                        }
                        Debug.Log(oneCardPlayerList.Count);

                        for (int i = 0; i < Math.Min(oneCardPlayerList.Count, 4); i++) {
                            oneCardPlayerList[i].sendQueue.Enqueue("W" + oneCardPlayerList.Count);
                        }
                    }
                    // waiting
                    else {
                        bool nameExist = false;

                        for (int i = 0; i < Math.Min(oneCardPlayerList.Count, 4); i++) {
                            if (oneCardPlayerList[i].Name.Equals(rec)) {
                                nameExist = true;
                                break;
                            }
                        }

                        // existing name
                        if (nameExist) {
                            serverSendQueue.Enqueue("WExisting Name");
                        }
                        // available name
                        else {
                            clientInfo.Name = rec;
                            oneCardPlayerList.Add(clientInfo);
                            for (int i = 0; i < Math.Min(oneCardPlayerList.Count, 4); i++) {
                                oneCardPlayerList[i].sendQueue.Enqueue("W" + Math.Min(oneCardPlayerList.Count, 4));
                            }

                            // game start
                            if (oneCardPlayerList.Count >= 4) {
                                ClientInfo[] clientList = new ClientInfo[4];
                                for (int i = 0; i < 4; i++) {
                                    clientList[i] = oneCardPlayerList[0];
                                    oneCardPlayerList.RemoveAt(0);
                                }

                                Room newRoom = new Room(clientList);
                                int startCard = newRoom.drawCard();
                                newRoom.openedCard = startCard;
                                newRoom.usedCards.Add(startCard);
                                string data = "WStart" + startCard;
                                foreach (ClientInfo client in newRoom.players) {
                                    data += ".";
                                    data += client.Name;
                                }
                                newRoom.broadcast(data, null);
                            }
                        }
                    }
                }
            }
        }
        Debug.Log("Done");
    }

    public void closeServer() {
        serverSocket.Close();
        serverSocket = null;
        opened = false;
    }
    public void connectToServer(string ipString, int port) {
        serverIP = IPAddress.Parse(ipString);
        serverEndPoint = new IPEndPoint(serverIP, port);
        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        if (clientSocket != null) {
            clientSocket.Connect(serverEndPoint);
            clientSocket.ReceiveTimeout = 10000;

            if (clientSocket.Connected) {
                Debug.Log("connected");
                connected = true;
                receiveThread = new Thread(receive);
                receiveThread.Start();
                sendThread = new Thread(send);
                sendThread.Start();
            }
        }
    }
    public void disconnect() {
        connected = false;
        clientSocket.Close();
        clientSocket = null;
    }
    public void receive() {
        while (connected) {
            int length = 0;
            try {
                length = clientSocket.Receive(getByte);
            }
            catch(Exception exception) {
                if (exception.Message.Equals("연결된 구성원으로부터 응답이 없어 연결하지 못했거나, 호스트로부터 응답이 없어 연결이 끊어졌습니다.")) {
                    if (SceneManager.GetActiveScene().name.Equals("OneCard")) {
                        if (GameManager.OneCard.isPvP) {
                            Debug.Log("Disconnect");
                            disconnect();
                        }
                    }
                }
            }

            string[] recArray = Encoding.UTF8.GetString(getByte, 0, length).Split(',');
            foreach (string rec in recArray) {
                if (rec.Length > 0) {
                    string data = rec;
                    if (data.StartsWith('A')) {
                        data = data[1..];
                        if (data.StartsWith('D')) {
                            data = data[1..];
                            Debug.Log(data);
                            receivedCardDrawActionQueue.Enqueue(int.Parse(data));
                        } else if (data.StartsWith('P')) {
                            data = data[1..];
                            receivedCardPlayActionQueue.Enqueue(int.Parse(data));
                            Debug.Log(data);
                        }
                    } else if (data.StartsWith('D')) {
                        data = data[1..];
                        receivedDrawQueue.Enqueue(int.Parse(data));
                    } else if (data.StartsWith('O')) {
                        data = data[1..];
                        if (data.StartsWith("StartTurn")) {
                            data = data[9..];
                            receivedStartTurnQueue.Enqueue(int.Parse(data));
                        } else if (data.StartsWith("OpenedCard")) {
                            data = data[10..];
                            receivedOpenedCardQueue.Enqueue(int.Parse(data));
                        } else if (data.StartsWith("CardPenalty")) {
                            data = data[11..];
                            receivedCardPenaltyQueue.Enqueue(int.Parse(data));
                        } else if (data.StartsWith("Direction")) {
                            data = data[9..];
                            receivedDirectionQueue.Enqueue(int.Parse(data));
                        } else if (data.Equals("ChooseSuit")) {
                            GameManager.OneCard.setAllSuitBtnsInteractable(true);
                        }
                    } else if (data.StartsWith('W')) {
                        data = data[1..];
                        receivedWaitWindowQueue.Enqueue(data);
                    }
                }
            }
        }
    }

    private void send() {
        while (connected) {
            if (sendQueue.Count > 0) {
                string data = sendQueue.Dequeue() + ",";
                Debug.Log("client "+ data);
                if (data.Length > 0) {
                    Debug.Log(clientSocket.Send(Encoding.UTF8.GetBytes(data)));
                }
            }
        }
    }

    void OnApplicationQuit() {
        if (connected) {
            sendQueue.Enqueue("CloseSocket");
            while (sendQueue.Count > 0) { }
            disconnect();
        }
        if (opened) {
            closeServer();
        }
        connected = false;
        opened = false;
    }
}
