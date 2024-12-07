// Imported from https://gist.github.com/unitycoder/9da35f2f3f176c82f6dc6f4771fda50e

using UnityEditor;
using UnityEditor.ShortcutManagement;

[InitializeOnLoad]
public static class EnterPlayModeBindings
{
    static EnterPlayModeBindings()
    {
        EditorApplication.playModeStateChanged += ModeChanged;
        EditorApplication.quitting += Quitting;
    }

    static void ModeChanged(PlayModeStateChange playModeState)
    {
        if (playModeState == PlayModeStateChange.EnteredPlayMode)
            ShortcutManager.instance.activeProfileId = "Play";
        else if (playModeState == PlayModeStateChange.EnteredEditMode)
            ShortcutManager.instance.activeProfileId = "Default";
    }

    static void Quitting()
    {
        ShortcutManager.instance.activeProfileId = "Default";
    }
}