using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeonLadder.Managers
{
    public class EventManager : MonoBehaviour
    {
        private Dictionary<string, Dictionary<GameObject, Action<Collider>>> eventDictionary;

        void Awake()
        {
            if (eventDictionary == null)
            {
                eventDictionary = new Dictionary<string, Dictionary<GameObject, Action<Collider>>>();
            }
        }

        public void StartListening(string eventName, GameObject listener, Action<Collider> callback)
        {
            if (!eventDictionary.TryGetValue(eventName, out var listeners))
            {
                listeners = new Dictionary<GameObject, Action<Collider>>();
                eventDictionary[eventName] = listeners;
            }

            if (listeners.ContainsKey(listener))
            {
                listeners[listener] += callback;
            }
            else
            {
                listeners.Add(listener, callback);
            }
        }

        public void StopListening(string eventName, GameObject listener, Action<Collider> callback)
        {
            if (eventDictionary.TryGetValue(eventName, out var listeners))
            {
                if (listeners.ContainsKey(listener))
                {
                    listeners[listener] -= callback;
                    if (listeners[listener] == null)
                    {
                        listeners.Remove(listener);
                    }
                }
            }
        }

        public void TriggerEvent(string eventName, GameObject listener, Collider collider)
        {
            if (eventDictionary.TryGetValue(eventName, out var listeners))
            {
                if (listeners.TryGetValue(listener, out var thisEvent))
                {
                    thisEvent.Invoke(collider);
                }
            }
        }
    }
}