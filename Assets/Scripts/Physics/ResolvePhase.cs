using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum JacobianType
{
    Normal,
    Tangent,
}

public struct ConstraintInfo
{
    public Matrix Jn;
    public Matrix Jt;
    public Matrix Jb;
    public Matrix inverseM;
    public Matrix V;
}


public class ResolvePhase
{
    Dictionary<Tuple<int, int>, Collision> m_manifolds;

    public ResolvePhase()
    {
        m_manifolds = new Dictionary<Tuple<int, int>, Collision>();
    }

    public void ResoloveCollision(List<Collision> collisions)
    {
        for (int i = 0; i < collisions.Count; i++)
        {
            //暖启动
            Tuple<int, int> idPairs = new Tuple<int, int>(collisions[i].m_shapeA.GetId(), collisions[i].m_shapeB.GetId());
            Collision collision = collisions[i];
            if (m_manifolds.ContainsKey(idPairs)) collision = WarmStarting(m_manifolds[idPairs], collision);
            else m_manifolds[idPairs] = collision;

            List<Contact> contacts = collision.GetContacts();      
            for (int j = 0; j < contacts.Count; j++)
            {
                InitConstraintInfo(collisions[i].m_shapeA, collisions[i].m_shapeB, contacts[j]);
                ContactConstraintSolver(collisions[i].m_shapeA, collisions[i].m_shapeB, contacts[j], JacobianType.Normal);
                ContactConstraintSolver(collisions[i].m_shapeA, collisions[i].m_shapeB, contacts[j], JacobianType.Tangent);
            }
        }
    }

    public Collision WarmStarting(Collision oldCollision, Collision newCollision)
    {
        Contact newContact = newCollision.GetContact(0);
        Shape shapeA = oldCollision.m_shapeA;
        Shape shapeB = oldCollision.m_shapeB;
        //先判断之前的点是否仍然有效
        List<Contact> contacts = oldCollision.GetContacts();
        for(int i = 0; i < contacts.Count;)
        {
            Contact contact = contacts[i];
            Vector3 localToGlobalA = shapeA.LocalToGlobal(contact.localPositionA);
            Vector3 localToGlobalB = shapeB.LocalToGlobal(contact.localPositionB);
            Vector3 rAB = localToGlobalB - localToGlobalA;
            Vector3 rA = contact.globalPositionA - localToGlobalA;
            Vector3 rB = contact.globalPositionB - localToGlobalB;
            bool stillPenetrating = Vector3.Dot(rAB, contact.normal) <= 0;
            bool rACloseEnough = rA.magnitude < PhysicsSettings.PersistentThreshold;
            bool rBCloseEnough = rB.magnitude < PhysicsSettings.PersistentThreshold;
            if (stillPenetrating && rACloseEnough && rBCloseEnough) i++;
            else contacts.Remove(contact);
        }

        //判断是否已有相同的接触点
        bool haveSamePoint = false;
        for (int i = 0; i < contacts.Count; i++)
        {
            Vector3 rA = newContact.globalPositionA - contacts[i].globalPositionA;
            Vector3 rB = newContact.globalPositionB - contacts[i].globalPositionB;
            bool rAFarEnough = rA.magnitude > PhysicsSettings.PersistentThreshold;
            bool rBFarEnough = rB.magnitude > PhysicsSettings.PersistentThreshold;
            if (!(rAFarEnough || rBFarEnough))
            {
                haveSamePoint = true;
                contacts[i].penetration = newContact.penetration;
                break;
            }
        }
        if (!haveSamePoint)
        {
            contacts.Add(newContact);
        }

        //当多于4个接触点时，删去渗透深度最小的接触点
        if (contacts.Count == 5)
        {
            float minPenetration = float.MaxValue;
            int removeIdx = -1;
            for (int i = 0; i < contacts.Count; i++)
            {
                if (contacts[i].penetration < minPenetration)
                {
                    minPenetration = contacts[i].penetration;
                    removeIdx = i;
                }
            }
            contacts.RemoveAt(removeIdx);
        }
        return newCollision;
    }

    public float GetAngleConstraintScaler(Vector3 w, Vector3 r)
    {
        if (w.magnitude <= Vector3.kEpsilon || r.magnitude <= Vector3.kEpsilon) return 0f;
        Vector3 wCrossR = Vector3.Cross(w, r);
        float theta = w.magnitude;
        float scaler = Mathf.Tan(theta) * r.magnitude * (1.0f / wCrossR.magnitude);
        return scaler;
    }

