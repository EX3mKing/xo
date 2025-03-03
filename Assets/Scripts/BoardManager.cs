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
            }
        }
    }
    
}
