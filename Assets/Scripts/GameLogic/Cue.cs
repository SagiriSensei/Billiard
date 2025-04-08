using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cue : MonoBehaviour
{
    private void Awake()
    {
    }

    public void Move(Vector3 offset)
    {
        transform.position += offset;
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }
}
