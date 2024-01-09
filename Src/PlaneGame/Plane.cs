using SpatialEngine;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using static SpatialEngine.Globals;
using static SpatialEngine.Math;
using static SpatialEngine.Debugging;
using Silk.NET.Input;
using System.Diagnostics;

namespace PlaneGame
{
    public class Plane
    {
        public int id;

        public Vector3 totalForce = Vector3.Zero;
        public Vector3 velocity = Vector3.Zero;
        public LiftCurve liftCurve = new LiftCurve(0.671f, -0.35f, 1f);
        public LiftCurve dragCurve = new LiftCurve(-0.41f, 0.0f, 0.0f);

        public float mass = 100f;

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
            totalForce = Vector3.Zero;
            float aoa = GetAngleOfAttack();
            ApplyDragForce(1.0f, 5.0f, aoa);
            ApplyLiftForce(1.0f, 5.0f, aoa);
            totalForce += forward * 10000;

            DrawDebugLine(scene.SpatialObjects[id].SO_rigidbody.GetVelocity() + scene.SpatialObjects[id].SO_rigidbody.GetPosition(), scene.SpatialObjects[id].SO_rigidbody.GetPosition(), new Vector3(255, 255, 0));
            DrawDebugLine(totalForce / mass + scene.SpatialObjects[id].SO_rigidbody.GetPosition(), scene.SpatialObjects[id].SO_rigidbody.GetPosition(), new Vector3(255, 255, 255));
            scene.SpatialObjects[id].SO_rigidbody.AddVelocity((totalForce / mass) * deltaTime);
            //had to invert the matrix for god knows why
            Matrix4x4.Invert(scene.SpatialObjects[id].SO_mesh.modelMat, out Matrix4x4 mat);
            forward = ApplyMatrixVec3(Vector3.UnitZ, mat);
            right = ApplyMatrixVec3(Vector3.UnitX, mat);
            backward = -forward;
            left = -right;
            up = Vector3.Cross(forward, right);
            down = -up;
        }

        float GetAngleOfAttack()
        {
            return Vector3Angle(up, scene.SpatialObjects[id].SO_rigidbody.GetVelocity()) - 90.0f;
        }

        void ApplyDragForce(float airDensity, float area, float angle)
        {
            Vector3 velocity = scene.SpatialObjects[id].SO_rigidbody.GetVelocity();
            float speed = velocity.Length();
            float dragForce = (-0.5f * 0.0175f * area) * airDensity * speed * dragCurve.Evaluate(MathF.Abs(angle));
            if(speed != 0)
                totalForce += dragForce * Vector3.Normalize(velocity);
        }

        void ApplyLiftForce(float airDensity, float area, float angle)
        {
            float speed = scene.SpatialObjects[id].SO_rigidbody.GetVelocity().Length();
            float liftForce = (0.5f * 1.9f * area) * airDensity * speed * MathF.Sign(angle) * liftCurve.Evaluate(MathF.Abs(angle));
            if(up != Vector3.Zero)
                totalForce += liftForce * Vector3.Normalize(up);
        }

        public class LiftCurve
        {
            public float size;
            public float size2;
            public float size3;

            public LiftCurve(float a, float b, float c)
            {
                this.size = a;
                this.size2 = b;
                this.size3 = c;
            }

            public float Evaluate(float input)
            {
                if (input > 90.0f)
                    return size * (1.567306f + size2) * (1.567306f + size2) + size3;
                float angle = input * (MathF.PI / 180.0f);
                float calc = size * ((angle + size2) * (angle + size2)) + size3;
                if (calc < 0.0f)
                    return 0;
                return calc;
            }
        }
    }
}
