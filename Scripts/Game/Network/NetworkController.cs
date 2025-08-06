using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using Unity.Services.Relay;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public partial class NetworkController : MonoBehaviour
{
    [SerializeField] private Text _txtJoinCode;
    [SerializeField] private Button _btnGameStart;
    [SerializeField] private InputField _inputField;
    [SerializeField] private Button _btnJoinMatch;

    private Lobby currentLobby;
    private const int MAX_PLAYER = 2;
    private string gameScene = "SampleScene";

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        if(!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        _btnGameStart.onClick.AddListener(StartMatchMaking);
        _btnJoinMatch.onClick.AddListener(() => JoinGameWithCode(_inputField.text));
    }

   
}
