package server;

import java.net.Socket;
import java.util.Queue;

public class ClientInfo {
	public Socket socket;
	public Queue<String> sendQueue;
	public String name;
	public int udp_port;
	public Room room;
	
	public ClientInfo(Socket _socket, Queue<String> _sendQueue) {
		socket = _socket;
		sendQueue = _sendQueue;
		name = "";
		udp_port = 0;
		room = null;
	}
}
