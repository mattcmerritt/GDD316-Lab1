using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Part 3: Differentiating different boids
// The plane boids will have a gray metallic color instead of the rainbow ones.
// They also have a tail and wings to make them look more like a plane.
// The other type of boid differs in shape (the other is cylindrical with a propellor).
public class PlaneColor : MonoBehaviour
{
    private bool ColorSwapped;

    // On the first update, replace the color and trail color with a gray color.
    private void Update()
    {
        if (!ColorSwapped)
        {
            Boid b = GetComponent<Boid>();
            //Give the Boid a random color, but make sure it's not too dark
            float value = Random.value * 0.75f;
            Color grayColor = new Color(value, value, value);
            
            Renderer[] rends = gameObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in rends)
            {
                r.material.color = grayColor;
            }
            TrailRenderer tRend = GetComponent<TrailRenderer>();
            tRend.material.SetColor("_TintColor", grayColor);

            ColorSwapped = true;
        }
        
    }
}
