using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public class pii {
        public int first, second;
        public pii() {
            first = 0;
            second = 0;
        }
        public pii(int _first, int _second) {
            first = _first;
            second = _second;
        }
    }

    public static GameManager gm = null;

    public static readonly int Spade = 0, Diamond = 1, Heart = 2, Club = 3
        , numsPerSuit = 10, cardsPerSuit = 13;

    public List<Sprite> cardSprites;

    public static int currentChips;

    public GameObject closedCardPrefab, openedCardPrefab, enemyCardPrefab, playerCardPrefab, computerPrefab, enemyPrefab;

    public ComputerScript computer;
    public PlayerScript player;

    //BlackJack
    public class BlackJack {
        public static TextMeshProUGUI betAmountText, cardNumText, chipNumText, messageText;
        public static List<Button> exitBtns = new List<Button>();
        public static List<Button> restartBtns = new List<Button>();
        public static List<Button> gameBtns = new List<Button>();
        public static List<Button> betBtns = new List<Button>();
        public static int totalCardsNum = 0, betAmount = 0;
        public static List<int> currentDeck = new List<int>();

        public static void start() {
            totalCardsNum = 52;
            refreshDeck();
            currentChips = 100;
            betAmount = 0;
            SceneManager.LoadScene("BlackJack");
        }
        public static void restart() {
            refreshDeck();
            betAmount = 0;
            SceneManager.LoadScene("BlackJack");
        }
        public static void refreshDeck() {
            currentDeck.Clear();
            for (int i = 0; i < totalCardsNum; i++) {
                currentDeck.Add(i);
            }
        }

        public static int drawCard() {
            int idx = Random.Range(0, currentDeck.Count);
            int ret = currentDeck[idx];
            currentDeck.RemoveAt(idx);
            BlackJack.cardNumText.text = currentDeck.Count.ToString();
            return ret;
        }

        public static void setAllBtnsInteractable(bool val) {
            foreach (Button btn in BlackJack.exitBtns) {
                btn.interactable = val;
            }
            foreach (Button btn in BlackJack.restartBtns) {
                btn.interactable = val;
            }
            foreach (Button btn in BlackJack.gameBtns) {
                btn.interactable = val;
            }
            foreach (Button btn in BlackJack.betBtns) {
                btn.interactable = val;
            }
        }
        public static void setExitBtnsInteractable(bool val) {
            foreach (Button btn in BlackJack.exitBtns) {
                btn.interactable = val;
            }
        }
        public static void setRestartBtnsInteractable(bool val) {
            foreach (Button btn in BlackJack.restartBtns) {
                btn.interactable = val;
            }
        }
        public static void setGameBtnsInteractable(bool val) {
            foreach (Button btn in BlackJack.gameBtns) {
                btn.interactable = val;
            }
        }
        public static void setBetBtnsInteractable(bool val) {
            foreach (Button btn in BlackJack.betBtns) {
                btn.interactable = val;
            }
        }
    }
    

    //OneCard
    public class OneCard {
        public static GameObject waitWindow;

        public static bool isPvP = false;
        public static readonly int PlayerCode = 0, ComputerCode = 1, EnemyCode = 2;
        public static readonly int CounterClockwise = 0, Clockwise = 1;
        public static int participantNum = 0, currentTurn = 0;
        public static List<pii> participants = new List<pii>();
        public static List<int> participantsPosList = new List<int>();
        //public static Vector3[] positions = { new Vector3(0, -3f, -5f), new Vector3(7f, 0, -5f), new Vector3(0, 3f, -5f), new Vector3(-7f, 0, -5f) };
        public static Vector3[] positions = { new Vector3(-7f, -3f, -5f), new Vector3(7f, -3f, -5f), new Vector3(7f, 3f, -5f), new Vector3(-7f, 3f, -5f) };
        public static Vector3[] rotations = { new Vector3(0, 0, 0), new Vector3(0, 0, 90f) };
        public static Vector3[,] cardPos = { { new Vector3(-7f, -3f, -0.1f), new Vector3(7f, -3f, -0.1f), new Vector3(7f, 3f, -0.1f), new Vector3(-7f, 3f, -0.1f) }, {
            new Vector3(-7f, -3f, -0.1f), new Vector3(6.8f, -2.5f, -0.1f), new Vector3(6.8f, 2.5f, -0.1f), new Vector3(-6.8f, 2.5f, -0.1f)},  {
            new Vector3(-7f, -3f, -0.1f), new Vector3(6.6f, -2f, -0.1f), new Vector3(6.6f, 2f, -0.1f), new Vector3(-6.6f, 2f, -0.1f)} };
        public static Vector3[] cardVec = { new Vector3(0.4f, 0, -0.1f), new Vector3(-0.4f, 0, -0.1f)};

        public static int cardsPerLayer = 10;

        public static List<PlayerScript> players = new List<PlayerScript>();
        public static List<ComputerScript> computers = new List<ComputerScript>();
        public static List<EnemyScript> enemys = new List<EnemyScript>();

        public static string name;
        public static List<string> nameList = new List<string>();
        public static int playerIdx = 0;

        public static int direction = 0, nextTurnNum = 1;

        public static List<int> usedCards = new List<int>();
        public static OpenedCardScript openedCard;
        public static int totalCardsNum = 0;
        public static List<int> currentDeck = new List<int>();

        public static readonly int General = 0, Attack = 1;
        public static int gameStatus = 0;
        public static int cardPenalty = 0;

        public static bool receivedOpenedCard, receivedCardPenalty;

        public static Button spadeBtn, diamondBtn, heartBtn, clubBtn;
        public static TextMeshProUGUI statusUGUI = null, penatlyUGUI = null;
        public static Image counterClockwiseImage = null, clockwiseImage = null;

        public static void wait() {
            waitWindow.GetComponent<WaitWindowScript>().open();
        }
        public static void start(bool _isPvP, int startCard = -1) {
            isPvP = _isPvP;
            if (isPvP) {
                playerIdx = nameList.IndexOf(name);
            } else {
                playerIdx = 0;
                nameList.Clear();
                nameList.Add("Me");
                nameList.Add("C1");
                nameList.Add("C2");
                nameList.Add("C3");
            }
            participantNum = 4;
            participants.Clear();
            participantsPosList.Clear();
            players.Clear();
            computers.Clear();
            enemys.Clear();
            totalCardsNum = 54;
            refreshDeck(true);
            //OneCard.currentPlayer = Random.Range(0, playerNum);
            currentTurn = 0;
            gameStatus = General;
            cardPenalty = 0;
            direction = CounterClockwise;
            receivedOpenedCard = true;
            receivedCardPenalty = true;

            if(startCard != -1) {
                usedCards.Add(startCard);
            }

            SceneManager.LoadScene("OneCard");
        }
        public static void restart() {
            usedCards.Clear();
            refreshDeck(true);
            //OneCard.currentPlayer = Random.Range(0, playerNum);
            currentTurn = 0;
            gameStatus = General;
            cardPenalty = 0;
            direction = CounterClockwise;
            receivedOpenedCard = true;
            receivedCardPenalty = true;

            SceneManager.LoadScene("OneCard");
        }
        public static void refreshDeck(bool isStart) {
            if (isStart) {
                usedCards.Clear();
                currentDeck.Clear();
                for (int i = 0; i < totalCardsNum; i++) {
                    currentDeck.Add(i);
                }
            } else {
                Debug.Log(usedCards.Count + " " + currentDeck.Count);
                currentDeck = usedCards.ToList();
                currentDeck.RemoveAt(0);
                usedCards.RemoveRange(1, usedCards.Count - 1);
                Debug.Log(usedCards.Count + " " + currentDeck.Count);
            }
        }

        public static int drawCard() {
            if(currentDeck.Count == 0) {
                refreshDeck(false);
            }
            int idx = Random.Range(0, currentDeck.Count);
            int ret = currentDeck[idx];
            currentDeck.RemoveAt(idx);
            return ret;
        }

        public static void findAvailableCards(ref List<int> idxList, ref List<pii> cardList) {
            int idx = 0;
            pii topCard = new pii(openedCard.card / cardsPerSuit, openedCard.card % cardsPerSuit + 1);
            foreach (pii targetCard in cardList) {
                if (isAvailableCard(topCard, targetCard)) {
                    idxList.Add(idx);
                }
                idx++;
            }
        }

        public static bool isAvailableCard(pii topCard, pii targetCard) {
            //Black Joker
            if (targetCard.first == 4 && targetCard.second == 1) {
                if(topCard.first == Spade || topCard.first == Club) {
                    return true;
                } else {
                    return false;
                }
            }
            //red Joker
            else if (targetCard.first == 4 && targetCard.second == 2) {
                if (topCard.first == Diamond || topCard.first == Heart) {
                    return true;
                } else if (topCard.first == 4 && topCard.second == 1) {
                    return true;
                } else {
                    return false;
                }
            } else {
                //Ace
                if(targetCard.second == 1) {
                    if(topCard.first == 4 && topCard.second == 1) {
                        if(targetCard.first == Spade) {
                            return true;
                        } else {
                            if (gameStatus == General) {
                                if(targetCard.first == Club) {
                                    return true;
                                } else {
                                    return false;
                                }
                            } else {
                                return false;
                            }
                        }
                    }
                    else if(topCard.first == 4 && topCard.second == 2) {
                        if (gameStatus == General) {
                            if(targetCard.first == Diamond || targetCard.first == Heart) {
                                return true;
                            } else {
                                return false;
                            }
                        } else {
                            return false;
                        }
                    }
                    else if(topCard.first == Spade && topCard.second == 1) {
                        if (gameStatus == General) {
                            return true;
                        } else {
                            return false;
                        }
                    }
                    else if(topCard.second == 1) {
                        return true;
                    } else {
                        if(targetCard.first == topCard.first) {
                            return true;
                        } else {
                            return false;
                        }
                    }
                }
                //two
                else if(targetCard.second == 2) {
                    if(topCard.second == 1) {
                        if (gameStatus == General) {
                            if (topCard.first == 4) {
                                if (targetCard.first == Spade || targetCard.first == Club) {
                                    return true;
                                } else {
                                    return false;
                                }
                            } else {
                                if(targetCard.first == topCard.first) {
                                    return true;
                                } else {
                                    return false;
                                }
                            }
                        } else {
                            return false;
                        }
                    }
                    else if(topCard.second == 2) {
                        if (topCard.first == 4) {
                            if (gameStatus == General) {
                                if(targetCard.first == Diamond || targetCard.first == Heart) {
                                    return true;
                                } else {
                                    return false;
                                }
                            } else {
                                return false;
                            }
                        } else {
                            return true;
                        }
                    }
                    else if(targetCard.first == topCard.first) {
                        return true;
                    } else {
                        return false;
                    }
                }
                //three
                else if(targetCard.second == 3) {
                    if(topCard.second == 1) {
                        if (gameStatus == General) {
                            if (targetCard.first == topCard.first) {
                                return true;
                            } else {
                                return false;
                            }
                        } else {
                            return false;
                        }
                    }
                    else if(topCard.second == 2) {
                        if(targetCard.first == topCard.first) {
                            return true;
                        } else {
                            return false;
                        }
                    }
                    else if(topCard.second == 3) {
                        return true;
                    }
                    else if(targetCard.first == topCard.first) {
                        return true;
                    } else {
                        return false;
                    }
                }
                //others
                else if(targetCard.second > 3) {
                    if (topCard.second == 1) {
                        if(topCard.first == 4) {
                            if (gameStatus == General) {
                                if (targetCard.first == Spade || targetCard.first == Club) {
                                    return true;
                                } else {
                                    return false;
                                }
                            } else {
                                return false;
                            }
                        } else {
                            if (gameStatus == General) {
                                if(targetCard.first == topCard.first) {
                                    return true;
                                } else {
                                    return false;
                                }
                            } else {
                                return false;
                            }
                        }
                    } else if (topCard.second == 2) {
                        if (topCard.first == 4) {
                            if (gameStatus == General) {
                                if (targetCard.first == Diamond || targetCard.first == Heart) {
                                    return true;
                                } else {
                                    return false;
                                }
                            } else {
                                return false;
                            }
                        } else {
                            if (gameStatus == General) {
                                if (targetCard.first == topCard.first) {
                                    return true;
                                } else {
                                    return false;
                                }
                            } else {
                                return false;
                            }
                        }
                    } else if (targetCard.first == topCard.first) {
                        return true;
                    } else if (targetCard.second == topCard.second) {
                        return true;
                    } else {
                        return false;
                    }
                } else {
                    return false;
                }
            }
        }

        //Card Actions
        public static IEnumerator cardEffect(int suit, int num, int changeSuit = -1) {
            nextTurnNum = 1;
            if(suit == 4) {
                gameStatus = Attack;
                if(num == 1) {
                    cardPenalty += 5;
                } else if(num == 2) {
                    cardPenalty += 7;
                }
            } else {
                if(num == 1) {
                    gameStatus = Attack;
                    if(suit == Spade) {
                        cardPenalty += 5;
                    } else {
                        cardPenalty += 3;
                    }
                }
                else if(num == 2) {
                    gameStatus = Attack;
                    cardPenalty += 2;
                }
                else if(num == 3) {
                    gameStatus = General;
                    cardPenalty = 0;
                }
                else if(num == 7) {
                    if (participants[currentTurn].first == ComputerCode) {
                        yield return new WaitForSeconds(3f);
                    }
                    openedCard.card = changeSuit*cardsPerSuit + openedCard.card % cardsPerSuit;
                }
                else if(num == 11) {
                    nextTurnNum = 2;
                }
                else if(num == 12) {
                    direction = (1 - direction);
                }
                else if(num == 13) {
                    nextTurnNum = 0;
                }
            }
            yield return null;

            GameManager.pii currentParticipant = participants[currentTurn];
            if (currentParticipant.first == PlayerCode) {
                players[currentParticipant.second].endTurnOneCard(true);
            } else {
                computers[currentParticipant.second].endTurnOneCard(true);
            }
        }

        public static IEnumerator cardAction(Vector3 start, Vector3 end, float time, int playerPos, GameObject playedCard) {
            GameObject cardImg = Instantiate(GameManager.gm.closedCardPrefab);
            //cardImg.transform.rotation = Quaternion.Euler(rotations[playerPos % 2]);
            for(int i = 0; i < 10; i++) {
                cardImg.transform.position = start + (end - start) * 0.1f * i;
                yield return new WaitForSeconds(time / 10f);
            }
            Destroy(cardImg);
            if (playedCard != null) {
                Destroy(playedCard);
            }
        }


        public static void setAllSuitBtnsInteractable(bool val) {
            spadeBtn.interactable = val;
            diamondBtn.interactable = val;
            heartBtn.interactable = val;
            clubBtn.interactable = val;
        }
    }

    private void Awake() {
        if (gm == null) {
            gm = this;
            DontDestroyOnLoad(gameObject);
            //Application.runInBackground = true;
        } else {
            Destroy(gameObject);
            return;
        }
    }

    // Start is called before the first frame update
    void Start() {
    }

    // Update is called once per frame
    void Update()
    {
        int idx;
        if (SceneManager.GetActiveScene().name.Equals("OneCard")) {
            if (player.isReady) {
                while (NetworkManager.nm.receivedCardDrawActionQueue.Count > 0) {
                    idx = NetworkManager.nm.receivedCardDrawActionQueue.Dequeue();
                    idx = GameManager.OneCard.participantsPosList[idx];
                    Debug.Log(idx + " " + GameManager.OneCard.enemys.Count);
                    GameManager.OneCard.enemys[idx - 1].drawCardOneCard();
                    //StartCoroutine(GameManager.OneCard.cardAction(GameObject.Find("Deck").transform.position
                    //   , GameManager.OneCard.positions[GameManager.OneCard.participantsPosList[idx]], 0.1f, GameManager.OneCard.participantsPosList[idx], null));
                }
                while (NetworkManager.nm.receivedCardPlayActionQueue.Count > 0) {
                    idx = NetworkManager.nm.receivedCardPlayActionQueue.Dequeue();
                    idx = GameManager.OneCard.participantsPosList[idx];
                    GameManager.OneCard.enemys[idx - 1].playCardOneCard();
                    //StartCoroutine(GameManager.OneCard.cardAction(GameManager.OneCard.positions[GameManager.OneCard.participantsPosList[idx]]
                    //    , GameManager.OneCard.openedCard.transform.position, 0.1f, GameManager.OneCard.participantsPosList[idx], null));
                }

                if (NetworkManager.nm.receivedOpenedCardQueue.Count > 0) {
                    int receivedOpenedCard = NetworkManager.nm.receivedOpenedCardQueue.Dequeue();
                    if (receivedOpenedCard != -1) {
                        GameManager.OneCard.openedCard.card = receivedOpenedCard;
                    }

                    GameManager.OneCard.receivedOpenedCard = true;
                }

                if (NetworkManager.nm.receivedCardPenaltyQueue.Count > 0) {
                    if(GameManager.OneCard.statusUGUI == null) {
                        GameManager.OneCard.statusUGUI = GameObject.Find("Status").GetComponent<TextMeshProUGUI>();
                    }
                    if (GameManager.OneCard.penatlyUGUI == null) {
                        GameManager.OneCard.penatlyUGUI = GameObject.Find("Penalty").GetComponent<TextMeshProUGUI>();
                    }

                    GameManager.OneCard.cardPenalty = NetworkManager.nm.receivedCardPenaltyQueue.Dequeue();

                    if(GameManager.OneCard.cardPenalty == 0) {
                        GameManager.OneCard.gameStatus = GameManager.OneCard.General;
                        GameManager.OneCard.statusUGUI.color = new Color(100f / 255, 100f / 255, 100f / 255);
                        GameManager.OneCard.penatlyUGUI.color = new Color(100f / 255, 100f / 255, 100f / 255);
                    }
                    else if(GameManager.OneCard.cardPenalty > 0) {
                        GameManager.OneCard.gameStatus = GameManager.OneCard.Attack;
                        GameManager.OneCard.statusUGUI.color = Color.white;
                        GameManager.OneCard.penatlyUGUI.color = Color.white;
                    }
                    GameManager.OneCard.penatlyUGUI.text = "" + GameManager.OneCard.cardPenalty;

                    GameManager.OneCard.receivedCardPenalty = true;
                }

                if(NetworkManager.nm.receivedDirectionQueue.Count > 0) {
                    if (GameManager.OneCard.counterClockwiseImage == null) {
                        GameManager.OneCard.counterClockwiseImage = GameObject.Find("CounterClockwise").GetComponent<Image>();
                    }
                    if (GameManager.OneCard.clockwiseImage == null) {
                        GameManager.OneCard.clockwiseImage = GameObject.Find("Clockwise").GetComponent<Image>();
                    }

                    GameManager.OneCard.direction = NetworkManager.nm.receivedDirectionQueue.Dequeue();
                    if(GameManager.OneCard.direction == GameManager.OneCard.CounterClockwise) {
                        GameManager.OneCard.counterClockwiseImage.color = Color.white;
                        GameManager.OneCard.clockwiseImage.color = new Color(100f / 255, 100f / 255, 100f / 255);
                    }
                    else if (GameManager.OneCard.direction == GameManager.OneCard.Clockwise) {
                        GameManager.OneCard.counterClockwiseImage.color = new Color(100f / 255, 100f / 255, 100f / 255);
                        GameManager.OneCard.clockwiseImage.color = Color.white;
                    }
                }

                if (NetworkManager.nm.receivedStartTurnQueue.Count > 0
                    && GameManager.OneCard.receivedOpenedCard && GameManager.OneCard.receivedCardPenalty) {
                    GameManager.OneCard.receivedOpenedCard = false;
                    GameManager.OneCard.receivedCardPenalty = false;
                    idx = NetworkManager.nm.receivedStartTurnQueue.Dequeue();
                    idx = GameManager.OneCard.participantsPosList[idx];
                    if (idx == 0) {
                        player.startTurnOneCard();
                    }

                    for (int i = 0; i < 3; i++) {
                        if (i == idx - 1) {
                            GameManager.OneCard.enemys[idx - 1].startTurnOneCard();
                        } else {
                            GameManager.OneCard.enemys[i].endTurnOneCard();
                        }
                    }
                }
            }
        }
    }

    public bool isSameColor(pii card1, pii card2) {
        return (card1.first + card2.first == 3);
    }
    public void exitApp() {
        Application.Quit();
    }
    public void openSelectGame() {
        SceneManager.LoadScene(1);
    }
    public void exitSelectGame() {
        SceneManager.LoadScene(0);
    }
    public void exitGame() {
        NetworkManager.nm.initiate_udp_related_resources();
        SceneManager.LoadScene(1);
    }
}
