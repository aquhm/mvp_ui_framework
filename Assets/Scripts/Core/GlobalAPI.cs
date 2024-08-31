using System;
using Client.Service;
using UnityEngine;

namespace Core
{
    public static class GlobalAPI
    {
        private static Services s_serviceHolder;
        public static TestApplication App { get; private set; }

        public static UiService UiService => s_serviceHolder?.UiService;
        public static TestService TestService => s_serviceHolder?.TestService;

        public static void Init(TestApplication app)
        {
            App = app;
            s_serviceHolder ??= App.Services;

            Debug.Log("[GlobalAPI] Init");
        }


        public static void OnRelease()
        {
            Debug.Log("[GlobalAPI] OnRelease");

            s_serviceHolder?.Release();
        }


        public static void Destroy()
        {
            Debug.Log("[GlobalAPI] Destroy");
            GC.Collect();
        }

        public static void QuitApplication(bool isForce = false)
        {
            App.QuitApplication(isForce).Forget();
        }
    }
}