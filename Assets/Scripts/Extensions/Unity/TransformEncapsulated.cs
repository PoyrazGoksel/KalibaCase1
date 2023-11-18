using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Extensions.Unity
{
    [Serializable]
    public class TransformEncapsulated
    {
        public Vector3 position => _transform.position;
        public Quaternion rotation => _transform.rotation;
        public Vector3 eulerAngles => _transform.eulerAngles;
        public Vector3 localPosition => _transform.localPosition;
        public Quaternion localRotation => _transform.localRotation;
        public Vector3 localEulerAngles => _transform.localEulerAngles;
        
        [SerializeField] private Transform _transform;
        
        public TransformEncapsulated(Transform transform)
        {
            _transform = transform;
        }
        
        public static implicit operator Transform(TransformEncapsulated transformEncapsulated)
        {
                return transformEncapsulated._transform;
        }

        public static explicit operator TransformEncapsulated
        (Transform transform)
        {
                return new TransformEncapsulated(transform);
        }
    }
}