using System.Collections.Generic;
using UnityEngine;

public class ElevatoManager : MonoBehaviour
{
    public static ElevatoManager Instance { get; private set; }
    
    public float[] floorPositions={-2.78f,-0.89f ,0.94f,2.91f};
    public List<ElevatorController> elevators = new List<ElevatorController>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start() => RegisterElevators();

    public void RegisterElevators()
    {
        elevators.Clear();
        elevators.AddRange(FindObjectsByType<ElevatorController>(FindObjectsSortMode.None));
        Debug.Log($"Registered {elevators.Count} elevators");
    }

    public float GetFloorPosition(int floor)
    {
        if (floor >= 0 && floor < floorPositions.Length) return floorPositions[floor];
        Debug.LogWarning($"Invalid floor number: {floor}. Returning ground floor position.");
        return floorPositions[0];
    }

    /// Calculates the distance score for an elevator to service a floor request.
    /// Lower scores indicate better suitability for the request.
    public int CalculateDistanceScore(ElevatorController elevator, int requestedFloor)
    {
        int floorDifference = Mathf.Abs(elevator.currentFloor - requestedFloor); // Calculate absolute floor difference
        ElevatorState elevatorState = elevator.CurrentState;

        if (elevatorState == ElevatorState.Idle) return floorDifference; // If Idle: return absolute difference
        
        // If moving towards request: return favorable score
        if ((elevatorState == ElevatorState.Moving_Up && requestedFloor > elevator.currentFloor) ||
            (elevatorState == ElevatorState.Moving_Down && requestedFloor < elevator.currentFloor))
            return floorDifference;

        return floorDifference + 10; // If moving away from request: apply penalty
    }

    /// Assigns a floor request to the most suitable elevator.
    public void AssignRequest(int requestedFloor)
    {
        Debug.Log($"=== BUTTON PRESSED: Floor {requestedFloor} requested ===");
        
        if (requestedFloor < 0 || requestedFloor > 3) // Validate floor is within bounds
        {
            Debug.LogWarning($"Invalid floor request: {requestedFloor}. Floor must be between 0 and 3.");
            return;
        }

        // Check if any elevator already has this floor in requestList (avoid duplicates)
        foreach (ElevatorController elevator in elevators)
        {
            if (elevator.requestList.Contains(requestedFloor))
            {
                Debug.Log($"Floor {requestedFloor} is already in service by an elevator.");
                return; 
            }
        }

        if (elevators.Count == 0) // If no elevators are available, log warning and return
        {
            Debug.LogWarning("No elevators available to service request.");
            return;
        }

        Debug.Log($"Total elevators available: {elevators.Count}");

        // Calculate distance score for each elevator and find best
        ElevatorController bestElevator = null;
        int minScore = int.MaxValue;
        int elevatorIndex = 0;

        foreach (ElevatorController elevator in elevators)
        {
            int score = CalculateDistanceScore(elevator, requestedFloor);
            Debug.Log($"  Elevator {elevatorIndex}: at floor {elevator.currentFloor}, state={elevator.CurrentState}, score={score}");

            if (score < minScore)
            {
                minScore = score;
                bestElevator = elevator;
            }
            elevatorIndex++;
        }

        // Select elevator with minimum distance score and assign request
        if (bestElevator != null)
        {
            bestElevator.AddRequest(requestedFloor);
            Debug.Log($"✓ ASSIGNED: Floor {requestedFloor} → Elevator at floor {bestElevator.currentFloor} (score: {minScore})");
        }
    }

}