    public void InitConstraintInfo(Shape shapeA, Shape shapeB, Contact contact)
    {
        ConstraintInfo info = new ConstraintInfo();
        RigidBody rbA = shapeA.GetRigidBody();
        RigidBody rbB = shapeB.GetRigidBody();
        List<Vector3> temp = new List<Vector3>();

        Vector3 pointA = shapeA.GetContactPointInDirection(contact.normal);
        Vector3 pointB = shapeB.GetContactPointInDirection(-contact.normal);
        Vector3 ra = pointA - shapeA.GetCenterPositionInGlobal();
        Vector3 rb = pointB - shapeB.GetCenterPositionInGlobal();
        temp.Add(-contact.normal);
        temp.Add(-Vector3.Cross(ra, contact.normal));
        temp.Add(contact.normal);
        temp.Add(Vector3.Cross(rb, contact.normal));
        info.Jn = new Matrix(temp);

        temp.Clear();
        Vector3 rbALinearVelocity = rbA.GetLinearVelocity();
        Vector3 rbAAngularVelocity = rbA.GetAngularVelocity();
        Vector3 rbBLinearVelocity = rbB.GetLinearVelocity();
        Vector3 rbBAngularVelocity = rbB.GetAngularVelocity();
        float rbAAngularScaler = GetAngleConstraintScaler(rbAAngularVelocity, ra);
        float rbBAngularScaler = GetAngleConstraintScaler(rbBAngularVelocity, rb);
        Vector3 relativeVelocity = rbALinearVelocity - rbBLinearVelocity;
        relativeVelocity += Vector3.Cross(rbAAngularVelocity, ra) * rbAAngularScaler - Vector3.Cross(rbBAngularVelocity, rb) * rbBAngularScaler;
        Vector3 biNormal = Vector3.Cross(relativeVelocity, contact.normal).normalized;
        Vector3 tangent = Vector3.Cross(contact.normal, biNormal).normalized;
        temp.Add(-tangent);
        temp.Add(-Vector3.Cross(ra, tangent));
        temp.Add(tangent);
        temp.Add(Vector3.Cross(rb, tangent));
        info.Jt = new Matrix(temp);
        Debug.DrawLine(shapeA.GetCenterPositionInGlobal(), shapeA.GetCenterPositionInGlobal() + tangent * 2);

        temp.Clear();
        temp.Add(-biNormal);
        temp.Add(-Vector3.Cross(ra, biNormal));
        temp.Add(biNormal);
        temp.Add(Vector3.Cross(rb, biNormal));
        info.Jb = new Matrix(temp);

        temp.Clear();
        temp.Add(rbALinearVelocity);
        temp.Add(rbAAngularVelocity * rbAAngularScaler);
        temp.Add(rbBLinearVelocity);
        temp.Add(rbBAngularVelocity * rbBAngularScaler);
        Matrix V = new Matrix(temp).Transpose();
        info.V = V;

        Matrix inverseM = new Matrix(12, 12);
        inverseM.SetValue(0, 0, rbA.GetInverseMassMatrix());
        inverseM.SetValue(3, 3, new Matrix(rbA.GetGlobalInverseInertiaTensor()));
        inverseM.SetValue(6, 6, rbB.GetInverseMassMatrix());
        inverseM.SetValue(9, 9, new Matrix(rbB.GetGlobalInverseInertiaTensor()));
        info.inverseM = inverseM;

        contact.normalInfo = info;
    }

    public void ContactConstraintSolver(Shape shapeA, Shape shapeB, Contact contact, JacobianType type)
    {
        RigidBody rbA = shapeA.GetRigidBody();
        RigidBody rbB = shapeB.GetRigidBody();

        ConstraintInfo info = contact.normalInfo;
        Matrix J = null;
        switch (type)
        {
            case JacobianType.Normal:
                J = info.Jn;
                break;
            case JacobianType.Tangent:
                J = info.Jt;
                break;
            default:
                break;
        }
        Matrix JV = J * info.V;
        Matrix JInverseMJT = J * info.inverseM * J.Transpose();

        float d = Mathf.Max(contact.penetration - PhysicsSettings.BaumgarteSlop, 0);
        float baumgarteTerm = -PhysicsSettings.BaumgarteScale / PhysicsSettings.DeltaTime * d;

        float lambda = 0;
        if (JInverseMJT[0, 0] != 0) lambda = -(JV[0, 0] + baumgarteTerm) / JInverseMJT[0, 0];
        if (lambda == 0) return;

        Matrix inverseMJT = info.inverseM * J.Transpose();

        switch (type)
        {
            case JacobianType.Normal:
                float oldImpulseSum = contact.normalImpulseSum;
                contact.normalImpulseSum = Mathf.Clamp(oldImpulseSum + lambda, 0, float.MaxValue);
                lambda = contact.normalImpulseSum - oldImpulseSum;
                break;
            case JacobianType.Tangent:
                float coefficientOfFriction = (shapeA.GetFriction() + shapeB.GetFriction()) / 2;
                oldImpulseSum = contact.tangentImpulseSum;
                float limit = coefficientOfFriction * contact.normalImpulseSum;
                contact.tangentImpulseSum = Mathf.Clamp(oldImpulseSum + lambda, -limit, limit);
                lambda = contact.tangentImpulseSum - oldImpulseSum;
                J = info.Jt;
                break;
            default:
                break;
        }

        Vector3 deltaVa = new Vector3(inverseMJT[0, 0], inverseMJT[1, 0], inverseMJT[2, 0]) * lambda;
        Vector3 deltaWa = new Vector3(inverseMJT[3, 0], inverseMJT[4, 0], inverseMJT[5, 0]) * lambda;
        Vector3 deltaVb = new Vector3(inverseMJT[6, 0], inverseMJT[7, 0], inverseMJT[8, 0]) * lambda;
        Vector3 deltaWb = new Vector3(inverseMJT[9, 0], inverseMJT[10, 0], inverseMJT[11, 0]) * lambda;

        rbA.AddVelocity(deltaVa, deltaWa);
        rbB.AddVelocity(deltaVb, deltaWb);
    }
}
