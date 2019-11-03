using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace WaterKat.AudioReader
{
    public class AudioReader : MonoBehaviour
    {
        private static AudioReader _singleton = null;
        private static readonly object _lockobject = new object();
        public static AudioReader instance
        {
            get
            {
                if (_singleton != null)
                {
                    return _singleton;
                }
                else
                {
                    GameObject newHome = new GameObject();
                    AudioReader newStaticClass = newHome.AddComponent<AudioReader>();
                    newHome.name = typeof(AudioReader).Name;
                    return newStaticClass;
                }
            }
        }
        private void Awake()
        {
            lock (_lockobject)
            {
                _singleton = this;

                AudioReader[] genericClasses = FindObjectsOfType<AudioReader>();
                foreach (AudioReader genericClass in genericClasses)
                {
                    if (genericClass != this)
                    {
                        genericClass.gameObject.SetActive(false);
                        Destroy(genericClass);
                        Destroy(genericClass.gameObject);
                    }
                }
                transform.gameObject.name = this.GetType().Name;
            }
        }

        //        Dictionary<AudioClip,>

        class AudioDataSet
        {
            AudioClip audioClip;
            
        }

        public static void deltaAudioSample(AudioClip audioClip)
        {
          //  audioClip
        }
    }
}
