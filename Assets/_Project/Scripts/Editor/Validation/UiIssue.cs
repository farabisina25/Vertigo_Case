using UnityEngine;

namespace Vertigo.Wheel.Editor.Validation
{
    public enum UiRule
    {
        CanvasScaler,
        Naming,
        ValueSuffix,
        RaycastTarget,
        Maskable,
        AnimatorOnRoot,
        ImageType,
        StretchedImage,
        LegacyText,
        EditorEvents,
    }

    public readonly struct UiIssue
    {
        public UiIssue(UiRule rule, Object context, string message)
        {
            Rule = rule;
            Context = context;
            Message = message;
        }

        public UiRule Rule { get; }
        public Object Context { get; }
        public string Message { get; }

        public override string ToString()
        {
            return $"[{Rule}] {Message}";
        }
    }
}
