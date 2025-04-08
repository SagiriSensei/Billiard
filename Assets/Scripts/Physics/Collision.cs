using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collision
{
    List<Contact> m_contacts;
    public Shape m_shapeA;
    public Shape m_shapeB;

    public Collision()
    {
        m_contacts = new List<Contact>();
    }

    public Contact GetContact(int index)
    {
        if (index >= m_contacts.Count) return m_contacts[m_contacts.Count - 1];
        else return m_contacts[index];
    }

    public List<Contact> GetContacts()
    {
        return m_contacts;
    }

    public void AddContact(Contact contact)
    {
        m_contacts.Add(contact);
    }
}

public class Contact
{
    //…¯Õ∏æ‡¿Î
    public float penetration;
    public Vector3 normal;

    public Vector3 localPositionA;
    public Vector3 localPositionB;
    public Vector3 globalPositionA;
    public Vector3 globalPositionB;

    public float normalImpulseSum;
    public float tangentImpulseSum;
    public float biNormalImpulseSum;

    public ConstraintInfo normalInfo;

    public void UpdateData(Contact contact)
    {
        penetration = contact.penetration;
        normal = contact.normal;

        localPositionA = contact.localPositionA;
        localPositionB = contact.localPositionB;
        globalPositionA = contact.globalPositionA;
        globalPositionB = contact.globalPositionB;
    }
}
