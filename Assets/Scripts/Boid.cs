using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    [Header("Set Dynamically")]
    public Rigidbody rigid;
    private Neighborhood neighborhood;
    public SphereCollider myCollider;
    public Vector3 vecToAtt;
    public Vector3 vecToAtt2;
    public Vector3 attPos;
    static Material lineMaterial;
    public float spinRate = 0.50f;
    public Quaternion myq;
    //public Vector3 boidRotVec;
    public float dangle;
    public Vector3 rotVec2;
    public float myangle;
    float rotationRate = 0.50f;

    public bool blocked;
    public int positionInFormation;

    // Use this for initialization
    void Awake()
    {
        neighborhood = GetComponent<Neighborhood>();
        rigid = GetComponent<Rigidbody>();
        myCollider = GetComponent<SphereCollider>();
        //boidRotVec = Vector3.up;
        dangle = 0.0f;
        rotVec2 = Vector3.zero;
        myangle = 0.0f;


        //Set a random initial position
        pos = Random.insideUnitSphere * Spawner.S.spawnRadius;

        //Set a random initial velocity
        Vector3 vel = Random.onUnitSphere * Spawner.S.velocity;

        rigid.velocity = vel;
        
        //LookAhead();

        //Construct the unit vector 15 deg shifted from the 'ray' pointing from 
        //boid to attractor
        //first we need the vector from boid to attractor
        attPos = Attractor.POS; //this is the attractor position
        //Boid positon is pos
        //vector from boid to attractor
        vecToAtt = attPos - pos; //vector between boid and attractor
        rotVec2 = vecToAtt.normalized * myCollider.radius; //resize to collider radius
        Vector3 myPerp = Vector3.Cross(Vector3.up, vecToAtt);//find a perpendicular to vecToAtt
        myq = Quaternion.AngleAxis(15, myPerp); //construct quaternion rotation for 15 deg around myPerp
        rotVec2 = myq * rotVec2; //rotate rotVec2. this is the vector we will spin about 'ray'

        //Give the Boid a random color, but make sure it's not too dark
        Color randColor = Color.black;
        while (randColor.r + randColor.g + randColor.b < 1.0f)
        {
            randColor = new Color(Random.value, Random.value, Random.value);
        }
        Renderer[] rends = gameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in rends)
        {
            r.material.color = randColor;
        }
        TrailRenderer tRend = GetComponent<TrailRenderer>();
        tRend.material.SetColor("_TintColor", randColor);
    }


    void LookAhead()
    {
        //Orients the Boid to look at the direction it's flying
        
        transform.LookAt(pos + rigid.velocity);
        Vector3 tvec;
        tvec = brot * Vector3.up;
        Quaternion qtemp = Quaternion.LookRotation(pos + rigid.velocity, tvec);
        //transform.rotation = qtemp;
        //transform.LookAt(pos + rigid.velocity,tvec);
        //transform.rotation = brot;
        //Debug.Log("myq " + myq.w + " " + myq.x + " " + myq.y + " " + myq.z);
    }

    public Vector3 pos
    {
        get { return transform.position; }
        set { transform.position = value; }
    }

    public Quaternion brot
    {
        get { return transform.rotation; }
        set { transform.rotation = value; }
    }

    //FixedUpdate is called one per physics update (i.e. 50x/second)
    private void FixedUpdate()
    {
        Vector3 vel = rigid.velocity;
        Spawner spn = Spawner.S;

        //Collision Avoidance - avoid neigbors who are too close
        Vector3 velAvoid = Vector3.zero;
        Vector3 tooClosePos = neighborhood.avgClosePos;

        // If the response is Vecator3.zero, then no need to react
        if (tooClosePos != Vector3.zero)
        {
            velAvoid = pos - tooClosePos;
            velAvoid.Normalize();
            velAvoid *= spn.velocity;
        }

        //Velocity matching - Try to match velocity with neigbors
        Vector3 velAlign = neighborhood.avgVel;
        // Only do more if the velAlign is not Vector3.zero
        if (velAlign != Vector3.zero)
        {
            // we're really interested in direction, so normalize the velocity
            velAlign.Normalize();
            // and then set it to the speeed we chose
            velAlign *= spn.velocity;
        }

        //Flock centering - move towards the center of local neighbors
        Vector3 velCenter = neighborhood.avgPos;
        if (velCenter != Vector3.zero)
        {
            velCenter -= transform.position;
            velCenter.Normalize();
            velCenter *= spn.velocity;
        }

        //ATTRACTION - Move towards the Atttractor
        Vector3 delta = Attractor.POS - pos;
        //Check whether we're attracted or avoiding the Attractor
        bool attracted = (delta.magnitude > spn.attractPushDist);
        Vector3 velAttract = delta.normalized * spn.velocity;

        // Part 2: Check if path is blocked by obstacle
        Vector3 perpVel = Vector3.zero;
        if (blocked) 
        {
            // Draw a vector to the attractor
            Vector3 target = Attractor.POS - pos;
            // Use a Raycast to check if path is clear
            RaycastHit hit;
            if (Physics.Raycast(transform.position, target, out hit, Mathf.Infinity))
            {
                // Change velocity to be perpendicular to the vector drawn from the boid to
                // the attractor to prevent the boid from running into the wall
                //Debug.LogWarning("BLOCKAGE DETECTED");
                perpVel = -Vector3.Cross(target, Vector3.right);
                perpVel.Normalize();
                perpVel *= spn.velocity;
            }
        }

        // Part 1: Put Boids in Formation
        // If not leading the line, follow other boids position
        Vector3 followVel = Vector3.zero;
        if (neighborhood.leaderPos != transform.position && neighborhood.leaderClose)
        {
            Vector3 target = neighborhood.leaderPos - pos;
            followVel = target;
            followVel.Normalize();
            followVel *= spn.velocity;
        }

        //Apply all the velocities
        float fdt = Time.fixedDeltaTime;
        // Part 2: Code for avoiding obstacles comes first, then the boids will avoid one another
        if (perpVel != Vector3.zero) 
        {
            vel = Vector3.Lerp(vel, perpVel, spn.collAvoid);
        }
        // Avoidance is now being handled with colliders to prevent the boids from bouncing
        // around too much around walls. Additionally, boids are set to move slower in line
        // if they are in the correct spot
        //else if (velAvoid != Vector3.zero)
        //{
        //    vel = Vector3.Lerp(vel, velAvoid, spn.collAvoid);
        //}
        // Part 1: Code for following leader boid in a line
        else if (followVel != Vector3.zero)
        {
            vel = Vector3.Lerp(vel, followVel, spn.velMatching * fdt);
        }
        else
        {
            if (velAlign != Vector3.zero)
            {
                vel = Vector3.Lerp(vel, velAlign, spn.velMatching * fdt);
                //Debug.Log(gameObject.name + " is Aligning");
            }
            if (velCenter != Vector3.zero)
            {
                vel = Vector3.Lerp(vel, velAlign, spn.flockCentering * fdt);
                //Debug.Log(gameObject.name + " is Centering");
            }
            if (velAttract != Vector3.zero)
            {
                if (attracted)
                {
                    vel = Vector3.Lerp(vel, velAttract, spn.attractPull * fdt);
                    //Debug.Log(gameObject.name + " is Attracted");
                }
                else
                {
                    vel = Vector3.Lerp(vel, -velAttract, spn.attractPush * fdt);
                    //Debug.Log(gameObject.name + " is Pushing");
                }
            }
        }

        //set vel to the velocity set on the spawner singleton
        vel = vel.normalized * spn.velocity;
        // Finally assign this to the Rigidbody
        rigid.velocity = vel;
        //Lock in the direction of the new velocity
        LookAhead();
    }

    // Part 2: Check if path is blocked by obstacle
    private void OnTriggerEnter(Collider other)
    {
        // Enabling the obstacle detection raycasting if a wall is close
        if (other.gameObject.tag == "Obstacle")
        {
            blocked = true;
            //Debug.Log("Collision soon!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Disabling the obstacle detection raycasting if a wall is no longer close
        if (other.gameObject.tag == "Obstacle")
        {
            blocked = false;
            //Debug.Log("Collision avoided.");
        }
    }
}