using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Vertigo.Wheel.Editor.SceneBuilding
{
    /// <summary>
    /// Writes private [SerializeField] references the same way the Inspector does.
    /// </summary>
    public sealed class SerializedFieldWriter : IDisposable
    {
        private readonly SerializedObject _serializedObject;

        public SerializedFieldWriter(Object target)
        {
            _serializedObject = new SerializedObject(target);
        }

        public SerializedFieldWriter Set(string fieldName, Object value)
        {
            GetProperty(fieldName).objectReferenceValue = value;
            return this;
        }

        public SerializedFieldWriter Set(string fieldName, int value)
        {
            GetProperty(fieldName).intValue = value;
            return this;
        }

        public SerializedFieldWriter Set(string fieldName, Vector2 value)
        {
            GetProperty(fieldName).vector2Value = value;
            return this;
        }

        public SerializedFieldWriter SetArray<T>(string fieldName, T[] values) where T : Object
        {
            SerializedProperty array = GetProperty(fieldName);
            array.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                array.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            return this;
        }

        public SerializedProperty GetProperty(string fieldName)
        {
            return _serializedObject.FindProperty(fieldName)
                ?? throw new ArgumentException($"{_serializedObject.targetObject.GetType().Name} has no serialized field '{fieldName}'.");
        }

        public void Dispose()
        {
            _serializedObject.ApplyModifiedPropertiesWithoutUndo();
            _serializedObject.Dispose();
        }
    }
}
