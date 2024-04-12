using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    private const float MAX_MOVEMENT = 4.5f;
    private const float MIN_MOVEMENT = -4.5f;

    public float playerSpeed;
    public float jumpSpeed;

    // Network variables
    public NetworkVariable<Vector3> PlayerPosition = new NetworkVariable<Vector3>();
    public NetworkVariable<Color> PlayerColor = new NetworkVariable<Color>();

    // Class components
    private MeshRenderer meshRenderer;
    private Rigidbody rb;
    private GameManager gameManager;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            RequestSpawnPlayerRpc();
        }
    }

    void Awake()
    {
        //QualitySettings.vSyncCount = 1;
	    //Application.targetFrameRate = 60;

        gameManager = GameManager.instance;

    }

    void Start() {
        meshRenderer = GetComponent<MeshRenderer>();
        rb = GetComponent<Rigidbody>();

        // Set player position
        UpdatePlayerPosition(PlayerPosition.Value, PlayerPosition.Value);
        PlayerPosition.OnValueChanged += UpdatePlayerPosition;
        UpdatePlayerColor(PlayerColor.Value, PlayerColor.Value);
        PlayerColor.OnValueChanged += UpdatePlayerColor;
    }

    void FixedUpdate()
    {
        if (!IsOwner) {
            return;
        }

        RequestMovePlayer();
    }

    void Update()
    {
        if (!IsOwner) {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            RequestJumpPlayer();
        }
    }

    private void UpdatePlayerPosition(Vector3 previousValue, Vector3 newValue)
    {
        transform.position = newValue;
    }

    private void UpdatePlayerColor(Color previousValue, Color newValue)
    {
        meshRenderer.material.SetColor("_Color", newValue);
    }

    [Rpc(SendTo.Server)]
    void RequestSpawnPlayerRpc(RpcParams rpcParams = default)
    {
        PlayerPosition.Value = new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
        ChangeColorRpc();
    }

    [Rpc(SendTo.Server)]
    void ChangeColorRpc(RpcParams rpcParams = default)
    {
        try {
            Color newColor = GetRandomNotUsedColor();
            gameManager.RemoveColorFromUsed(PlayerColor.Value);
            PlayerColor.Value = newColor;
        } catch (NoAvailableColorException ex) {
            Debug.Log("No new color is available. Skipping ChangeColor request");
        }
    }

    Color GetRandomNotUsedColor()
    {
        List<Color> availableColors = gameManager.GetAvailableColors();

        // If no available colors, throw exception
        if (availableColors.Count == 0) {
            Debug.Log("No available colors");
            throw new NoAvailableColorException();
        }

        // Pick new random color from available colors
        Color selectedColor = availableColors[Random.Range(0, availableColors.Count)];

        // Track new and old colors
        gameManager.AddColorToUsed(selectedColor);
        
        return selectedColor;
    }

    void RequestMovePlayer()
    {
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        //transform.position += movement * playerSpeed * Time.deltaTime;

        // Avoid unneccessary network packets if player is not moving
        if (Vector3.zero == movement) {
            return;
        }
        
        RequestMovePlayerRpc(movement);
    }

    [Rpc(SendTo.Server)]
    void RequestMovePlayerRpc(Vector3 movement)
    {
        if (Vector3.zero == movement) {
            return;
        }

        transform.position += movement * playerSpeed * Time.fixedDeltaTime;

        // Run server-side checks for movement
        RunMovementChecks();
    }

    void RequestJumpPlayer()
    {
        // Only jump if player is grounded
        if (!IsGrounded()) {
            return;
        }

        RequestJumpPlayerRpc();
    }

    [Rpc(SendTo.Server)]
    void RequestJumpPlayerRpc()
    {
        // Only jump if player is grounded
        if (!IsGrounded()) {
            return;
        }

        rb.AddForce(Vector3.up * jumpSpeed, ForceMode.Impulse);
    }

    // TODO Use Raycast
    private bool IsGrounded() {
        return transform.position.y <= 1;
    }

    // Run server-side checks for movement
    private void RunMovementChecks()
    {
        // Prevent move outside world
        ClampMovementToGameRoom();
    }

    private void ClampMovementToGameRoom()
    {
        float clampedX = Mathf.Clamp(transform.position.x, MIN_MOVEMENT, MAX_MOVEMENT);
        float clampedZ = Mathf.Clamp(transform.position.z, MIN_MOVEMENT, MAX_MOVEMENT);
        
        transform.position = new Vector3(clampedX, transform.position.y, clampedZ);
    }
}
