using System;
using System.Numerics;
using GameTestingUtilites;

namespace GameTesting
{
    public class Camera
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 target;
        public Vector3 direction;
        public Matrix4x4 viewMat;
        public float zoom;

        public Camera(Vector3 position, Quaternion rotation, Vector3 target, float zoom)
        {
            this.position = position;
            this.rotation = rotation;
            this.target = target;
            direction = GetCamDir();
            this.zoom = zoom;
        }

        public Vector3 GetCamDir()
        {
            return Vector3.Normalize(position - target);
        }

        public Vector3 GetCameraRight()
        {
            return Vector3.Normalize(Vector3.Cross(Vector3.UnitY, direction));
        }

        public Vector3 GetCameraFront()
        {
            return Vector3.Cross(Vector3.UnitY, GetCameraRight());
        }

        public void SetViewMat()
        {
            viewMat = MathExt.MatrixLookAt(position, position - new Vector3(0,0,-1), Vector3.UnitY); 
        }

        public Matrix4x4 GetProjMat()
        {
            return MathExt.MatrixPerspective(MathF.PI / 180 * zoom, 1.77777f, 0.0001f, 100f);
        }

        public Ray ScreenToWorldCast(float msX, float msY)
        {
            SetViewMat();
            msX = 1 - (2 * msX) / Game.SCR_WIDTH;
            msY = 1 - (2 * msY) / Game.SCR_HEIGHT;
            Console.WriteLine(msX + " " + msY);
            Vector3 near = MathExt.ApplyMatrixVec3Proj(new Vector3(msX, msY, 0), GetProjMat(), viewMat);
            Vector3 far = MathExt.ApplyMatrixVec3Proj(new Vector3(msX, msY, 1), GetProjMat(), viewMat);
            Ray ray = new Ray();
            ray.direction = Vector3.Normalize(far - near);
            ray.origin = position;
            return ray;
        }
        
    }
}