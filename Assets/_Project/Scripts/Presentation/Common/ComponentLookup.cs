using UnityEngine;

namespace Vertigo.Wheel.Presentation.Common
{
    public static class ComponentLookup
    {
        /// <summary>
        /// Finds a component on a child (inactive included) whose GameObject has the given name.
        /// Used from OnValidate so serialized references are wired automatically in the editor.
        /// </summary>
        public static T FindInChildren<T>(this Component root, string objectName) where T : Component
        {
            foreach (T component in root.GetComponentsInChildren<T>(true))
            {
                if (component.name == objectName)
                {
                    return component;
                }
            }

            return null;
        }

        public static void AssignFromChildren<T>(this Component root, ref T field, string objectName) where T : Component
        {
            T found = root.FindInChildren<T>(objectName);
            if (found != null)
            {
                field = found;
            }
            else if (field == null)
            {
                Debug.LogWarning($"{root.name}: no child '{objectName}' with {typeof(T).Name} found.", root);
            }
        }
    }
}
