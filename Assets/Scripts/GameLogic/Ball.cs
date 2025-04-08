using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Sphere))]
public class Ball : MonoBehaviour
{
    public float m_bonusFactor;
    public Rigidbody m_rigidBody;

    private void Awake()
    {
        GameManager.instance.AddBall(this);
        m_rigidBody = GetComponent<Rigidbody>();
    }

    public void AddForce(Vector3 force, Vector3 position)
    {
        m_rigidBody.AddForceAtPosition(force, position, ForceMode.Impulse);
    }

    public void SetVelocity(Vector3 linearVelocity, Vector3 angularVelocity)
    {
        m_rigidBody.velocity = linearVelocity;
        m_rigidBody.angularVelocity = angularVelocity;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Pocket")
        {
            GameManager.instance.RemoveBall(this);
        }
    }

    public void OnCollisionEnter(UnityEngine.Collision collision)
    {
        if (collision.gameObject.tag == "Edge")
        {
            Vector3 impulse = collision.impulse;
            ContactPoint[] contacts = new ContactPoint[100];
            int count = collision.GetContacts(contacts);
            Vector3 normal = Vector3.zero;
            for (int i = 0; i < count; i++)
            {
                normal += contacts[i].normal;
            }
            normal = (normal / count).normalized;
            normal = Vector3.Dot(normal, impulse) * normal;
            m_rigidBody.AddForce(normal, ForceMode.Impulse);
        }
    }
}
