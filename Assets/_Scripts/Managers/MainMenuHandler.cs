namespace nexx.Manager
{
    using nexx.Saving;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    public class MainMenuHandler : MonoBehaviour
    {
        [SerializeField] private Button continueButton;


        public void Start()
        {
            continueButton.interactable = SaveManager.SaveExists("Solitaire");
        }


        public void CreateNewGame()
        {
            SaveManager.RemoveSave("Solitaire");
            InternalGameHandler();
        }


        public void LoadGame() => InternalGameHandler();

        private void InternalGameHandler()
        {
            SceneManager.LoadScene(1);
        }

        public void QuitGame() => Application.Quit();
    }
}