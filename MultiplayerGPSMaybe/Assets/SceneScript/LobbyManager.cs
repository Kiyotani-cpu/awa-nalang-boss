using System;
using System.Collections.Generic;
using Niantic.Lightship.SharedAR.Colocalization;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private SharedSpaceManager _sharedSpaceManager;
    public static int MAX_AMOUNT_CLIENTS_ROOM = 4; // Example value

    [SerializeField] private Texture2D _targetImage;
    [SerializeField] private float _targetImageSize;

    [SerializeField] private Button StartGameButton;
    [SerializeField] private Button CreateRoomButton;
    [SerializeField] private Button JoinRoomButton;
    [SerializeField] private InputField RoomNameInput;
    [SerializeField] private Transform RoomListContainer; // Parent object for the room buttons
    [SerializeField] private GameObject RoomButtonPrefab; // Prefab for a room button

    private string roomName;
    private bool isHost;
    private Dictionary<string, Button> roomButtons = new Dictionary<string, Button>();

    public static event Action OnStartSharedSpaceHost;
    public static event Action OnJoinSharedSpaceClient;
    public static event Action OnStartGame;
    public static event Action OnStartSharedSpace;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        _sharedSpaceManager.sharedSpaceManagerStateChanged += SharedSpaceManagerOnsharedSpaceManagerStateChanged;

        StartGameButton.onClick.AddListener(StartGame);
        CreateRoomButton.onClick.AddListener(CreateGameHost);
        JoinRoomButton.onClick.AddListener(JoinGameClient);

        StartGameButton.interactable = false;

        BlitImageForColocalization.OnTextureRendered += BlitImageForColocalizationOnTextureRendered;
    }

    private void OnDestroy()
    {
        _sharedSpaceManager.sharedSpaceManagerStateChanged -= SharedSpaceManagerOnsharedSpaceManagerStateChanged;
        BlitImageForColocalization.OnTextureRendered -= BlitImageForColocalizationOnTextureRendered;
    }

    private void BlitImageForColocalizationOnTextureRendered(Texture2D texture)
    {
        SetTargetImage(texture);
        StartSharedSpace();
    }

    private void SetTargetImage(Texture2D texture2D)
    {
        _targetImage = texture2D;
    }

    private void SharedSpaceManagerOnsharedSpaceManagerStateChanged(SharedSpaceManager.SharedSpaceManagerStateChangeEventArgs obj)
    {
        if (obj.Tracking)
        {
            StartGameButton.interactable = true;
            CreateRoomButton.interactable = false;
            JoinRoomButton.interactable = false;
        }
    }

    private void StartGame()
    {
        OnStartGame?.Invoke();

        if (isHost)
        {
            NetworkManager.Singleton.StartHost();
        }
        else
        {
            NetworkManager.Singleton.StartClient();
        }
    }

    private void StartSharedSpace()
    {
        OnStartSharedSpace?.Invoke();

        if (_sharedSpaceManager.GetColocalizationType() == SharedSpaceManager.ColocalizationType.MockColocalization)
        {
            var mockTrackingArgs = ISharedSpaceTrackingOptions.CreateMockTrackingOptions();
            var roomArgs = ISharedSpaceRoomOptions.CreateLightshipRoomOptions(
                roomName,
                MAX_AMOUNT_CLIENTS_ROOM,
                "MockColocalizationDemo"
            );

            _sharedSpaceManager.StartSharedSpace(mockTrackingArgs, roomArgs);
            return;
        }

        if (_sharedSpaceManager.GetColocalizationType() == SharedSpaceManager.ColocalizationType.ImageTrackingColocalization)
        {
            var imageTrackingOptions = ISharedSpaceTrackingOptions.CreateImageTrackingOptions(
                _targetImage, _targetImageSize
            );

            var roomArgs = ISharedSpaceRoomOptions.CreateLightshipRoomOptions(
                roomName,
                MAX_AMOUNT_CLIENTS_ROOM,
                "ImageColocalization"
            );

            _sharedSpaceManager.StartSharedSpace(imageTrackingOptions, roomArgs);
            return;
        }
    }

    private void CreateGameHost()
    {
        roomName = RoomNameInput.text;

        if (string.IsNullOrWhiteSpace(roomName))
        {
            Debug.LogWarning("Room name cannot be empty!");
            return;
        }

        isHost = true;
        OnStartSharedSpaceHost?.Invoke();

        AddRoomToList(roomName);
    }

    private void JoinGameClient()
    {
        isHost = false;
        OnJoinSharedSpaceClient?.Invoke();
    }

    private void AddRoomToList(string roomName)
    {
        if (roomButtons.ContainsKey(roomName))
        {
            Debug.LogWarning("Room already exists!");
            return;
        }

        var roomButton = Instantiate(RoomButtonPrefab, RoomListContainer);
        roomButton.GetComponentInChildren<Text>().text = roomName;

        var button = roomButton.GetComponent<Button>();
        button.onClick.AddListener(() => JoinRoom(roomName));
        roomButtons.Add(roomName, button);
    }

    private void JoinRoom(string roomName)
    {
        this.roomName = roomName;
        Debug.Log($"Joining room: {roomName}");
        JoinGameClient();
        StartSharedSpace();
    }
}
