using System.Collections.Generic;
using Components.Main.Grids;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Extensions.Unity
{
    public static class CameraExt
    {
        
        public static void Encapsulate(this Camera camera, Transform camTrans, List<Vector3> arg0, Vector3 offSet)
        {
            Vector3 totPos = Vector3.zero;
            int itemCount = 0;

            foreach (Vector3 pos in arg0)
            {
                totPos += pos;
                itemCount ++;
            }

            Vector3 center = totPos / itemCount;

            Bounds bounds = new Bounds(center, Vector3.one);

            foreach (Vector3 pos in arg0)
            {
                bounds.Encapsulate(pos);
            }

            camTrans.position = bounds.center + offSet;
            Vector3 diff = camTrans.position - bounds.center;
            float distToFoc = diff.magnitude;
            float currentProjectionOnGrid = Mathf.Tan(camera.fieldOfView) * distToFoc * 2f;

            camTrans.position = bounds.center + (diff * bounds.max.x / currentProjectionOnGrid);
            camTrans.LookAt(bounds.center);
        }
        public static Texture2D CaptureScreen(this Camera camera, CaptureData captureData)
        {
            RenderTexture active = RenderTexture.active;

            RenderTexture.active = RenderTexture.GetTemporary
            (
                captureData.Width,
                captureData.Height,
                captureData.Depth,
                captureData.GraphicsFormat
            );
            
            camera.targetTexture = RenderTexture.active;
            camera.Render();

            Texture2D newPhoto = new Texture2D
            (
                RenderTexture.active.width / captureData.Scale,
                RenderTexture.active.height / captureData.Scale,
                captureData.TextureFormat,
                false,
                true
            );

            newPhoto.ReadPixels
            (
                new Rect
                (
                    captureData.X,
                    captureData.Y,
                    RenderTexture.active.width / (float)captureData.Scale,
                    RenderTexture.active.height / (float)captureData.Scale
                ),
                0,
                0
            );
            
            newPhoto.Apply();

            camera.targetTexture = null;
            RenderTexture.ReleaseTemporary(RenderTexture.active);
            RenderTexture.active = active;

            return newPhoto;
        }
        
        public readonly struct CaptureData
        {
            public readonly int Scale;
            public readonly int Width;
            public readonly int Height;
            public readonly float X;
            public readonly float Y;
            public readonly int Depth;
            public readonly TextureFormat TextureFormat;
            public readonly GraphicsFormat GraphicsFormat;
            
            public CaptureData
            (
                int scale, 
                int width, 
                int height,
                float x,
                float y,
                int depth,
                TextureFormat textureFormat,
                GraphicsFormat graphicsFormat
            )
            {
                Scale = scale;
                Width = width;
                Height = height;
                X = x;
                Y = y;
                Depth = depth;
                TextureFormat = textureFormat;
                GraphicsFormat = graphicsFormat;
            }
        }
    }
}