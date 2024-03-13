using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ButtonScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start() {
        if (SceneManager.GetActiveScene().name.Equals("BlackJack")) {
            GameManager.BlackJack.exitBtns.Clear();
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("ExitBtn")) {
                GameManager.BlackJack.exitBtns.Add(go.GetComponent<Button>());
            }

            GameManager.BlackJack.restartBtns.Clear();
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("RestartBtn")) {
                GameManager.BlackJack.restartBtns.Add(go.GetComponent<Button>());
            }

            GameManager.BlackJack.gameBtns.Clear();
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("GameBtn")) {
                GameManager.BlackJack.gameBtns.Add(go.GetComponent<Button>());
            }

            GameManager.BlackJack.betBtns.Clear();
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("BetBtn")) {
                GameManager.BlackJack.betBtns.Add(go.GetComponent<Button>());
            }
            GameManager.BlackJack.setExitBtnsInteractable(true);
            GameManager.BlackJack.setRestartBtnsInteractable(false);
            GameManager.BlackJack.setGameBtnsInteractable(false);
            GameManager.BlackJack.setBetBtnsInteractable(true);
        }
        else if (SceneManager.GetActiveScene().name.Equals("OneCard")) {
            GameManager.OneCard.spadeBtn = GameObject.Find("SpadeBtn").GetComponent<Button>();
            GameManager.OneCard.diamondBtn = GameObject.Find("DiamondBtn").GetComponent<Button>();
            GameManager.OneCard.heartBtn = GameObject.Find("HeartBtn").GetComponent<Button>();
            GameManager.OneCard.clubBtn = GameObject.Find("ClubBtn").GetComponent<Button>();
            GameManager.OneCard.setAllSuitBtnsInteractable(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void exitApp() {
        GameManager.gm.exitApp();
    }
    public void openSelectGame() {
        GameManager.gm.openSelectGame();
    }
    public void exitSelectGame() {
        GameManager.gm.exitSelectGame();
    }

    public void startBlackJack() {
        GameManager.BlackJack.start();
    }
    public void restartBlackJack() {
        GameManager.BlackJack.restart();
    }
    public void waitOneCard() {
        GameManager.OneCard.wait();
    }
    public void startOneCard(bool isPvP) {
        GameManager.OneCard.start(isPvP);
    }
    public void restartOneCard() {
        GameManager.OneCard.restart();
    }
    public void exitGame() {
        GameManager.gm.exitGame();
    }
    public void betBlackJack() {
        string btnName = EventSystem.current.currentSelectedGameObject.name;
        int betAmount = 0;
        if (btnName.Equals("10%Btn")) {
            betAmount = (int)(GameManager.currentChips * 0.1f);
        }else if (btnName.Equals("30%Btn")) {
            betAmount = (int)(GameManager.currentChips * 0.3f);
        } else if (btnName.Equals("50%Btn")) {
            betAmount = (int)(GameManager.currentChips * 0.5f);
        } else if (btnName.Equals("70%Btn")) {
            betAmount = (int)(GameManager.currentChips * 0.7f);
        } else if (btnName.Equals("100%Btn")) {
            betAmount = GameManager.currentChips;
        }

        if(betAmount > 0) {
            GameManager.BlackJack.setAllBtnsInteractable(false);
            GameManager.BlackJack.betAmount = betAmount;
            GameManager.BlackJack.betAmountText.text = "Bet Amount\n" + betAmount + " chips";
            GameManager.BlackJack.messageText.text = "You bet " + betAmount + " chips";
            GameManager.gm.computer.startBlackJack();
            GameManager.gm.player.startBlackJack();

            GameManager.BlackJack.setExitBtnsInteractable(false);
            GameManager.BlackJack.setRestartBtnsInteractable(false);
            GameManager.BlackJack.setGameBtnsInteractable(true);
            GameManager.BlackJack.setBetBtnsInteractable(false);
        } else {
            GameManager.BlackJack.messageText.text = "Bet Amount less than 1 chip!";
        }
    }

    public void changeSuit() {
        int suit = GameManager.OneCard.openedCard.card / GameManager.cardsPerSuit;
        int num = GameManager.OneCard.openedCard.card % GameManager.cardsPerSuit + 1;
        string btnName = EventSystem.current.currentSelectedGameObject.name;
        if (btnName.Equals("SpadeBtn")) {
            if (GameManager.OneCard.isPvP) {
                NetworkManager.nm.sendQueue.Enqueue("OChooseSuit" + GameManager.Spade);
            } else {
                StartCoroutine(GameManager.OneCard.cardEffect(suit, num, GameManager.Spade));
            }
        } else if (btnName.Equals("DiamondBtn")) {
            if (GameManager.OneCard.isPvP) {
                NetworkManager.nm.sendQueue.Enqueue("OChooseSuit" + GameManager.Diamond);
            } else {
                StartCoroutine(GameManager.OneCard.cardEffect(suit, num, GameManager.Diamond));
            }
        } else if (btnName.Equals("HeartBtn")) {
            if (GameManager.OneCard.isPvP) {
                NetworkManager.nm.sendQueue.Enqueue("OChooseSuit" + GameManager.Heart);
            } else {
                StartCoroutine(GameManager.OneCard.cardEffect(suit, num, GameManager.Heart));
            }
        } else if (btnName.Equals("ClubBtn")) {
            if (GameManager.OneCard.isPvP) {
                NetworkManager.nm.sendQueue.Enqueue("OChooseSuit" + GameManager.Club);
            } else {
                StartCoroutine(GameManager.OneCard.cardEffect(suit, num, GameManager.Club));
            }
        } else {
            Debug.Log(btnName);
        }
        GameManager.OneCard.setAllSuitBtnsInteractable(false);
    }
}
