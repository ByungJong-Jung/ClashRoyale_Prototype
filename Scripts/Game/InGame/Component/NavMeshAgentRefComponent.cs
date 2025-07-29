using UnityEngine;
using UnityEngine.AI;
public struct NavMeshAgentRefComponent : IComponent
{
    public NavMeshAgent agent;
    public Vector3 lastRequestedDestination;
}
