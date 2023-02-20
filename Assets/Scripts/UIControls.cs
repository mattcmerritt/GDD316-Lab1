using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIControls : MonoBehaviour
{
    [SerializeField] private GameObject Obstacle;
    [SerializeField] private GameObject BoidPrefab, PlaneBoidPrefab, PropellorBoidPrefab;

    public void SpawnBoid()
    {
        Spawner.S.boidPrefab = BoidPrefab;
        Spawner.S.InstantiateBoid();
    }

    public void SpawnPlaneBoid()
    {
        Spawner.S.boidPrefab = PlaneBoidPrefab;
        Spawner.S.InstantiateBoid();
    }
    public void SpawnPropellorBoid()
    {
        Spawner.S.boidPrefab = PropellorBoidPrefab;
        Spawner.S.InstantiateBoid();
    }

    public void ChangeObstacle(bool state)
    {
        Obstacle.SetActive(state);
    }
}
