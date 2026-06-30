using UnityEngine;

public class PlayerSpawnController : MonoBehaviour
{
    public Transform operationRoomSpawnPoint;

    void Start()
    {
        if (PlayerPrefs.GetInt("SpawnInOperationRoom", 0) == 1)
        {
            transform.position = operationRoomSpawnPoint.position;
            transform.rotation = operationRoomSpawnPoint.rotation;

            // Clear the flag so normal starts work again
            PlayerPrefs.DeleteKey("SpawnInOperationRoom");
        }
    }
}