using UnityEditor;
using UnityEngine;

namespace Tofunaut.ShapeMath2D_Unity.Editor
{
    [CustomEditor(typeof(ShrinkWrapDemo))]
    public class ShrinkWrapDemoEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if(GUILayout.Button("Regenerate"))
                (target as ShrinkWrapDemo)?.Regenerate();
            
            base.OnInspectorGUI();
        }
    }
    
}