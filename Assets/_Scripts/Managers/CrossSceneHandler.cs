namespace nexx.Manager
{

    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.SceneManagement;

    public class CrossSceneHandler : MonoBehaviour
    {
/*        [Header("Main")]
        [SerializeField] private AdManager admanager;*/


        private UnityAction onSceneChange;


        private void Awake()
        {
            SceneManager.activeSceneChanged += OnSceneChange;

            //admanager.Setup(ref onSceneChange);
            DontDestroyOnLoad(gameObject);
        }

        private void OnSceneChange(Scene start, Scene end)
        {
            onSceneChange?.Invoke();
        }
    }
}