using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text;
using TMPro;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour
{

    public int cardSum, numOfAces, playerIdx = 0, playerPos;
    public List<int> availableCardIdxList;
    public List<GameManager.pii> cards;
    public List<GameObject> cardObjects;
    public bool isReady;

    public TextMeshProUGUI[] nameObjectList = new TextMeshProUGUI[4];

    // Start is called before the first frame update
    void Start() {
        GameManager.gm.player = this;
        cardSum = 0;
        numOfAces = 0;
        availableCardIdxList = new List<int>();
        cards = new List<GameManager.pii>();
        cardObjects = new List<GameObject>();
        isReady = false;

        if (SceneManager.GetActiveScene().name.Equals("OneCard")) {
            GameManager.OneCard.players.Add(GameManager.gm.player);
            startOneCard();
        }
    }

    // Update is called once per frame
    void Update()
    {
        while (NetworkManager.nm.receivedDrawQueue.Count > 0) {
            drawCardOneCard(NetworkManager.nm.receivedDrawQueue.Dequeue());
        }
        if (GameManager.OneCard.isPvP && !isReady && cards.Count == 7) {
            for (int i = 0; i < GameManager.gm.player.cards.Count; i++) {
                cardObjects[i].GetComponent<PlayerCardScript>().sprite.color = Color.gray;
            }
            isReady = true;
            NetworkManager.nm.sendQueue.Enqueue("OReady");
        }
    }

    public void startBlackJack() {
        for (int i = 0; i < 2; i++) {
            drawCardBlackJack();
        }
    }

    public void startOneCard() {
        playerIdx = GameManager.OneCard.playerIdx;
        playerPos = 0;
        GameManager.OneCard.participants.Insert(0, new GameManager.pii(GameManager.OneCard.PlayerCode, 0));
        if (GameManager.OneCard.isPvP) {
            for (int i = 0; i < 3; i++) {
                Instantiate(GameManager.gm.enemyPrefab);
            }
        }
        else if (!GameManager.OneCard.isPvP) {
            for (int i = 0; i < 3; i++) {
                Instantiate(GameManager.gm.computerPrefab);
            }
        }

        for(int i = 0; i < 4; i++) {
            nameObjectList[i].text = GameManager.OneCard.nameList[(playerIdx + i) % 4];
        }

        for (int i = 0; i < 4; i++) {
            GameManager.OneCard.participantsPosList.Add(-1);
        }
        for (int i = 0; i < 4; i++) {
            GameManager.OneCard.participantsPosList[(playerIdx + i) % 4] = i;
        }

        for (int i = 0; i < 7; i++) {
            if (GameManager.OneCard.isPvP) {
                Debug.Log("Draw");
                NetworkManager.nm.sendQueue.Enqueue("D");
            } else {
                drawCardOneCard();
            }
        }

        if (GameManager.OneCard.isPvP) {

        } else {
            if (GameManager.OneCard.currentTurn == playerIdx) {
                startTurnOneCard();
            } else {
                GameObject.Find("Deck").GetComponent<SpriteRenderer>().color = Color.gray;
                for (int i = 0; i < GameManager.gm.player.cards.Count; i++) {
                    cardObjects[i].GetComponent<PlayerCardScript>().sprite.color = Color.gray;
                }
            }
        }
        
    }

    public void drawCardBlackJack() {
        int card = GameManager.BlackJack.drawCard();
        float offset = 3f, startX = 0;
        Vector3 cardPos = new Vector3(0, 0, 0);
        cards.Add(new GameManager.pii(card / GameManager.cardsPerSuit
            , card % GameManager.cardsPerSuit + 1));
        cardObjects.Add(Instantiate(GameManager.gm.playerCardPrefab));
        cardObjects[cardObjects.Count - 1].name = "PlayerCard" + (cards.Count - 1);
        cardObjects[cardObjects.Count - 1].GetComponent<SpriteRenderer>().sprite =
            GameManager.gm.cardSprites[cards[cards.Count - 1].first
            * GameManager.cardsPerSuit + cards[cards.Count - 1].second - 1];
        if (cards.Count > 3) {
            offset = 8f / (cards.Count - 1);
        }
        startX = -offset * (cards.Count - 1) / 2 + 3f;

        for (int i = 0; i < cards.Count; i++) {
            cardPos.x = startX + offset * i;
            cardPos.z = -i * 0.01f;
            cardObjects[i].transform.position = cardPos;
        }

        if (card % GameManager.cardsPerSuit + 1 < 10) {
            cardSum += (card % GameManager.cardsPerSuit + 1);
        } else {
            cardSum += 10;
        }

        if ((card % GameManager.cardsPerSuit + 1) == 1) {
            numOfAces++;
        }

        if (cardSum > 21) {
            endTurnBlackJack();
        }
    }

    public void drawCardOneCard(int serverCard = 0) {
        int card = 0;
        if (GameManager.OneCard.isPvP) {
            card = serverCard;
        } else {
            card = GameManager.OneCard.drawCard();
        }

        cards.Add(new GameManager.pii(card / GameManager.cardsPerSuit
            , card % GameManager.cardsPerSuit + 1));
        cardObjects.Add(Instantiate(GameManager.gm.playerCardPrefab));
        cardObjects[cardObjects.Count - 1].name = "PlayerCard" + (cards.Count - 1);
        cardObjects[cardObjects.Count - 1].GetComponent<PlayerCardScript>().card = card;
        cardObjects[cardObjects.Count - 1].GetComponent<PlayerCardScript>().sprite.color = Color.gray;
        cardObjects[cardObjects.Count - 1].transform.localScale = new Vector3(0.8f, 0.8f, 1f);
        cardObjects[cardObjects.Count - 1].transform.position = GameManager.OneCard.cardPos[0, playerPos]
            + GameManager.OneCard.cardVec[playerPos < (3 - playerPos) ? playerPos : (3 - playerPos)] * (cards.Count - 1);
        StartCoroutine(GameManager.OneCard.cardAction(GameObject.Find("Deck").transform.position, cardObjects[cardObjects.Count - 1].transform.position, 0.1f, playerPos, null));
    }
    public void startTurnOneCard() {
        GameObject.Find("Deck").GetComponent<SpriteRenderer>().color = Color.white;
        availableCardIdxList.Clear();
        GameManager.OneCard.findAvailableCards(ref availableCardIdxList, ref cards);
        int count = 0;
        for(int i = 0; i < cards.Count; i++) {
            if (count < availableCardIdxList.Count) {
                if (i == availableCardIdxList[count]) {
                    cardObjects[i].GetComponent<PlayerCardScript>().sprite.color = Color.white;
                    count++;
                } else {
                    cardObjects[i].GetComponent<PlayerCardScript>().sprite.color = Color.gray;
                }
            } else {
                cardObjects[i].GetComponent<PlayerCardScript>().sprite.color = Color.gray;
            }
        }
    }

    public void endTurnBlackJack() {
        GameManager.BlackJack.setAllBtnsInteractable(false);
        for (int i = 0; i < numOfAces; i++) {
            if (cardSum + 10 <= 21) {
                cardSum += 10;
            } else {
                break;
            }
        }

        GameManager.gm.computer.revealCards();

        if (cardSum <= 21) {
            if (GameManager.gm.computer.cardSum > 21) {
                winBlackJack();
            } else {
                if (cardSum > GameManager.gm.computer.cardSum) {
                    winBlackJack();
                } else if (cardSum == GameManager.gm.computer.cardSum) {
                    if (cards.Count == 2) {
                        winBlackJack();
                    } else {
                        drawBlackJack();
                    }
                } else {
                    loseBlackJack();
                }
            }
        } else {
            loseBlackJack();
        }

        GameManager.BlackJack.setExitBtnsInteractable(true);
        GameManager.BlackJack.setRestartBtnsInteractable(true);
        GameManager.BlackJack.setGameBtnsInteractable(false);
        GameManager.BlackJack.setBetBtnsInteractable(false);
    }
    public void endTurnOneCard(bool didPlayACard, int playedCard = -1) {
        PlayerCardScript playerCard;
        GameObject.Find("Deck").GetComponent<SpriteRenderer>().color = Color.gray;
        for (int i = 0; i < GameManager.gm.player.cards.Count; i++) {
            playerCard = cardObjects[i].GetComponent<PlayerCardScript>();
            playerCard.sprite.color = Color.gray;
            playerCard.overStack = 1;
            playerCard.isMouseOver = false;
            cardObjects[i].transform.position = GameManager.OneCard.cardPos[0, 0] + GameManager.OneCard.cardVec[0] * i;
        }

        if (GameManager.OneCard.isPvP) {
            NetworkManager.nm.sendQueue.Enqueue("OEndTurn" + playedCard);
        } else {
            if (!didPlayACard) {
                if (GameManager.OneCard.gameStatus == GameManager.OneCard.General) {
                    drawCardOneCard();
                } else {
                    for (int i = 0; i < GameManager.OneCard.cardPenalty; i++) {
                        drawCardOneCard();
                    }
                    GameManager.OneCard.gameStatus = GameManager.OneCard.General;
                    GameManager.OneCard.cardPenalty = 0;
                }
            }

            if (GameManager.OneCard.statusUGUI == null) {
                GameManager.OneCard.statusUGUI = GameObject.Find("Status").GetComponent<TextMeshProUGUI>();
            }
            if (GameManager.OneCard.penatlyUGUI == null) {
                GameManager.OneCard.penatlyUGUI = GameObject.Find("Penalty").GetComponent<TextMeshProUGUI>();
            }

            if (GameManager.OneCard.gameStatus == GameManager.OneCard.General) {
                GameManager.OneCard.statusUGUI.color = new Color(100f / 255, 100f / 255, 100f / 255);
                GameManager.OneCard.penatlyUGUI.color = new Color(100f / 255, 100f / 255, 100f / 255);
            } else if (GameManager.OneCard.gameStatus == GameManager.OneCard.Attack) {
                GameManager.OneCard.statusUGUI.color = Color.white;
                GameManager.OneCard.penatlyUGUI.color = Color.white;
            }
            GameManager.OneCard.penatlyUGUI.text = "" + GameManager.OneCard.cardPenalty;


            if (GameManager.OneCard.counterClockwiseImage == null) {
                GameManager.OneCard.counterClockwiseImage = GameObject.Find("CounterClockwise").GetComponent<Image>();
            }
            if (GameManager.OneCard.clockwiseImage == null) {
                GameManager.OneCard.clockwiseImage = GameObject.Find("Clockwise").GetComponent<Image>();
            }

            if (GameManager.OneCard.direction == GameManager.OneCard.CounterClockwise) {
                GameManager.OneCard.counterClockwiseImage.color = Color.white;
                GameManager.OneCard.clockwiseImage.color = new Color(100f / 255, 100f / 255, 100f / 255);
                GameManager.OneCard.currentTurn = (GameManager.OneCard.currentTurn + GameManager.OneCard.nextTurnNum)
                    % GameManager.OneCard.participantNum;
            } else {
                GameManager.OneCard.counterClockwiseImage.color = new Color(100f / 255, 100f / 255, 100f / 255);
                GameManager.OneCard.clockwiseImage.color = Color.white;
                GameManager.OneCard.currentTurn = (GameManager.OneCard.currentTurn + GameManager.OneCard.participantNum
                    - GameManager.OneCard.nextTurnNum) % GameManager.OneCard.participantNum;
            }

            GameManager.pii nextParticipant = GameManager.OneCard.participants[GameManager.OneCard.currentTurn];
            if (nextParticipant.first == GameManager.OneCard.PlayerCode) {
                GameManager.OneCard.players[nextParticipant.second].startTurnOneCard();
            } else {
                GameManager.OneCard.computers[nextParticipant.second].startTurnOneCard();
            }

            GameManager.OneCard.nextTurnNum = 1;
        }
    }

    public void winBlackJack() {
        GameManager.currentChips += GameManager.BlackJack.betAmount;
        GameManager.BlackJack.chipNumText.text = GameManager.currentChips + " chips";
        GameManager.BlackJack.messageText.text = "You win!\nGot "
            + GameManager.BlackJack.betAmount + " chips.";
    }
    public void drawBlackJack() {
        GameManager.BlackJack.messageText.text = "Draw!\nNo change to money";
    }
    public void loseBlackJack() {
        GameManager.currentChips -= GameManager.BlackJack.betAmount;
        GameManager.BlackJack.chipNumText.text = GameManager.currentChips + " chips";
        GameManager.BlackJack.messageText.text = "You lose!\nLost "
            + GameManager.BlackJack.betAmount + " chips.";
    }
}
