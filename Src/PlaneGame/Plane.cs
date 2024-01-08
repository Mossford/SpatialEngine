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

namespace PlaneGame
{
    public class Plane
    {
        public int id;

        public Vector3 totalForce = Vector3.Zero;
        public Vector3 velocity = Vector3.Zero;
        public LiftCurve liftCurve = new LiftCurve(0.671f, -0.35f, 1f);
        public LiftCurve dragCurve = new LiftCurve(-0.41f, 0.0f, 0.0f);

        public float mass = 1000f;

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
            ApplyDragForce(1.0f, 10.0f);
            ApplyLiftForce(1.0f, 10.0f);
            //totalForce += new Vector3(0,-9.81f,0);

            //velocity += deltaTime * totalForce;
            scene.SpatialObjects[id].SO_rigidbody.AddImpulseForce(totalForce * deltaTime, 1.0f);

            Vector4 tmpForward = ApplyMatrix(new Vector4(0, 0, 1, 1), bodyInterface.GetWorldTransform(scene.SpatialObjects[id].SO_rigidbody.rbID));
            Vector4 tmpRight = ApplyMatrix(new Vector4(1, 0, 0, 1), bodyInterface.GetWorldTransform(scene.SpatialObjects[id].SO_rigidbody.rbID));
            forward = new Vector3(tmpForward.X, tmpForward.Y, tmpForward.Z);
            right = new Vector3(tmpRight.X, tmpRight.Y, tmpRight.Z);
            backward = -forward;
            left = -right;
            up = Vector3.Cross(forward, right);
            down = -up;
        }

        float GetAngleOfAttack()
        {
            return Vector3Angle(up, scene.SpatialObjects[id].SO_rigidbody.GetVelocity()) - 90.0f;
        }

        void ApplyDragForce(float airDensity, float area)
        {
            Vector3 velocity = scene.SpatialObjects[id].SO_rigidbody.GetVelocity();
            float speed = velocity.Length();
            float dragForce = (-0.5f * 0.0175f * area) * airDensity * speed * dragCurve.Evaluate(MathF.Abs(GetAngleOfAttack()));
            if(speed != 0)
                totalForce += dragForce * Vector3.Normalize(velocity);
        }

        void ApplyLiftForce(float airDensity, float area)
        {
            float speed = scene.SpatialObjects[id].SO_rigidbody.GetVelocity().Length();
            float liftForce = (0.5f * 1.9f * area) * airDensity * speed * MathF.Sign(GetAngleOfAttack()) * liftCurve.Evaluate(MathF.Abs(GetAngleOfAttack()));
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
