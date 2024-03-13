using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class ClientInfo
{
	public Socket socket;
	public Queue<string> sendQueue, receiveQueue;
	public string Name;
	public Room room;

	public ClientInfo(Socket _socket, Queue<string> _sendQueue, Queue<string> _receiveQueue) {
		socket = _socket;
		sendQueue = _sendQueue;
		receiveQueue = _receiveQueue;
		Name = "";
		room = null;
	}
}
