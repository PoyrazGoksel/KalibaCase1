using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Extensions.Unity
{
    [Serializable]
    public class SerializedInterface<T> : ISerializationCallbackReceiver where T : class 
    {
        [OnValueChanged("AssignObject")][ShowInInspector] private T InterfaceRef;

        public T Value
        {
            get => AssignedObject as T;
            set => AssignedObject = value as Object;
        }

        public SerializedInterface(Object uniObj)
        {
            AssignedObject = uniObj;
            InterfaceRef = uniObj as T;
        }
        
        [HideInInspector][SerializeField] private Object AssignedObject;

        private void AssignObject()
        {
            AssignedObject = InterfaceRef as Object;
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            InterfaceRef = AssignedObject as T;
        }
    }
}