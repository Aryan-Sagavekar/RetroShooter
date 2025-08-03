using UnityEngine;
using UnityEngine.InputSystem.XR;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    MapGenerator mapGenerator;
    public GameObject currentPlayer;
    public int parts = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        } else
        {
            Instance = this;
        }

        DontDestroyOnLoad(gameObject);
    }

}
