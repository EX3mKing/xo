using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class BoardManager : NetworkBehaviour
{
    private Button[,] _buttons = new Button[3, 3];
    private int[,] _states = new int[3, 3];
    
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
            _buttons[r, c].interactable = false;
            _states[r, c] = 1;
            Debug.Log(CheckWinState(1));
            ChangeSpriteClientRpc(r, c);
            GameManager.Instance.isHostsTurn.Value = 1;

            if (CheckWinState(1)) GameManager.Instance.gameState.Value = 1;
            if (CheckDrawState()) GameManager.Instance.gameState.Value = 3;
            
        }
        else if (!NetworkManager.Singleton.IsHost && GameManager.Instance.isHostsTurn.Value == 1)
        {
            _buttons[r, c].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "O";
            _buttons[r, c].interactable = false;
            _states[r, c] = 2;
            ChangeSpriteServerRpc(r, c);
        }
    }

    [ClientRpc]
    private void ChangeSpriteClientRpc(int r, int c)
    {
        _buttons[r, c].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "X";
        _buttons[r, c].interactable = false;
        _states[r, c] = 1;
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeSpriteServerRpc(int r, int c)
    {
        _buttons[r, c].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "O";
        _buttons[r, c].interactable = false;
        _states[r, c] = 2;
        
        GameManager.Instance.isHostsTurn.Value = 0;
        
        if (CheckWinState(2)) GameManager.Instance.gameState.Value = 2;
        if (CheckDrawState()) GameManager.Instance.gameState.Value = 3;
        
    }
    
    private bool CheckWinState(int sender)
    {
        for (int i = 0; i < 3; i++)
        {
            if (_states[i, 0] == _states[i, 1] && _states[i, 1] == _states[i, 2] && _states[i, 2] == sender) return true;
            if (_states[0, i] == _states[1, i] && _states[1,i] == _states[2, i] && _states[2, i] == sender) return true;
        }

        if (_states[1, 1] == sender)
        {
            if (_states[0, 0] == _states[1, 1] && _states[1, 1] == _states[2, 2]) return true;
            if (_states[2, 0] == _states[1, 1] && _states[1, 1] == _states[0, 2]) return true;
        }
        
        return false;
    }

    private bool CheckDrawState()
    {
        foreach (var state in _states)
        {
            if (state == 0) return false;
        }

        return true;
    }
}
