using SpatialEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using static SpatialEngine.Globals;
using static SpatialEngine.Math;

namespace PlaneGame
{
    public class Plane
    {
        public int id;

        public Vector3 totalForce = Vector3.Zero;
        public Vector3 velocity = Vector3.Zero;

        Vector3 up;
        Vector3 down;
        Vector3 forward;
        Vector3 backward;
        Vector3 left;
        Vector3 right;

        public Plane(int id)
        {
            this.id = id;
        }

        public void Update(float deltaTime)
        {
            ApplyDragForce(1.0f, 1.0f);
            //ApplyLiftForce(1.0f, 20.0f);

            velocity += totalForce * deltaTime;

            Vector4 tmpForward = ApplyMatrix(new Vector4(1, 0, 0, 1), bodyInterface.GetWorldTransform(scene.SpatialObjects[id].SO_rigidbody.rbID));
            Vector4 tmpRight = ApplyMatrix(new Vector4(0, 0, 1, 1), bodyInterface.GetWorldTransform(scene.SpatialObjects[id].SO_rigidbody.rbID));
            forward = new Vector3(tmpForward.X, tmpForward.Y, tmpForward.Z);
            right = new Vector3(tmpRight.X, tmpRight.Y, tmpRight.Z);
            backward = -forward;
            left = -right;
            up = Vector3.Cross(forward, right);
            down = -up;
        }
        void ApplyDragForce(float airDensity, float area)
        {
            totalForce += backward * (0.5f * airDensity * area * 0.0175f * -scene.SpatialObjects[id].SO_rigidbody.GetVelocity().Length());
        }

        void ApplyLiftForce(float airDensity, float area)
        {
            float speed = scene.SpatialObjects[id].SO_rigidbody.GetVelocity().Length();
            totalForce += Vector3.UnitY * (0.5f * airDensity * speed * speed * area * 1.9f);
        }
    }
}
