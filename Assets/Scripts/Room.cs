using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room
{

	public List<ClientInfo> players;

	public List<int> currentDeck, usedCards;
	public int openedCard;
	public int readyCount, passTurnNum, cardPenalty, gameStatus, currentTurn, nextTurn, direction;

	System.Random random = new System.Random();

	public Room(ClientInfo[] clients) {
		if (clients != null) {
			players = new List<ClientInfo>();
			foreach (ClientInfo client in clients) {
				client.room = this;
				players.Add(client);
			}
		} else {
			players = null;
		}

		currentDeck = new List<int>();
		usedCards = new List<int>();
		openedCard = 0;

		readyCount = 0;
		passTurnNum = 0;
		cardPenalty = 0;
		gameStatus = NetworkManager.General;
		currentTurn = 0;
		nextTurn = 0;
		direction = NetworkManager.CounterClockwise;

		refreshDeck(true);
	}

	public void refreshDeck(bool isStart) {
		if (isStart) {
			usedCards.Clear();
			currentDeck.Clear();
			for (int i = 0; i < NetworkManager.totalCardsNum; i++) {
				currentDeck.Add(i);
			}
		} else {
			currentDeck.Clear();
			for (int i = 1; i < usedCards.Count; i++) {
				currentDeck.Add(usedCards[i]);
			}

			while (usedCards.Count != 1) {
				usedCards.Remove(1);
			}
		}
	}

	public int drawCard() {
		int idx, ret;
		if (currentDeck.Count == 0) {
			refreshDeck(false);
		}
		idx = random.Next(currentDeck.Count);
		ret = currentDeck[idx];
		currentDeck.RemoveAt(idx);
		Debug.Log("Drawed: " + ret);
		return ret;
	}

	public void cardEffect(int card) {
		int suit = card / 13;
		int num = card % 13 + 1;
		passTurnNum = 1;
		if (suit == 4) {
			gameStatus = NetworkManager.Attack;
			if (num == 1) {
				cardPenalty += 5;
			} else if (num == 2) {
				cardPenalty += 7;
			}
		} else {
			if (num == 1) {
				gameStatus = NetworkManager.Attack;
				if (suit == NetworkManager.Spade) {
					cardPenalty += 5;
				} else {
					cardPenalty += 3;
				}
			} else if (num == 2) {
				gameStatus = NetworkManager.Attack;
				cardPenalty += 2;
			} else if (num == 3) {
				gameStatus = NetworkManager.General;
				cardPenalty = 0;
			} else if (num == 11) {
				passTurnNum = 2;
			} else if (num == 12) {
				direction = (1 - direction);
			} else if (num == 13) {
				passTurnNum = 0;
			}
		}
	}

	public void broadcast(string data, ClientInfo except) {
		if (players != null) {
			foreach (ClientInfo client in players) {
				if (!client.Equals(except))
					client.sendQueue.Enqueue(data);
			}
			Debug.Log("success");
		}
	}
}
