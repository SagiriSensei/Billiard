using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsSettings : MonoBehaviour
{
    //����ů�����жϽӴ����Ƿ���Ч
    public float m_persistentThreshold;
    public float m_baumgarteScale;
    public float m_baumgarteSlop;
    public float m_deltaTime;
    public float m_gravity;
    public static float PersistentThreshold;
    public static float BaumgarteScale;
    public static float BaumgarteSlop;
    public static float DeltaTime;
    public static float Gravity;

    private void Awake()
    {
        PersistentThreshold = m_persistentThreshold;
        BaumgarteScale = m_baumgarteScale;
        BaumgarteSlop = m_baumgarteSlop;
        DeltaTime = m_deltaTime;
        Gravity = m_gravity;
    }
}
