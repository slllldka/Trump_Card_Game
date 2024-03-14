package server;

import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.ServerSocket;
import java.net.Socket;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.LinkedList;
import java.util.Queue;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

public class Server {

	protected static final int General = 0, Attack = 1, CounterClockwise = 0, Clockwise = 1, Spade = 0, Diamond = 1, Heart = 2, Club = 3;
	
	protected static ExecutorService threadPool;

	protected static int playerNum = 0;
	
	protected static ArrayList<ClientInfo> oneCardPlayerList;
	
	protected static int totalCardsNum = 54;

	public static void main(String[] args) {
		threadPool = Executors.newFixedThreadPool(100);
		
		oneCardPlayerList = new ArrayList<ClientInfo>();

		ServerSocket serverSocket = null;
		try {
			serverSocket = new ServerSocket(11111);
			System.out.println("server created!");
		} catch (Exception e) {
			e.printStackTrace();
		}

		while (true) {
			try {
				int idx = playerNum;
				playerNum++;
				System.out.println("waiting for access...");

				Socket socket = serverSocket.accept();
				InputStream in = socket.getInputStream();
				OutputStream out = socket.getOutputStream();
				DataInputStream dis = new DataInputStream(in);
				DataOutputStream dos = new DataOutputStream(out);
				Queue<String> sendQueue = new LinkedList<String>();
				Queue<String> receiveQueue = new LinkedList<String>();
				ClientInfo clientInfo = new ClientInfo(socket, sendQueue);
				socket.setSoTimeout(600000);
				
				Runnable sendThread = new Runnable() {

					@Override
					public void run() {
						while (socket.isConnected() && !socket.isClosed()) {
							while (sendQueue.size() > 0) {
								String sendString = sendQueue.poll() + ",";
								try {
									dos.write(sendString.getBytes("UTF-8"));
								} catch (IOException e) {
									e.printStackTrace();
								}
							}
							try {
								Thread.sleep(1);
							} catch (InterruptedException e) {
								e.printStackTrace();
							}
						}
						System.out.println("Done");
					}
				};
				threadPool.submit(sendThread);

				Runnable receiveThread = new Runnable() {

					@Override
					public void run() {
						byte[] receive = new byte[1024];
						int length;
						while (socket.isConnected() && !socket.isClosed()) {
							try {
								length = dis.read(receive);
								String[] recArray = new String(receive, 0, length, StandardCharsets.UTF_8).split(",");
								for (String rec : recArray) {
									System.out.println(rec);
									receiveQueue.add(rec);
								}
							} catch (IOException e) {
								if(e.getClass().getSimpleName().equals("SocketTimeoutException")) {
									System.err.println("timeout");
									try {
										socket.close();
									} catch (IOException e1) {
										e1.printStackTrace();
									}
								}
								e.printStackTrace();
							}
						}
						System.out.println("Done");
					}
				};
				threadPool.submit(receiveThread);

				Runnable receiveProcessThread = new Runnable() {

					@Override
					public void run() {
						int pID = idx;
						while (socket.isConnected() && !socket.isClosed()) {
							while (receiveQueue.size() > 0) {
								String rec = receiveQueue.poll();
								System.out.println("received " + pID + " " + rec);
								char startChar = rec.charAt(0);
								if(rec.equals("CloseSocket")) {
									System.out.println("close");
									try {
										socket.close();
										break;
									} catch (IOException e) {
										e.printStackTrace();
									}
								}
								// draw
								else if (startChar == 'D') {
									sendQueue.add("D" + clientInfo.room.drawCard());
									clientInfo.room.broadcast("AD" + clientInfo.room.players.indexOf(clientInfo), clientInfo);
								}
								// OneCard
								else if (startChar == 'O') {
									rec = rec.substring(1);
									// ready
									if (rec.equals("Ready")) {
										clientInfo.room.readyCount++;
										if(clientInfo.room.readyCount == 4) {
											clientInfo.room.broadcast("OStartTurn0", null);
										}
									} else if (rec.startsWith("EndTurn")) {
										int card = -2;
										rec = rec.substring(7);
										card = Integer.parseInt(rec);
										System.out.println(card);
										//didn't play card
										if (card == -1) {
											clientInfo.room.passTurnNum = 1;
											if(clientInfo.room.gameStatus == General) {
												sendQueue.add("D" + clientInfo.room.drawCard());
												clientInfo.room.broadcast("AD"+clientInfo.room.players.indexOf(clientInfo), clientInfo);
											}
											else if(clientInfo.room.gameStatus == Attack) {
												for(int i=0;i<clientInfo.room.cardPenalty;i++) {
													sendQueue.add("D" + clientInfo.room.drawCard());
													clientInfo.room.broadcast("AD"+clientInfo.room.players.indexOf(clientInfo), clientInfo);
												}
												clientInfo.room.gameStatus = General;
												clientInfo.room.cardPenalty = 0;
											}
											
											clientInfo.room.broadcast("OOpenedCard-1", null);
											clientInfo.room.broadcast("OCardPenalty"+clientInfo.room.cardPenalty, null);
										}
										//played card
										else {
											clientInfo.room.usedCards.add(0, card);
											clientInfo.room.openedCard = card;
											clientInfo.room.broadcast("AP" + clientInfo.room.players.indexOf(clientInfo), clientInfo);
											clientInfo.room.broadcast("OOpenedCard" + card, null);
											
											//seven
											if((card % 13 + 1) == 7) {
												sendQueue.add("OChooseSuit");
												continue;
											}
											else {
												clientInfo.room.cardEffect(card);
												clientInfo.room.broadcast("OCardPenalty"+clientInfo.room.cardPenalty, null);
												clientInfo.room.broadcast("ODirection"+clientInfo.room.direction, null);
											}
										}

										if(clientInfo.room.direction == CounterClockwise) {
											clientInfo.room.nextTurn = (clientInfo.room.players.indexOf(clientInfo) + clientInfo.room.passTurnNum) % 4;
										}
										else {
											clientInfo.room.nextTurn = (clientInfo.room.players.indexOf(clientInfo) - clientInfo.room.passTurnNum + 4) % 4;
										}
										
										clientInfo.room.broadcast("OStartTurn"+clientInfo.room.nextTurn, null);
										
									} else if(rec.startsWith("ChooseSuit")) {
										int suit = -1;
										rec = rec.substring(10);
										suit = Integer.parseInt(rec);
										clientInfo.room.openedCard = suit*13 + 6;
										clientInfo.room.broadcast("OOpenedCard"+clientInfo.room.openedCard, null);
										clientInfo.room.broadcast("OCardPenalty"+clientInfo.room.cardPenalty, null);

										if(clientInfo.room.direction == CounterClockwise) {
											clientInfo.room.nextTurn = (clientInfo.room.players.indexOf(clientInfo) + clientInfo.room.passTurnNum) % 4;
										}
										else {
											clientInfo.room.nextTurn = (clientInfo.room.players.indexOf(clientInfo) - clientInfo.room.passTurnNum + 4) % 4;
										}
										clientInfo.room.broadcast("OStartTurn"+clientInfo.room.nextTurn, null);
										
									}
								}
								// wait
								else if (startChar == 'W') {
									rec = rec.substring(1);
									// exit
									if (rec.startsWith("Exit")) {
										rec = rec.substring(4);
										for (ClientInfo data : oneCardPlayerList) {
											if (data.name.equals(rec)) {
												System.out.println("remove");
												oneCardPlayerList.remove(data);
												break;
											}
										}
										System.out.println(oneCardPlayerList.size());
										
										for(int i=0;i<Math.min(oneCardPlayerList.size(), 4);i++) {
											oneCardPlayerList.get(i).sendQueue.add("W" + oneCardPlayerList.size());
										}
									}
									// waiting
									else {
										boolean nameExist = false;
										
										for(int i=0;i<Math.min(oneCardPlayerList.size(), 4);i++) {
											if(oneCardPlayerList.get(i).name.equals(rec)) {
												nameExist = true;
												break;
											}
										}

										// existing name
										if (nameExist) {
											sendQueue.add("WExisting Name");
										}
										// available name
										else {
											clientInfo.name = rec;
											oneCardPlayerList.add(clientInfo);
											for(int i=0;i<Math.min(oneCardPlayerList.size(), 4);i++) {
												oneCardPlayerList.get(i).sendQueue.add("W" + Math.min(oneCardPlayerList.size(), 4));
											}

											// game start
											if(oneCardPlayerList.size() >= 4) {
												ClientInfo[] clientList = new ClientInfo[4];
												for(int i=0;i<4;i++) {
													clientList[i] = oneCardPlayerList.remove(0);
												}
												
												Room newRoom = new Room(clientList);
												String data = "WStart" + newRoom.drawCard();
												for(ClientInfo client : newRoom.players) {
													data += ".";
													data += client.name;
												}
												newRoom.broadcast(data, null);
											}
										}
									}
								}
							}
							try {
								Thread.sleep(1);
							} catch (InterruptedException e) {
								e.printStackTrace();
							}
						}
						System.out.println("Done");
					}
				};
				threadPool.submit(receiveProcessThread);
			} catch (IOException e) {
				e.printStackTrace();
			}
		}
	}

}
