using UnityEngine;

public class Floor : MonoBehaviour
{
    public int floorNumber;

    /// Handles floor button click events and sends request to ElevatorManager.
    public void OnFloorButtonClicked()
    {
        if (ElevatoManager.Instance != null)
            ElevatoManager.Instance.AssignRequest(floorNumber);
        else
            Debug.LogError("ElevatoManager instance not found. Cannot assign floor request.");
    }
}
