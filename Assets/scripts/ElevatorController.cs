using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum ElevatorState{Idle,Moving_Up,Moving_Down}

public class ElevatorController : MonoBehaviour
{
    private ElevatorState currentState;
    public int currentFloor;
    private int targetFloor;
    public List<int> requestList;
    
    // Public property to access current state for distance scoring
    public ElevatorState CurrentState => currentState;
    
    [SerializeField]
    private TextMeshProUGUI floorDisplay;
    
    [SerializeField]
    private float moveSpeed = 2.0f;

    void Start()
    {
        currentState = ElevatorState.Idle; // Set initial state to Idle
        currentFloor = 0; // Set initial floor to Ground floor
        requestList = new List<int>(); // Initialize request list as empty

        // Set initial position to ground floor using ElevatoManager
        if (ElevatoManager.Instance != null)
        {
            float groundFloorPosition = ElevatoManager.Instance.GetFloorPosition(0);
            transform.position = new Vector3(transform.position.x, groundFloorPosition, transform.position.z);
        }

        UpdateFloorDisplay(); // Update floor display to show "G" for ground floor
    }

    private void UpdateFloorDisplay()
    {
        if (floorDisplay != null)
            floorDisplay.text = currentFloor == 0 ? "G" : currentFloor.ToString();
    }
    /// Sets the elevator state and handles state transitions.
    private void SetState(ElevatorState newState)
    {
        currentState = newState;
    }

    /// Updates the elevator state based on current conditions.
    private void UpdateState()
    {
        if (requestList.Count == 0) // Transition to Idle when requestList is empty
        {
            SetState(ElevatorState.Idle);
            return;
        }

        if (targetFloor > currentFloor) // Transition to Moving_Up
            SetState(ElevatorState.Moving_Up);
        else if (targetFloor < currentFloor) // Transition to Moving_Down
            SetState(ElevatorState.Moving_Down);
        else
            SetState(ElevatorState.Idle);
    }

    /// Adds a floor request to the elevator's request list.
    public void AddRequest(int floor)
    {
        if (floor < 0 || floor > 3) // Validate floor is between 0 and 3
        {
            Debug.LogWarning($"Invalid floor request: {floor}. Floor must be between 0 and 3.");
            return;
        }

        if (floor == currentFloor) // Ignore request if elevator is already at that floor
        {
            Debug.Log($"Elevator already at floor {floor}. Ignoring request.");
            return;
        }

        if (!requestList.Contains(floor)) // Add floor to requestList if not already present
        {
            requestList.Add(floor);
            SortRequestList(); // Call SortRequestList() to optimize order
            
            if (currentState == ElevatorState.Idle) // If currently Idle, set targetFloor and update state
            {
                targetFloor = requestList[0];
                UpdateState();
            }
        }
    }

    /// Sorts the request list to optimize elevator movement based on current state and floor.
    private void SortRequestList()
    {
        if (requestList.Count <= 1) return;

        if (currentState == ElevatorState.Moving_Up) // Prioritize floors above in ascending order
        {
            requestList.Sort((a, b) =>
            {
                bool aAbove = a > currentFloor;
                bool bAbove = b > currentFloor;
                if (aAbove && bAbove) return a.CompareTo(b); // Both above: ascending
                if (!aAbove && !bAbove) return b.CompareTo(a); // Both below: descending
                return aAbove ? -1 : 1; // Prioritize above
            });
        }
        else if (currentState == ElevatorState.Moving_Down) // Prioritize floors below in descending order
        {
            requestList.Sort((a, b) =>
            {
                bool aBelow = a < currentFloor;
                bool bBelow = b < currentFloor;
                if (aBelow && bBelow) return b.CompareTo(a); // Both below: descending
                if (!aBelow && !bBelow) return a.CompareTo(b); // Both above: ascending
                return aBelow ? -1 : 1; // Prioritize below
            });
        }
        else // Idle: Sort by absolute distance from currentFloor
        {
            requestList.Sort((a, b) =>
                Mathf.Abs(a - currentFloor).CompareTo(Mathf.Abs(b - currentFloor))
            );
        }
    }

    /// Handles smooth elevator movement each frame.
    void Update()
    {
        if (currentState == ElevatorState.Idle || requestList.Count == 0) return;

        // Get target position and move towards it
        float targetPosition = ElevatoManager.Instance.GetFloorPosition(targetFloor);
        Vector3 targetPos = new Vector3(transform.position.x, targetPosition, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        // Check if within threshold distance (0.01f) of target position
        if (Mathf.Abs(transform.position.y - targetPosition) <= 0.01f)
        {
            transform.position = targetPos; // Snap to exact position
            currentFloor = targetFloor; // Update currentFloor
            UpdateFloorDisplay(); // Update floor display
            requestList.Remove(targetFloor); // Remove from requestList

            // Process next request or transition to Idle
            if (requestList.Count > 0)
            {
                targetFloor = requestList[0];
                UpdateState();
            }
            else
            {
                SetState(ElevatorState.Idle); // Handle boundary state transitions
            }
        }
    }

}
