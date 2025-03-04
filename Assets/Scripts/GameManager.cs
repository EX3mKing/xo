using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;
    [SerializeField] private GameObject p_board;

    // 0    -   is hosts turn
    // 1    -   clients turn
    // else -   neutral / locked
    public NetworkVariable<int> isHostsTurn = new NetworkVariable<int>(0);
    
    // 0 - neutral
    // 1 - host win
    // 2 - client win
    // 3 - draw
    public NetworkVariable<int> gameState = new NetworkVariable<int>(0); 

    [SerializeField] private GameObject o_resultPanel;
    [SerializeField] private GameObject o_joinUI;
    [SerializeField] private TextMeshProUGUI tmp_resultText;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
        {
            Debug.Log($"person {clientId}, joined the fight");
            if (NetworkManager.Singleton.IsHost && NetworkManager.Singleton.ConnectedClients.Count == 2)
            {
                Debug.Log("Spawn shi");
                Instantiate(p_board).GetComponent<NetworkObject>().Spawn();
                o_joinUI.SetActive(false);
                HideRelayUIClientRPC();
            }
        };

        gameState.OnValueChanged += (oldValue, newValue) =>
        {
            if (newValue != 0) o_resultPanel.SetActive(true);
            switch (newValue)
            {
                case 0:
                    break;
                case 1:
                    tmp_resultText.text = "Host won";
                    break;
                case 2:
                    tmp_resultText.text = "Client won";
                    break;
                case 3:
                    tmp_resultText.text = "Draw";
                    break;
                default:
                    Debug.LogError("Something went wrong");
                    break;
            }
            
            Debug.Log("shi changed");
        };
    }

    [ClientRpc]
    private void HideRelayUIClientRPC()
    {
        o_joinUI.SetActive(false);
    }

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    public void RestartGame()
    {
        if (IsHost)
        {
            FindAnyObjectByType<BoardManager>().GetComponent<NetworkObject>().Despawn();
            gameState.Value = 0;
            isHostsTurn.Value = 0;
            RestartGameClientRpc();
            o_resultPanel.SetActive(false);
            Instantiate(p_board).GetComponent<NetworkObject>().Spawn();
        }
        else
        {
            RestartGameServerRpc();
        }
        
    }

    [ServerRpc(RequireOwnership = false)]
    private void RestartGameServerRpc()
    {
        FindAnyObjectByType<BoardManager>().GetComponent<NetworkObject>().Despawn();
        o_resultPanel.SetActive(false);
        Instantiate(p_board).GetComponent<NetworkObject>().Spawn();
        gameState.Value = 0;
        isHostsTurn.Value = 0;
    }

    [ClientRpc]
    private void RestartGameClientRpc()
    {
        o_resultPanel.SetActive(false);
    }
}
