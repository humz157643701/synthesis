﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Synthesis.Simulator
{
    /**
     * This class is responsible for handling robots, fields, and will act as the middle man between
     * elements of the simulator and all other components (DriverPractice, Controller, etc.)
     */
    public class SimulatorHandler
    {
        public event EventHandler<ProtoField> FieldSpawned;

        private SimulatorHandler() { }

        #region Load Functions

        public void LoadField(ProtoField field, Transform parent = null, Vector3? spawnposition = null, Quaternion? spawnRotation = null)
        {
            GameObject fieldObj = new GameObject(field.FieldName);

            List<ProtoNode> nodeList = new List<ProtoNode>(field.Nodes);

            List<GameObject> nodeObjects = new List<GameObject>();
            for (int i = 0; i < field.Nodes.Count; i++)
            {
                LoadNode(field.Nodes[i], out GameObject obj, parent = fieldObj.transform);
                nodeObjects.Add(obj);
            }

            // Check for Joints
            for (int i = 0; i < nodeList.Count; i++)
            {
                foreach (JointInfo jointInfo in nodeList[i].Joints)
                {
                    if (jointInfo.Type != JointType.NoJoint)
                    {
                        Rigidbody connectedBody = nodeObjects[nodeList.FindIndex(x => x.NodeID == jointInfo.CompanionID)].GetComponent<Rigidbody>();
                        switch (jointInfo.Type)
                        {
                            case JointType.Fixed:
                                nodeObjects[i].AddComponent<FixedJoint>().connectedBody = connectedBody;
                                break;
                            case JointType.Hinge:
                                HingeJoint j = nodeObjects[i].AddComponent<HingeJoint>();
                                j.connectedBody = connectedBody;
                                // Vector3 anchor = 
                                // Vector3 axis = 
                                j.anchor = new Vector3(jointInfo.Origin.X, jointInfo.Origin.Y, jointInfo.Origin.Z);
                                j.axis = new Vector3(jointInfo.Direction.X, jointInfo.Direction.Y, jointInfo.Direction.Z);
                                j.enableCollision = true;
                                break;
                        }
                    }
                }
            }

            fieldObj.transform.position = spawnposition ?? Vector3.zero;
            fieldObj.transform.rotation = spawnRotation ?? Quaternion.Euler(Vector3.zero);
            if (parent != null) fieldObj.transform.parent = parent;
        }

        public void LoadNode(ProtoNode node, out GameObject result, Transform parent = null, Vector3? spawnposition = null, Quaternion? spawnRotation = null)
        {
            GameObject nodeObj = new GameObject(node.Name);

            for (int i = 0; i < node.Bodies.Count; i++)
            {
                LoadBody(node.Bodies[i], parent = nodeObj.transform);
            }

            // Physics
            Rigidbody rb = nodeObj.AddComponent<Rigidbody>();
            if (node.IsDynamic)
            {
                rb.mass = node.Mass;
            } else
            {
                rb.constraints = RigidbodyConstraints.FreezeAll;
            }

            nodeObj.transform.position = spawnposition ?? Vector3.zero;
            nodeObj.transform.rotation = spawnRotation ?? Quaternion.Euler(Vector3.zero);
            if (parent != null) nodeObj.transform.parent = parent;
            result = nodeObj;
        }

        public void LoadBody(ProtoObject obj, Transform parent = null, Vector3? spawnposition = null, Quaternion? spawnRotation = null)
        {
            Vector3[] verts = new Vector3[obj.Verts.Count];
            for (int i = 0; i < verts.Length; i++)
            {
                verts[i] = new Vector3(obj.Verts[i].X, obj.Verts[i].Y, obj.Verts[i].Z); // - position;
            }
            Vector2[] uvs = new Vector2[obj.Uv.Count];
            for (int i = 0; i < uvs.Length; i++)
            {
                uvs[i] = new Vector2(obj.Uv[i].X, obj.Uv[i].Y);
            }
            int[] tris = new int[obj.Tris.Count];
            for (int i = 0; i < tris.Length; i++)
            {
                tris[i] = obj.Tris[i];
            }
            System.Array.Reverse(tris);

            Mesh m = new Mesh();
            m.vertices = verts;
            m.triangles = tris;
            m.uv = uvs;
            m.RecalculateBounds();
            m.RecalculateNormals();

            GameObject gameObj = new GameObject("_b");
            MeshFilter filter = gameObj.AddComponent<MeshFilter>();
            filter.mesh = m;
            MeshRenderer renderer = gameObj.AddComponent<MeshRenderer>();
            renderer.material = ObjectLedger.Instance.spawnMat;

            // Collider
            gameObj.AddComponent<MeshCollider>().convex = true;

            gameObj.transform.position = spawnposition ?? Vector3.zero;
            gameObj.transform.rotation = spawnRotation ?? Quaternion.Euler(Vector3.zero);
            if (parent != null) gameObj.transform.parent = parent;
        }

        #endregion

        private static SimulatorHandler instance;
        public static SimulatorHandler Instance {
            get {
                if (instance == null) instance = new SimulatorHandler();
                return instance;
            }
        }
    }
}