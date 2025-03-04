using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor;
using UnityEngine;

public class AnimationClipReferenceConverter : EditorWindow
{
    private AnimationClip animationClip;
    private string root;

    public static void RemoveAllReferenceOf(AnimationClip clip, string reference)
    {
        EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);
        AnimationCurve[] curves = new AnimationCurve[bindings.Length];

        for(int i = 0; i < bindings.Length; i++)
        {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, bindings[i]);
            curves[i] = curve;

            if (!bindings[i].path.Contains(reference)) continue;
            string newPath = bindings[i].path.Replace(reference, string.Empty);

            Debug.Log(newPath);
            bindings[i].path = newPath;
        }

        AnimationUtility.SetEditorCurves(clip, bindings, curves);
    }
    [MenuItem("Window/AnimationClipReferenceConverter")]
    public static void ShowWindow()
    {
        // Show existing window instance. If one doesn't exist, make one.
        EditorWindow.GetWindow(typeof(AnimationClipReferenceConverter));
    }

    void OnGUI()
    {
        animationClip = (AnimationClip)EditorGUILayout.ObjectField("Animation Clip", animationClip, typeof(AnimationClip), false);

        if (animationClip == null) return;

        root = EditorGUILayout.TextField("Reference", root);

        if (root == string.Empty) return;

        if(GUILayout.Button("Convert clip"))
        {
            RemoveAllReferenceOf(animationClip, root);
        }
    }
}
