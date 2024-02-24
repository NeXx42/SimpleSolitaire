namespace nexx.Saving
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class SerializableGameobject : MonoBehaviour
    {
        [SerializeField] private string identifier;

#if UNITY_EDITOR

        public string SetIdentity(int multi, out bool wasChanged)
        {
            wasChanged = false;

            if(identifier == "")
            {
                identifier = (transform.GetInstanceID() * multi).ToString();
                wasChanged = true;
            }

            return identifier;
        }

#endif

        public string getIdentity => identifier;
    }
}