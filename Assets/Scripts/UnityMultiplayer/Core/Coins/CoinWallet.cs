using Unity.Netcode;
using UnityEngine;

public class CoinWallet : NetworkBehaviour
{
    public NetworkVariable<int> TotalCoins = new NetworkVariable<int>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(!other.TryGetComponent<Coin>(out Coin coin)){return;}
        int coinValue = coin.Collect();
        if (!IsServer) return;
        TotalCoins.Value += coinValue;
    }
    public bool CanSpendCoins(int coinsToSpend)
    {
        return coinsToSpend <= TotalCoins.Value;
    }
    public void SpendCoins(int coinsToSpend)
    {

        if(CanSpendCoins(coinsToSpend))
        {
            if(IsServer)
            {
                TotalCoins.Value -= coinsToSpend;
            }
        }
    }
}
