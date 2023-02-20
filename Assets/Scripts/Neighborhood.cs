using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Neighborhood : MonoBehaviour
{
    [Header("Set Dynamically")]
    public List<Boid>       neighbors;
    private SphereCollider  coll;

    void Start()
    {
        neighbors = new List<Boid>();
        coll = GetComponent<SphereCollider>();
        coll.radius = Spawner.S.neighborDist / 2;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (coll.radius != Spawner.S.neighborDist/2)
        {
            coll.radius = Spawner.S.neighborDist/2;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Boid b = other.GetComponent<Boid>();
        if (b != null)
        {
            if (neighbors.IndexOf(b) == -1)
            { 
                neighbors.Add(b);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        Boid b = other.GetComponent<Boid>();
        if (b != null)
        {
            if (neighbors.IndexOf(b) != -1)
            {
                neighbors.Remove(b);
            }
        }
    }


    public Vector3 avgPos
    {
        get
        {
            Vector3 avg = Vector3.zero;
            if (neighbors.Count == 0) return avg;

            for (int i=0; i<neighbors.Count; i++)
            {
                avg += neighbors[i].pos;
            }
            avg /= neighbors.Count;

            return avg;

        }
    }

    public Vector3 avgVel
    {
        get
        {
            Vector3 avg = Vector3.zero;
            if (neighbors.Count == 0) return avg;

            for (int i = 0; i < neighbors.Count; i++)
            {
                avg += neighbors[i].rigid.velocity;
            }
            avg /= neighbors.Count;

            return avg;
        }
    }

    public Vector3 avgClosePos
    {
        get
        {
            Vector3 avg = Vector3.zero;
            Vector3 delta;
            int nearCount = 0;
            for (int i = 0; i < neighbors.Count; i++)
            {
                delta = neighbors[i].pos - transform.position;
                if (delta.magnitude <= Spawner.S.collDist)
                {
                    avg += neighbors[i].pos;
                    nearCount++;
                }
            }

            // If there were no neighbors too close, return Vector3.zero
            if (nearCount == 0) return avg;

            // Otherwise, averge their locations
            avg /= nearCount;
            return avg;
        }
    }

    public Vector3 leaderPos
    {
        get
        {
            // Obtain reference to own data
            Boid self = GetComponent<Boid>();
            // if this Boid is leading the pack, do not change behavior
            if (self.positionInFormation == 0)
            {
                return transform.position;
            }
            
            // otherwise, return the position of the Boid that is one ahead
            for (int i = 0; i < neighbors.Count; i++)
            {
                if (neighbors[i].positionInFormation == self.positionInFormation - 1)
                {
                    return neighbors[i].transform.position;
                }
            }

            // if a leader was not found, then the boid will set its own path
            return transform.position;
        }
    }

    public bool leaderClose
    {
        get
        {
            // Obtain reference to own data
            Boid self = GetComponent<Boid>();
            if (self.positionInFormation == 0)
            {
                return false;
            }

            // find leader in list of neighboring boids
            for (int i = 0; i < neighbors.Count; i++)
            {
                if (neighbors[i].positionInFormation == self.positionInFormation - 1)
                {
                    //Debug.Log(gameObject.name + " lost its leader");
                    return true;
                }
            }

            return false;
        }
    }
}
