using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public partial class NetworkController : MonoBehaviour
{
    private void OnPlayerJoined()
    {
        if(NetworkManager.Singleton.ConnectedClients.Count >= MAX_PLAYER)
        {
            ChangeSceneForAllPlayers();
        }
    }

    private void ChangeSceneForAllPlayers()
    {
        if(NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(gameScene, LoadSceneMode.Single);
        }
    }

}
