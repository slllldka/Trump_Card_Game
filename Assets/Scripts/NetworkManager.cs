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
using UnityEngine.UIElements;
using System.Xml;

public class NetworkManager : MonoBehaviour {


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

    UdpClient udpClient = null;
    bool on_holepunch = false;
    bool udp_leader = false;
    IPEndPoint[] udp_endPoint;
    public string[] udp_nickname;
    public Queue<String>[] udpSendQueue;
    int udp_connected_count = 0;
    bool[] udp_connected_arr;
    Room udp_room;

    string stunServer = "74.125.250.129";
    int stunPort = 19302;
    public int ex_port = 0;

    // Start is called before the first frame update

    void Awake() {
        if (nm == null) {
            nm = this;
            DontDestroyOnLoad(this);
        } else {
            Destroy(gameObject);
        }
    }

    void Start() {
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

        udp_nickname = new string[4];
        udp_endPoint = new IPEndPoint[4];
        udpSendQueue = new Queue<String>[4];
        udpSendQueue[0] = new Queue<String>();
        udpSendQueue[1] = new Queue<String>();
        udpSendQueue[2] = new Queue<String>();
        udpSendQueue[3] = new Queue<String>();
        udp_connected_arr = new bool[4];
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
        serverIP = IPAddress.Parse(ipString.Trim());
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
            } catch (Exception exception) {
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
                    } else if (data.StartsWith("HolePunch")) {
                        on_holepunch = true;
                        data = data[9..];
                        if (data.Equals("Fail")) {
                            initiate_udp_related_resources();
                        }

                        if (data[0] == 'L') {
                            udp_leader = true;
                            udp_connected_count = 0;
                            udp_endPoint[0] = new IPEndPoint(IPAddress.Parse("127.0.0.1"), ((IPEndPoint)udpClient.Client.LocalEndPoint).Port);
                            data = data[1..];
                            string[] participantArray = data.Split('/');
                            int idx = 0;
                            foreach (string participant in participantArray) {
                                if (participant.Length == 0) {
                                    continue;
                                }
                                string[] elements = participant.Split(':');
                                udp_nickname[idx + 1] = elements[0];
                                udp_endPoint[idx + 1] = new IPEndPoint(IPAddress.Parse(elements[1]), int.Parse(elements[2]));

                                //send to participant
                                udpSendQueue[idx + 1].Clear();
                                Thread sendToParticipantThread = new Thread(new ParameterizedThreadStart(udp_Send_To_Participant));
                                runningThreads.Add(sendToParticipantThread);
                                sendToParticipantThread.Start(idx + 1);

                                idx++;
                            }

                            int try_count = 0;
                            while (udp_connected_count < 3 && try_count < 5000) {
                                if (!udp_connected_arr[1]) {
                                    lock (udpSendQueue[1]) {
                                        udpSendQueue[1].Enqueue("Sync");
                                    }
                                }
                                if (!udp_connected_arr[2]) {
                                    lock (udpSendQueue[2]) {
                                        udpSendQueue[2].Enqueue("Sync");
                                    }
                                }
                                if (!udp_connected_arr[3]) {
                                    lock (udpSendQueue[3]) {
                                        udpSendQueue[3].Enqueue("Sync");
                                    }
                                }
                                try_count++;
                                Thread.Sleep(1);
                            }

                            if (try_count < 5000) {
                                Debug.Log("holepunch done!");

                                lock (udpSendQueue[1]) {
                                    udpSendQueue[1].Enqueue("HolepunchDone");
                                }

                                lock (udpSendQueue[2]) {
                                    udpSendQueue[2].Enqueue("HolepunchDone");
                                }

                                lock (udpSendQueue[3]) {
                                    udpSendQueue[3].Enqueue("HolepunchDone");
                                }

                                //send to leader
                                sendQueue.Clear();
                                Thread sendToLeaderThread = new Thread(udp_Send_To_Leader);
                                runningThreads.Add(sendToLeaderThread);
                                sendToLeaderThread.Start();

                                //send to self
                                udpSendQueue[0].Clear();
                                Thread sendToSelfThread = new Thread(new ParameterizedThreadStart(udp_Send_To_Participant));
                                runningThreads.Add(sendToSelfThread);
                                sendToSelfThread.Start(0);

                                //game start
                                udp_room = new Room(null);
                                int startCard = udp_room.drawCard();
                                udp_room.openedCard = startCard;
                                udp_room.usedCards.Add(startCard);
                                string str = "WStart" + startCard;
                                foreach (string nickname in udp_nickname) {
                                    str += ".";
                                    str += nickname;
                                }
                                udp_broadcast(str, -1);
                            } else {
                                lock (udpSendQueue[1]) {
                                    udpSendQueue[1].Clear();
                                }

                                lock (udpSendQueue[2]) {
                                    udpSendQueue[2].Clear();
                                }

                                lock (udpSendQueue[3]) {
                                    udpSendQueue[3].Clear();
                                }
                                on_holepunch = false;
                                sendQueue.Enqueue("WHolePunchFail");
                            }
                        } else if (data[0] == 'P') {
                            udp_leader = false;

                            //send to leader
                            sendQueue.Clear();
                            Thread sendToLeaderThread = new Thread(udp_Send_To_Leader);
                            runningThreads.Add(sendToLeaderThread);
                            sendToLeaderThread.Start();

                            data = data[1..];
                            string[] elements = data.Split(':');
                            Debug.Log(elements[0]);
                            Debug.Log(int.Parse(elements[1]));
                            udp_endPoint[0] = new IPEndPoint(IPAddress.Parse(elements[0]), int.Parse(elements[1]));
                            int try_count = 0;
                            while (!udp_connected_arr[0] && try_count < 5000) {
                                sendQueue.Enqueue("Sync");
                                try_count++;
                                Thread.Sleep(1);
                            }

                            if(try_count >= 5000) {
                                lock (sendQueue) {
                                    sendQueue.Clear();
                                }
                                on_holepunch = false;
                            }
                        }
                        Debug.Log(data);
                    }
                }
            }
        }
    }

    private void send() {
        while (connected) {
            if (!on_holepunch) {
                if (sendQueue.Count > 0) {
                    string data = sendQueue.Dequeue() + ",";
                    Debug.Log("client " + data);
                    if (data.Length > 0) {
                        Debug.Log(clientSocket.Send(Encoding.UTF8.GetBytes(data)));
                    }
                }
            }
        }
    }

    public int openUdpClient() {
        udpClient = new UdpClient(0);
        ex_port = 0;
        byte[] sendByte = Encoding.UTF8.GetBytes("Hello");
        Debug.Log(udpClient.Send(sendByte, sendByte.Length, new IPEndPoint(serverIP, 22222)));

        Thread receiveThread = new Thread(udp_Receive);
        runningThreads.Add(receiveThread);
        receiveThread.Start();

        // STUN 요청 메시지 생성
        byte[] stunRequest = new byte[20];
        stunRequest[0] = 0x00; // Binding Request Type
        stunRequest[1] = 0x01;
        stunRequest[2] = 0x00; // Message Length
        stunRequest[3] = 0x00;
        // Transaction ID (16 bytes, 랜덤)
        var rand = new System.Random();
        rand.NextBytes(stunRequest[4..20]);
        udpClient.Send(stunRequest, stunRequest.Length, new IPEndPoint(IPAddress.Parse(stunServer), 19302));

        return ((IPEndPoint)udpClient.Client.LocalEndPoint).Port;
    }

    public void closeUdpClient() {
        if (udpClient != null) {
            udpClient.Close();
            udpClient = null;
        }
    }

    private void udp_Send_To_Leader() {
        while (udpClient != null) {
            while (sendQueue.Count > 0) {
                String str = sendQueue.Dequeue();
                if (str.Equals("Sync")) {
                    if (udp_connected_arr[0]) {
                        continue;
                    }
                }
                byte[] sendByte = Encoding.UTF8.GetBytes(str + ',');
                udpClient.Send(sendByte, sendByte.Length, udp_endPoint[0]);
            }
        }
    }

    private void udp_Send_To_Participant(object obj) {
        int idx = (int)obj;
        while (udpClient != null) {
            while (udpSendQueue[idx].Count > 0) {
                if (idx > 0) {
                    String str = udpSendQueue[idx].Dequeue();
                    if (str == null) {
                        continue;
                    }
                    if (str.Equals("Sync")) {
                        if (udp_connected_arr[idx]) {
                            continue;
                        }
                    }
                    byte[] sendByte = Encoding.UTF8.GetBytes(str + ',');
                    udpClient.Send(sendByte, sendByte.Length, udp_endPoint[idx]);
                } else {
                    //loopback으로 보내고 받았다 치고 적용

                    String rec = udpSendQueue[idx].Dequeue();
                    Debug.Log("LoopBack: " + rec);
                    if (rec.StartsWith('A')) {
                        rec = rec[1..];
                        if (rec.StartsWith('D')) {
                            rec = rec[1..];
                            Debug.Log(rec);
                            receivedCardDrawActionQueue.Enqueue(int.Parse(rec));
                        } else if (rec.StartsWith('P')) {
                            rec = rec[1..];
                            receivedCardPlayActionQueue.Enqueue(int.Parse(rec));
                            Debug.Log(rec);
                        }
                    } else if (rec.StartsWith('D')) {
                        rec = rec[1..];
                        receivedDrawQueue.Enqueue(int.Parse(rec));
                    } else if (rec.StartsWith('O')) {
                        rec = rec[1..];
                        if (rec.StartsWith("StartTurn")) {
                            rec = rec[9..];
                            receivedStartTurnQueue.Enqueue(int.Parse(rec));
                        } else if (rec.StartsWith("OpenedCard")) {
                            rec = rec[10..];
                            receivedOpenedCardQueue.Enqueue(int.Parse(rec));
                        } else if (rec.StartsWith("CardPenalty")) {
                            rec = rec[11..];
                            receivedCardPenaltyQueue.Enqueue(int.Parse(rec));
                        } else if (rec.StartsWith("Direction")) {
                            rec = rec[9..];
                            receivedDirectionQueue.Enqueue(int.Parse(rec));
                        } else if (rec.Equals("ChooseSuit")) {
                            GameManager.OneCard.setAllSuitBtnsInteractable(true);
                        }
                    } else if (rec.StartsWith('W')) {
                        rec = rec[1..];
                        receivedWaitWindowQueue.Enqueue(rec);
                    }
                }
            }
        }
    }

    private void udp_broadcast(string data, int except) {
        for (int i = 0; i < 4; i++) {
            if (i != except) {
                udpSendQueue[i].Enqueue(data);
            }
        }
    }

    private void udp_Receive() {
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
        while (udpClient != null) {
            byte[] data = null;
            try {
                data = udpClient.Receive(ref endPoint);
            } catch (Exception e) {
                Debug.Log("Receive Error" + e.Message);
            }

            if (ex_port == 0 && endPoint.Equals(new IPEndPoint(IPAddress.Parse(stunServer), stunPort))) {
                Debug.Log("Good");
                // 응답 파싱해서 MAPPED-ADDRESS 찾기
                if (data.Length >= 20 + 8) {
                    int attrType = (data[20] << 8) | data[21];
                    int attrLen = (data[22] << 8) | data[23];

                    if (attrType == 0x0001 && attrLen == 8) // MAPPED-ADDRESS
                    {
                        byte family = data[25];
                        int port = (data[26] << 8) | data[27];
                        string ip = $"{data[28]}.{data[29]}.{data[30]}.{data[31]}";

                        Debug.Log($"External IP: {ip}");
                        Debug.Log($"External Port: {port}");

                        ex_port = port;
                    } else {
                        Debug.Log("MAPPED-ADDRESS not found in response.");
                    }
                } else {
                    Debug.Log("Invalid STUN response.");
                }
                continue;
            }

            string[] recArray = Encoding.UTF8.GetString(data, 0, data.Length).Split(',');
            foreach (string _rec in recArray) {
                string rec = new string(_rec);
                if (rec.Length == 0) {
                    continue;
                }
                Debug.Log(rec);
                if (udp_leader) {
                    int idx = 0;
                    for (int i = 1; i <= 3; i++) {
                        if (endPoint.Equals(udp_endPoint[i])) {
                            idx = i;
                            break;
                        }
                    }

                    if (idx > 0) {
                        if (rec.Equals("Sync")) {
                            lock (udpSendQueue[idx]) {
                                udpSendQueue[idx].Enqueue("Ack");
                            }
                        } else if (rec.Equals("Ack")) {
                            if (!udp_connected_arr[idx]) {
                                udp_connected_count++;
                                udp_connected_arr[idx] = true;
                            }
                        }
                    }
                        //serverReceive 함수 참고
                        char startChar = rec[0];
                    // draw
                    if (startChar == 'D') {
                        lock (udp_room) {
                            udpSendQueue[idx].Enqueue("D" + udp_room.drawCard());
                            udp_broadcast("AD" + idx, idx);
                        }
                    }
                    // OneCard
                    else if (startChar == 'O') {
                        rec = rec[1..];
                        // ready
                        if (rec.Equals("Ready")) {
                            udp_room.readyCount++;
                            if (udp_room.readyCount == 4) {
                                udp_broadcast("OStartTurn0", -1);
                            }
                        } else if (rec.StartsWith("EndTurn")) {
                            int card = -2;
                            rec = rec[7..];
                            card = int.Parse(rec);
                            Debug.Log(card);
                            //didn't play card
                            if (card == -1) {
                                udp_room.passTurnNum = 1;
                                if (udp_room.gameStatus == General) {
                                    udpSendQueue[idx].Enqueue("D" + udp_room.drawCard());
                                    udp_broadcast("AD" + idx, idx);
                                } else if (udp_room.gameStatus == Attack) {
                                    for (int i = 0; i < udp_room.cardPenalty; i++) {
                                        udpSendQueue[idx].Enqueue("D" + udp_room.drawCard());
                                        udp_broadcast("AD" + idx, idx);
                                    }
                                    udp_room.gameStatus = General;
                                    udp_room.cardPenalty = 0;
                                }

                                udp_broadcast("OOpenedCard-1", -1);
                                udp_broadcast("OCardPenalty" + udp_room.cardPenalty, -1);
                            }
                            //played card
                            else {
                                udp_room.usedCards.Insert(0, card);
                                udp_room.openedCard = card;
                                udp_broadcast("AP" + idx, idx);
                                udp_broadcast("OOpenedCard" + card, -1);

                                //seven
                                if ((card % 13 + 1) == 7) {
                                    udpSendQueue[idx].Enqueue("OChooseSuit");
                                    continue;
                                } else {
                                    udp_room.cardEffect(card);
                                    udp_broadcast("OCardPenalty" + udp_room.cardPenalty, -1);
                                    udp_broadcast("ODirection" + udp_room.direction, -1);
                                }
                            }

                            if (udp_room.direction == CounterClockwise) {
                                udp_room.nextTurn = (idx + udp_room.passTurnNum) % 4;
                            } else {
                                udp_room.nextTurn = (idx - udp_room.passTurnNum + 4) % 4;
                            }

                            udp_broadcast("OStartTurn" + udp_room.nextTurn, -1);

                        } else if (rec.StartsWith("ChooseSuit")) {
                            int suit = -1;
                            rec = rec[10..];
                            suit = int.Parse(rec);
                            udp_room.openedCard = suit * 13 + 6;
                            udp_broadcast("OOpenedCard" + udp_room.openedCard, -1);
                            udp_broadcast("OCardPenalty" + udp_room.cardPenalty, -1);

                            if (udp_room.direction == CounterClockwise) {
                                udp_room.nextTurn = (idx + udp_room.passTurnNum) % 4;
                            } else {
                                udp_room.nextTurn = (idx - udp_room.passTurnNum + 4) % 4;
                            }
                            udp_broadcast("OStartTurn" + udp_room.nextTurn, -1);
                        }
                    }
                } else {
                    if (rec.Equals("Sync")) {
                        sendQueue.Enqueue("Ack");
                    } else if (rec.Equals("Ack")) {
                        if (!udp_connected_arr[0]) {
                            udp_connected_arr[0] = true;
                        }
                    } else if (rec.Equals("HolepunchDone")) {
                        lock (sendQueue) {
                            sendQueue.Clear();
                        }
                    }
                    //receive 함수 참고
                    else if (rec.StartsWith('A')) {
                        rec = rec[1..];
                        if (rec.StartsWith('D')) {
                            rec = rec[1..];
                            Debug.Log(rec);
                            receivedCardDrawActionQueue.Enqueue(int.Parse(rec));
                        } else if (rec.StartsWith('P')) {
                            rec = rec[1..];
                            receivedCardPlayActionQueue.Enqueue(int.Parse(rec));
                            Debug.Log(rec);
                        }
                    } else if (rec.StartsWith('D')) {
                        rec = rec[1..];
                        receivedDrawQueue.Enqueue(int.Parse(rec));
                    } else if (rec.StartsWith('O')) {
                        rec = rec[1..];
                        if (rec.StartsWith("StartTurn")) {
                            rec = rec[9..];
                            receivedStartTurnQueue.Enqueue(int.Parse(rec));
                        } else if (rec.StartsWith("OpenedCard")) {
                            rec = rec[10..];
                            receivedOpenedCardQueue.Enqueue(int.Parse(rec));
                        } else if (rec.StartsWith("CardPenalty")) {
                            rec = rec[11..];
                            receivedCardPenaltyQueue.Enqueue(int.Parse(rec));
                        } else if (rec.StartsWith("Direction")) {
                            rec = rec[9..];
                            receivedDirectionQueue.Enqueue(int.Parse(rec));
                        } else if (rec.Equals("ChooseSuit")) {
                            GameManager.OneCard.setAllSuitBtnsInteractable(true);
                        }
                    } else if (rec.StartsWith('W')) {
                        rec = rec[1..];
                        receivedWaitWindowQueue.Enqueue(rec);
                    }
                }
            }
        }
    }

    public void initiate_udp_related_resources() {
        closeUdpClient();
        on_holepunch = false;
        udp_leader = false;
        udp_endPoint = new IPEndPoint[4];
        udp_nickname = new string[4];
        udpSendQueue = new Queue<String>[4];
        udpSendQueue[0] = new Queue<String>();
        udpSendQueue[1] = new Queue<String>();
        udpSendQueue[2] = new Queue<String>();
        udpSendQueue[3] = new Queue<String>();
        udp_connected_count = 0;
        udp_connected_arr = new bool[4];
        udp_room = null;
        ex_port = 0;
}

    void OnApplicationQuit() {
        if (udpClient != null) {
            closeUdpClient();
            on_holepunch = false;
        }
        lock (sendQueue) {
            sendQueue.Clear();
        }
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