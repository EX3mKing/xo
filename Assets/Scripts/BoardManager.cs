using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class BoardManager : NetworkBehaviour
{
    private Button[,] _buttons = new Button[3, 3];
    
    public override void OnNetworkSpawn()
    {
        var cells = GetComponentsInChildren<Button>();
        
        base.OnNetworkSpawn();
        int n = 0;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                _buttons[i, j] = cells[n];
                n++;

                int r = i;
                int c = j;
                
                _buttons[i,j].onClick.AddListener(delegate
                {
                    OnClickCell(r, c);
                });
            }
        }
    }

    private void OnClickCell(int r, int c)
    {
        if (NetworkManager.Singleton.IsHost && GameManager.Instance.isHostsTurn.Value == 0)
        {
            _buttons[r, c].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "X";
            ChangeSpriteClientRpc(r, c);
            GameManager.Instance.isHostsTurn.Value = 1;
            _buttons[r, c].interactable = false;
        }
        else if (!NetworkManager.Singleton.IsHost && GameManager.Instance.isHostsTurn.Value == 1)
        {
            _buttons[r, c].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "O";
            ChangeSpriteServerRpc(r, c);
            _buttons[r, c].interactable = false;
        }

    }

    [ClientRpc]
    private void ChangeSpriteClientRpc(int r, int c)
    {
        _buttons[r, c].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "X";
        _buttons[r, c].interactable = false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeSpriteServerRpc(int r, int c)
    {
        _buttons[r, c].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "O";
        _buttons[r, c].interactable = false;
        GameManager.Instance.isHostsTurn.Value = 0;
    }
}
