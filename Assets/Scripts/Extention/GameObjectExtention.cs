using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Client.Extention
{
    public static class GameObjectExtention
    {
        private static readonly Queue<Transform> _traversalQueue = new();
        
        public static T Find<T>(this Transform self, string name) where T : Component
        {
            _traversalQueue.Clear();
            _traversalQueue.Enqueue(self);
            while (_traversalQueue.Count > 0)
            {
                var x = _traversalQueue.Dequeue();
                for (var i = 0; i < x.childCount; i++)
                {
                    var child = x.GetChild(i);
                    if (child.name == name && child.GetComponent<T>() != default)
                    {
                        return child.GetComponent<T>();
                    }

                    if (child.childCount > 0)
                    {
                        _traversalQueue.Enqueue(child);
                    }
                }
            }

            return null;
        }

        public static T GetCustomAttribute<T>(this MonoBehaviour self) where T : System.Attribute
        {
            return self.GetType().GetCustomAttribute<T>();
        }
        
        public static void DontDestroyOnLoad2(this GameObject self)
        {
            if (self.transform.parent != null)
            {
                self.transform.SetParent(null, true);
            }

            Object.DontDestroyOnLoad(self);
        }
        
        public static T Assert<T>(this T self) where T : Object
        {
            Debug.Assert(self != default);
            return self;
        }
    }
}