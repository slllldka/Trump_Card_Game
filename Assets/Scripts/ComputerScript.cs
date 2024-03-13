using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
public class ComputerScript : MonoBehaviour
{

    public int cardSum, numOfAces, playerIdx = 0, playerPos = 0;
    public List<int> availableCardIdxList;
    public List<GameManager.pii> cards;
    public List<GameObject> cardObjects;

    // Start is called before the first frame update
    void Start() {
        GameManager.gm.computer = this;
        cardSum = 0;
        numOfAces = 0;
        availableCardIdxList = new List<int>();
        cards = new List<GameManager.pii>();
        cardObjects = new List<GameObject>();
        if (SceneManager.GetActiveScene().name.Equals("OneCard")) {
            GameManager.OneCard.computers.Add(this);
            playerIdx = GameManager.OneCard.computers.Count;
            playerPos = GameManager.OneCard.computers.Count;
            GameManager.OneCard.participants.Add(new GameManager.pii(GameManager.OneCard.ComputerCode, GameManager.OneCard.computers.Count - 1));
            startOneCard();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void startBlackJack() {
        drawCardsBlackJack();
    }
    public void startOneCard() {
        for(int i = 0; i < 7; i++) {
            drawCardOneCard();
        }
        if (GameManager.OneCard.currentTurn == playerIdx) {
            startTurnOneCard();
        } else
            for (int i = 0; i < cards.Count; i++) {
            cardObjects[i].GetComponent<SpriteRenderer>().color = Color.gray;
        }
    }
    public void drawCardsBlackJack() {
        int card = 0;
        for (int i = 0; i < 3; i++) {
            card = GameManager.BlackJack.drawCard();
            cards.Add(new GameManager.pii(card / GameManager.cardsPerSuit
                , card % GameManager.cardsPerSuit + 1));
            if (card % GameManager.cardsPerSuit + 1 < 10) {
                cardSum += card % GameManager.cardsPerSuit + 1;
            } else {
                cardSum += 10;
            }
            if ((card % GameManager.cardsPerSuit + 1) == 1) {
                numOfAces++;
            }
        }

        for(int i = 0; i < numOfAces; i++) {
            if(cardSum + 10 <= 21) {
                cardSum += 10;
            } else {
                break;
            }
        }
    }
    public void drawCardOneCard() {
        int card = 0;
        card = GameManager.OneCard.drawCard();
        cards.Add(new GameManager.pii(card / GameManager.cardsPerSuit
            , card % GameManager.cardsPerSuit + 1));
        cardObjects.Add(Instantiate(GameManager.gm.enemyCardPrefab));
        cardObjects[cardObjects.Count - 1].name = "EnemyCard" + playerIdx.ToString() + (cards.Count - 1);
        cardObjects[cardObjects.Count - 1].transform.localScale = new Vector3(0.6f, 0.6f, 1f);
        cardObjects[cardObjects.Count - 1].transform.position = GameManager.OneCard.cardPos[(cardObjects.Count - 1) / GameManager.OneCard.cardsPerLayer, playerPos]
            + GameManager.OneCard.cardVec[playerPos < (3 - playerPos) ? playerPos : (3 - playerPos)] * ((cards.Count - 1) % GameManager.OneCard.cardsPerLayer) * 3 / 4
            + new Vector3(0, 0, -0.1f) * ((cards.Count - 1) - (cards.Count - 1) % GameManager.OneCard.cardsPerLayer) * 3 / 4;
        //cardObjects[cardObjects.Count - 1].transform.rotation = Quaternion.Euler(GameManager.OneCard.rotations[playerPos % 2]);
        StartCoroutine(GameManager.OneCard.cardAction(GameObject.Find("Deck").transform.position, cardObjects[cardObjects.Count - 1].transform.position, 0.1f, playerPos, null));
    }

    public void revealCards() {
        GameObject[] cardObjects = GameObject.FindGameObjectsWithTag("EnemyCard");
        for(int i = 0; i < cards.Count; i++) {
            cardObjects[i].GetComponent<SpriteRenderer>().sprite =
                GameManager.gm.cardSprites[cards[i].first
                * GameManager.cardsPerSuit + cards[i].second - 1];
        }
    }
    public void startTurnOneCard() {
        availableCardIdxList.Clear();
        GameManager.OneCard.findAvailableCards(ref availableCardIdxList, ref cards);
        for (int i = 0; i < cards.Count; i++) {
            cardObjects[i].GetComponent<SpriteRenderer>().color = Color.white;
        }

        if (availableCardIdxList.Count > 0) {
            int idx = Random.Range(0, availableCardIdxList.Count);
            StartCoroutine(playTurn(availableCardIdxList[idx]));
        } else {
            StartCoroutine(playTurn());
        }
    }

    IEnumerator playTurn(int idx = -1) {
        yield return new WaitForSeconds(3f);
        if (idx != -1) {
            playCard(idx);
        } else {
            endTurnOneCard(false);
        }
    }

    public void playCard(int idx) {
        bool destroyed = false;
        GameObject playedCard = cardObjects[idx];
        for (int i = 0; i < cards.Count; i++) {
            if(i == idx && !destroyed) {
                GameManager.OneCard.usedCards.Insert(0, cards[i].first * GameManager.cardsPerSuit + cards[i].second - 1);
                GameManager.OneCard.openedCard.card = GameManager.OneCard.usedCards[0];
                cards.RemoveAt(i);
                cardObjects.RemoveAt(i);
                destroyed = true;
                i--;
            } else {
                if (destroyed) {
                    cardObjects[i].name = "EnemyCard" + playerIdx.ToString() + i;
                    cardObjects[i].transform.position = GameManager.OneCard.cardPos[i / GameManager.OneCard.cardsPerLayer, playerPos]
                        + GameManager.OneCard.cardVec[playerPos < (3 - playerPos) ? playerPos : (3 - playerPos)] * (i % GameManager.OneCard.cardsPerLayer) * 3 / 4
                        + new Vector3(0, 0, -0.1f) * (i - i % GameManager.OneCard.cardsPerLayer) * 3 / 4;
                }
            }
        }

        if (GameManager.OneCard.openedCard.card % GameManager.cardsPerSuit + 1 == 7) {
            StartCoroutine(GameManager.OneCard.cardEffect(GameManager.OneCard.openedCard.card / GameManager.cardsPerSuit
                , GameManager.OneCard.openedCard.card % GameManager.cardsPerSuit + 1, Random.Range(0, 4)));
        } else {
            StartCoroutine(GameManager.OneCard.cardEffect(GameManager.OneCard.openedCard.card / GameManager.cardsPerSuit
                , GameManager.OneCard.openedCard.card % GameManager.cardsPerSuit + 1));
        }

        playedCard.transform.position += new Vector3(0, 0, -10f);
        StartCoroutine(GameManager.OneCard.cardAction(playedCard.transform.position, GameManager.OneCard.openedCard.gameObject.transform.position, 0.1f, playerPos, playedCard));
    }

    public void endTurnOneCard(bool didPlayACard) {
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

        for (int i = 0; i < cards.Count; i++) {
            cardObjects[i].GetComponent<SpriteRenderer>().color = Color.gray;
        }

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
