using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RespawnBehaviour))]
public class RespawnBehaviourSetter : Editor
{
#if UNITY_EDITOR

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Automated Setup"))
        {
            foreach (GameObject obj in Selection.gameObjects)
            {
                RespawnBehaviour thisScript = obj.GetComponent<RespawnBehaviour>();
                thisScript.originalPosition = obj.transform.position;
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

                Renderer rd = obj.GetComponent<Renderer>();
                if (rd)
                {
                    thisScript.Renderer = rd;
                }
            }
        }
        base.OnInspectorGUI();
    }
#endif
}