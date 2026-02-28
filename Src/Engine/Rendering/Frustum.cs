using System;
using System.Numerics;

namespace SpatialEngine.Rendering
{
    public struct Frustum
    {
        struct Plane
        {
            public Vector3 normal;
            public float distance;

            public Plane()
            {
                normal = new Vector3(0f);
                distance = 0f;
            }

            public Plane(Vector3 p1, Vector3 normal)
            {
                this.normal = Vector3.Normalize(normal);
                this.distance = Vector3.Dot(p1, this.normal);
            }

            public float GetSignedDistanceToPlane(Vector3 point)
            {
                return Vector3.Dot(normal, point) - distance;
            }
        };
        
        Plane topFace;
        Plane bottomFace;

        Plane rightFace;
        Plane leftFace;

        Plane farFace;
        Plane nearFace;
        
        
        public void CreateFromCamera(in Camera cam)
        {
            //increase the fov a little bit
            float halfVSide = cam.farPlane * MathF.Tan((cam.zoom * MathF.PI / 180.0f) * 0.8f);
            float halfHSide = halfVSide * cam.aspect;
            Vector3 frontMultFar = cam.farPlane * cam.GetCamDir();

            Vector3 up = cam.GetCameraUp();
            Vector3 forward = cam.GetCamDir();
            Vector3 right = Vector3.Cross(forward, up);
            nearFace = new Plane(cam.position + cam.nearPlane * forward, forward);
            farFace = new Plane(cam.position + frontMultFar, -forward );
            rightFace = new Plane(cam.position, Vector3.Cross(frontMultFar - right * halfHSide, up));
            leftFace = new Plane(cam.position, Vector3.Cross(up, frontMultFar + right * halfHSide));
            topFace = new Plane(cam.position, Vector3.Cross(right, frontMultFar - up * halfVSide));
            bottomFace = new Plane(cam.position, Vector3.Cross(frontMultFar + up * halfVSide, right));
        }
        
        
        public bool IsInFrustum(Vector3 pos, Vector3 extents)
        {
            return (IsOnOrForwardPlane(leftFace, pos, extents) &&
                    IsOnOrForwardPlane(rightFace, pos, extents) &&
                    IsOnOrForwardPlane(topFace, pos, extents) &&
                    IsOnOrForwardPlane(bottomFace, pos, extents) &&
                    IsOnOrForwardPlane(nearFace, pos, extents) &&
                    IsOnOrForwardPlane(farFace, pos, extents));
        }


        bool IsOnOrForwardPlane(Plane plane, Vector3 pos, Vector3 extents)
        {
            float r = extents.X * MathF.Abs(plane.normal.X) + extents.Y * MathF.Abs(plane.normal.Y) + extents.Z * MathF.Abs(plane.normal.Z);

            return -r <= plane.GetSignedDistanceToPlane(pos);
        }
            
    }
}