using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField]
    private GameState gameState;

    void Update()
    {
        gameState.Update();
    }
}
