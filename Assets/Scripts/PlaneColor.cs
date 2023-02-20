using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneColor : MonoBehaviour
{
    private bool ColorSwapped;

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
