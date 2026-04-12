using UnityEngine;

[DisallowMultipleComponent]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerView playerView;

    private PlayerModel playerModel;
    private IInputService inputService;
    private bool initialized;

    private void Awake()
    {
        if (playerView == null)
        {
            playerView = GetComponent<PlayerView>();
        }

        playerModel = new PlayerModel();
    }

    private void Update()
    {
        if (!initialized || inputService == null || playerView == null)
        {
            return;
        }

        playerModel.UpdateFromInput(inputService);
        playerView.Apply(playerModel);
    }

    public void Initialize(IInputService inputService)
    {
        this.inputService = inputService;
        initialized = inputService != null && playerView != null;
    }
}
