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

        //Apply all the velocities
        float fdt = Time.fixedDeltaTime;
        if (velAvoid != Vector3.zero)
        {
            vel = Vector3.Lerp(vel, velAvoid, spn.collAvoid);
        }
        else
        {
            if (velAlign != Vector3.zero)
            {
                vel = Vector3.Lerp(vel, velAlign, spn.velMatching * fdt);
            }
            if (velCenter != Vector3.zero)
            {
                vel = Vector3.Lerp(vel, velAlign, spn.flockCentering * fdt);
            }
            if (velAttract != Vector3.zero)
            {
                if (attracted)
                {
                    vel = Vector3.Lerp(vel, velAttract, spn.attractPull * fdt);
                }
                else
                {
                    vel = Vector3.Lerp(vel, -velAttract, spn.attractPush * fdt);
                }
            }
        }

        //set vel to the velocity set on the spawner singleton
        vel = vel.normalized * spn.velocity;
        // Finally assign this to the Rigidbody
        rigid.velocity = vel;
        //Lock in the direction of the new velocity
        //LookAhead();


        //Update the line pointing to the attractor with spherecollider radius
        //Get attractor postion and draw it
        attPos = Attractor.POS;
        vecToAtt = attPos - pos;
        //normalize vectoatt and multiply by collider radius to change vector length
        vecToAtt = vecToAtt.normalized * myCollider.radius;
        //Now the line from pos to pos + vecToAtt that is drawn with GL.Lines below
        // is the vector from the boid pointing towards the attractor with the radius of
        // the SphereCollider of the boid

        //Now draw a line projecting vecToAtt on the plane:
        //this is the vector from boid to attractor again
        //this vector with the y-component equal to zero lies in the plane x,z
        //it is plotted below between pos and attPos by setting y-components to zero

        //(By the way this only works if the plane is the x,z plane!
        //To generalize, we would compute the normal to the plane say nplane
        // then the vectors 
        //vecToAtt2Perp = Vector3.Dot(nplane,vecToAtt2)*nplane
        //posPerp = Vector3.Dot(nplane,pos)*nplan
        //are the ammounts of the pos and vecToAtt2 that are perpendicular to the plane
        // Now subtract 
        // vecToAtt2InPlane = vecToAtt2 - vecToAtt2Perp;
        // posInPlane = pos - posPerp;
        // ploting a line between these two gives the projectioin of vecToAtt2
        // in the genearl plane)

        //Now spin each boid about the vector it is moving in

        //create the quaternion and rotate boid via transform.rotation
        dangle += spinRate * Time.deltaTime;
        brot = Quaternion.AngleAxis(dangle, vecToAtt);

        //Now update, rotate and draw the vector 15 deg from the boids 'ray' to the attractor
       float delta_myangle = rotationRate * Time.deltaTime; //update the angle step
        myangle += delta_myangle; //add the angle step to the angle
        myq = Quaternion.AngleAxis(myangle, vecToAtt); //update the quaternion rotation
        //Debug.Log("myq " + myq.w + " " + myq.x + " " + myq.y + " " + myq.z);
        rotVec2 = myq * rotVec2; //rotate the vector on the 15 deg cone

        LookAhead();

    }


    public void OnRenderObject()
    {

    // Draw lines
    CreateLineMaterial();
    // Apply the line material
    lineMaterial.SetPass(0);
        GL.PushMatrix();
        // Set transformation matrix for drawing to
        // match our transform
        //GL.MultMatrix(transform.localToWorldMatrix);

        
        GL.Begin(GL.LINES);

        //this is the line pointing from boid to attractor
        Color color1 = Color.red;
        GL.Color(color1);        
        GL.Vertex(pos); // this is the boid position
        //GL.Vertex(attPos); // this line attractor position
        GL.Vertex(pos + vecToAtt); //position of end of vector pointing from boid to attractor

        //this is the component of the line pointing from boid to attractor 
        //that lies in the x,z plane
        Color color2 = Color.blue;
        GL.Color(color2);
        GL.Vertex3(pos.x,0,pos.z);//x,z component of boid position
        GL.Vertex3(attPos.x,0,attPos.z);//x,z component of attractor position

        //this is the line rotating on the 15 deg cone that is centered on attPos
        Color color3 = Color.green;
        GL.Color(color3);
        GL.Vertex(pos);
        GL.Vertex(pos + rotVec2);

        GL.End();
        GL.PopMatrix();

    }

static void CreateLineMaterial()
    {
        if (!lineMaterial)
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            lineMaterial.SetInt("_ZWrite", 0);
        }
    }
}