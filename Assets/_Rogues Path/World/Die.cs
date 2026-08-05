using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Rogues_Path.World {
    public class Die: MonoBehaviour {
        public Rigidbody RigidBody;

        [SerializeField] private List<Transform> DieFaces = new();
        public int? ReadDie() {
            if (RigidBody.angularVelocity.magnitude > 0.1f) {
                return null;
            }

            foreach (var face in DieFaces) {
                if (face.up == Vector3.up) {
                    return DieFaces.IndexOf(face) + 1;
                }
            }

            return null;
        }
    }
}