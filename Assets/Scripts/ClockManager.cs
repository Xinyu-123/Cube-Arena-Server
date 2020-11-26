using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    class ClockManager: MonoBehaviour
    {
        public static ClockManager instance;

        private void Awake()
        {
            if (instance == null)
                instance = this;
            else if (instance != this)
            {
                Debug.Log("Instance already exists, destroying object!");
                Destroy(this);
            }

        }

        private void Start()
        {
            StartCoroutine(SyncClock());
        }

        private IEnumerator SyncClock()
        {
            ServerSend.SyncClock();

            yield return new WaitForSeconds(1f);
            StartCoroutine(SyncClock());
        }
    }
}
