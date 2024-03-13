using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConnectWindowScript : MonoBehaviour
{
    public TMP_InputField ipField, portField;
    public Button connectBtn, disconnectBtn;
    private static string ipString, portString;

    // Start is called before the first frame update
    void Start()
    {
        ipField = transform.Find("IPInputField").GetComponent<TMP_InputField>();
        portField = transform.Find("PortInputField").GetComponent<TMP_InputField>();
        connectBtn = transform.Find("ConnectBtn").GetComponent<Button>();
        disconnectBtn = transform.Find("DisconnectBtn").GetComponent<Button>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void openConnectWindow() {
        gameObject.SetActive(true);
        if(NetworkManager.nm.connected) {
            ipField.interactable = false;
            portField.interactable = false;
            connectBtn.interactable = false;
            disconnectBtn.interactable = true;

            ipField.text = ipString;
            portField.text = portString;
        } else {
            ipField.interactable = true;
            portField.interactable = true;
            connectBtn.interactable = true;
            disconnectBtn.interactable = false;

            ipField.text = "";
            portField.text = "";
        }
    }
    public void closeConnectWindow() {
        gameObject.SetActive(false);
    }
    public void connect() {
        NetworkManager.nm.connectToServer(ipField.text, int.Parse(portField.text));
        if (NetworkManager.nm.connected) {
            ipField.interactable = false;
            portField.interactable = false;
            connectBtn.interactable = false;
            disconnectBtn.interactable = true;

            ipString = ipField.text;
            portString = portField.text;
        }
    }
    public void disconnect() {
        NetworkManager.nm.disconnect();
        ipField.interactable = true;
        portField.interactable = true;
        connectBtn.interactable = true;
        disconnectBtn.interactable = false;
    }
}
