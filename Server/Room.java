package server;

import java.util.ArrayList;
import java.util.Random;

public class Room {
	public ArrayList<ClientInfo> players;
	
	public ArrayList<Integer> currentDeck, usedCards;
	public int openedCard;
	public int readyCount, passTurnNum, cardPenalty, gameStatus, currentTurn, nextTurn, direction;
	
	public Random random;
	
	public Room(ClientInfo... clients) {
		players = new ArrayList<ClientInfo>();
		for(ClientInfo client : clients) {
			client.room = this;
			players.add(client);
		}
		
		currentDeck = new ArrayList<Integer>();
		usedCards = new ArrayList<Integer>();
		openedCard = 0;
		
		readyCount = 0;
		passTurnNum = 0;
		cardPenalty = 0;
		gameStatus = Server.General;
		currentTurn = 0;
		nextTurn = 0;
		direction = Server.CounterClockwise;
		
		random = new Random();
		refreshDeck(true);
	}
	
	public void refreshDeck(boolean isStart) {
		if (isStart) {
			usedCards.clear();
			currentDeck.clear();
			for (int i = 0; i < Server.totalCardsNum; i++) {
				currentDeck.add(i);
			}
		} else {
			currentDeck.clear();
			for (int i = 1; i < usedCards.size(); i++) {
				currentDeck.add(usedCards.get(i));
			}
			
			while(usedCards.size() != 1) {
				usedCards.remove(1);
			}
		}
	}
	
	public int drawCard() {
		if (currentDeck.size() == 0) {
			refreshDeck(false);
		}
		int idx = random.nextInt(currentDeck.size());
		int ret = currentDeck.get(idx);
		currentDeck.remove(idx);
		return ret;
	}
	
	public void cardEffect(int card) {
		int suit = card / 13;
		int num = card % 13 + 1;
        passTurnNum = 1;
        if(suit == 4) {
            gameStatus = Server.Attack;
            if(num == 1) {
                cardPenalty += 5;
            } else if(num == 2) {
                cardPenalty += 7;
            }
        } else {
            if(num == 1) {
                gameStatus = Server.Attack;
                if(suit == Server.Spade) {
                    cardPenalty += 5;
                } else {
                    cardPenalty += 3;
                }
            }
            else if(num == 2) {
                gameStatus = Server.Attack;
                cardPenalty += 2;
            }
            else if(num == 3) {
                gameStatus = Server.General;
                cardPenalty = 0;
            }
            else if(num == 11) {
                passTurnNum = 2;
            }
            else if(num == 12) {
                direction = (1 - direction);
            }
            else if(num == 13) {
                passTurnNum = 0;
            }
        }
    }
	
	public void broadcast(String data, ClientInfo except) {
		for(ClientInfo client : players) {
			if(!client.equals(except))
				client.sendQueue.add(data);
		}
		System.out.println("success");
	}
	
	public void HolePunch() {
		String data_leader = "HolePunchL";
		String data_participant = "HolePunchP";
		for(int i=1;i<4;i++) {
			data_leader += players.get(i).name;
			data_leader += ':';
			data_leader += players.get(i).socket.getInetAddress().getHostAddress();
			data_leader += ':';
			data_leader += players.get(i).udp_port;
			data_leader +='/';
		}
		players.get(0).sendQueue.add(data_leader);

		data_participant += players.get(0).socket.getInetAddress().getHostAddress();
		data_participant += ':';
		data_participant += players.get(0).udp_port;
		broadcast(data_participant, players.get(0));
	}
}
