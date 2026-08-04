using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingCell : MonoBehaviour
{
    public float newY = 0f;
    public bool randomSpeed = false;
    public float speed = 1f;
    Vector3 firstpos, secondpos;
    float a = 0f;
    bool b = false;
    void Start()
    {
        if (randomSpeed) speed = Random.Range(0.2f, 1f);
        firstpos = transform.position;
        secondpos = transform.position;
        secondpos.y += newY;
    }

    void FixedUpdate()
    {
        if(!b)
        {
            a += (Time.fixedDeltaTime*speed);
            if( a>= 1f)
            {
                b = true;
            }
        }
        else
        {
            a -= (Time.fixedDeltaTime * speed);
            if (a <= 0f)
            {
                b = false;
            }
        }

        transform.position = Vector3.Lerp(firstpos, secondpos, a);
    }
}
