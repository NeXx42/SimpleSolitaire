namespace nexx.Editor
{
    using nexx.Saving;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEditor.PackageManager.UI;
    using UnityEngine;


    public class Editor_SceneSerializor : EditorWindow
    {
        [MenuItem("Nexx/Serialize Scene")]
        static void Init()
        {
            Editor_SceneSerializor window = (Editor_SceneSerializor)EditorWindow.GetWindow(typeof(Editor_SceneSerializor));
            window.Show();
        }

        void OnGUI()
        {
            if(GUILayout.Button("Serialize Scene"))
            {
                SerializableGameobject[] objs = GameObject.FindObjectsOfType<SerializableGameobject>(true);
                FindConflicts(objs);
            }
        }


        private void FindConflicts(SerializableGameobject[] objs)
        {
            Dictionary<string, bool> existingIdentities = new Dictionary<string, bool>();

            foreach (SerializableGameobject obj in objs)
            {
                string str = obj.SetIdentity(Random.Range(0, 100), out bool isNew);

                if (existingIdentities.ContainsKey(str))
                {
                    Debug.Log($"Conflicting id found at {obj.name}, the problem is {(isNew ? "not systemic" : "systemic")}");
                }
                else
                    existingIdentities.Add(str, isNew);
            }
        }
    }
}