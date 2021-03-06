using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EnemyAttackTarget))]
public class AutomatedSetup : Editor
{
#if UNITY_EDITOR
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Automated Setup"))
        {
            foreach (GameObject obj in Selection.gameObjects)
            {
                EnemyAttackTarget thisScript = obj.GetComponent<EnemyAttackTarget>();
                Collider2D c = obj.GetComponent<Collider2D>();
                if (c != null)
                {
                    thisScript.Collider = c;
                }

                Rigidbody2D r = obj.GetComponent<Rigidbody2D>();
                if (r != null)
                {
                    thisScript.RigidBody = r;
                }

                Renderer[] rd = obj.transform.GetComponentsInChildren<Renderer>();
                if (rd != null)
                {
                    thisScript.Renderer = rd;
                }
            }
        }
        base.OnInspectorGUI();
    }
#endif
}