using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticTrail : MonoBehaviour
{
    [SerializeField]
    private LineRenderer 
        lineRend;
    [Range(2,100), SerializeField]
    private int
        posCount = 26;
    [SerializeField]
    private float
        length = 3f;
    private float[]
        poses;
    public enum
        TYPES {UP, DOWN, RIGHT, LEFT};
    [SerializeField]
    private TYPES
        trailType = TYPES.LEFT;

    private void Init()
    {
        lineRend.positionCount = posCount;
        poses = new float[posCount];
        for (var i = 0; i < posCount; i++)
        {
            switch (trailType)
            {
                case TYPES.DOWN: case TYPES.UP:
                    poses[i] = transform.position.x;
                    break;
                case TYPES.LEFT: case TYPES.RIGHT:
                    poses[i] = transform.position.y;
                    break;
            }
        }

        //StartCoroutine(UpdateWithDelay());
    }

    private void UpdateTrail()
    {
        for (var i = 0; i < posCount; i++)
        {
            Vector3 position = default(Vector3);
            var pos = poses[i];
            switch (trailType)
            {
                case TYPES.DOWN:
                    position = new Vector3(pos, transform.position.y - (length / posCount * i), 0f);
                    break;
                case TYPES.UP:
                    position = new Vector3(pos, transform.position.y + (length / posCount * i), 0f);
                    break;
                case TYPES.LEFT:
                    position = new Vector3(transform.position.x - (length / posCount * i), pos, 0f);
                    break;
                case TYPES.RIGHT:
                    position = new Vector3(transform.position.x + (length / posCount * i), pos, 0f);
                    break;
            }
            lineRend.SetPosition(i, position);
        }
    }

    private void UpdatePositions()
    {
        var nextposes = new float[posCount];
        for (var i = 1; i < posCount; i++)
            nextposes[i] = poses[i - 1];

        switch (trailType)
        {
            case TYPES.DOWN: case TYPES.UP:
                nextposes[0] = transform.position.x;
                break;
            case TYPES.LEFT: case TYPES.RIGHT:
                nextposes[0] = transform.position.y;
                break;
        }

        poses = nextposes;

    }
    /*
    private IEnumerator UpdateWithDelay()
    {
        for(; ; )
        {
            UpdatePositions();
            UpdateTrail();

            yield return new WaitForSeconds(delay);
        }
    }
    */
    private void Start()
    { Init(); }

    private void FixedUpdate()
    {
        UpdatePositions();
        UpdateTrail();
    }
}
