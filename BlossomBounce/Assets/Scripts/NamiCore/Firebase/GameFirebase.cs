using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Analytics;
using Firebase.Crashlytics;
using Firebase.Extensions;
using UnityEngine;

namespace Nami.Controller
{
    public class GameFirebase : MonoBehaviour
    {
        private static GameFirebase api;
        public static GameFirebase Get => api;

        private static bool firebaseInitialized = false;
        private Firebase.FirebaseApp appFirebase;

        // Start is called before the first frame update
        void Awake()
        {
            if(api != null)
            {
                Destroy(gameObject);
                return;
            }
            api = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            InitFirebase();
        }

        private void InitFirebase()
        {
            //Debug.Log("start init");
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
                var dependencyStatus = task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    // Create and hold a reference to your FirebaseApp,
                    // where app is a Firebase.FirebaseApp property of your application class.
                    InitializeFirebase();
                }
                else
                {
                    UnityEngine.Debug.LogError(System.String.Format(
                      "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                    // Firebase Unity SDK is not safe to use here.
                }
            });
        }

        // Handle initialization of the necessary firebase modules:
        void InitializeFirebase()
        {
            //DebugLog("Enabling data collection.");
            //FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);

            //DebugLog("Set user properties.");
            // Set default session duration values.
            Crashlytics.ReportUncaughtExceptionsAsFatal = true;
            appFirebase = Firebase.FirebaseApp.DefaultInstance;
            Application.logMessageReceived += HandleLog;
            LoadRemoteConfig();

            firebaseInitialized = true;

            //HandleLog("send test ", "stack trace ", LogType.Exception);
            Debug.Log("Initialize Firebase.");
        }

        private void LoadRemoteConfig()
        {
            Debug.Log("Load remote config");

            System.Threading.Tasks.Task fetchTask =
            Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.FetchAsync(
                TimeSpan.Zero);
            fetchTask.ContinueWithOnMainThread(FetchDataComplete);
        }

        void FetchDataComplete(Task fetchTask)
        {
            if (fetchTask.IsCanceled)
            {
                Debug.Log("Fetch canceled.");
            }
            else if (fetchTask.IsFaulted)
            {
                Debug.Log("Fetch encountered an error.");
            }
            else if (fetchTask.IsCompleted)
            {
                Debug.Log("Fetch completed successfully!");
            }


            var info = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.Info;
            switch (info.LastFetchStatus)
            {
                case Firebase.RemoteConfig.LastFetchStatus.Success:
                    Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.ActivateAsync()
                    .ContinueWithOnMainThread(task => {
                        //Debug.Log(string.Format("Remote data loaded and ready (last fetch time {0}).",
                        //           info.FetchTime));

                        //how to get value
                        GameAds.Get.time_inter_ad = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance
                                                  .GetValue("time_inter_ads").LongValue;

                        Debug.Log($"time_inter_ad: {GameAds.Get.time_inter_ad}");
                    });

                    break;
                case Firebase.RemoteConfig.LastFetchStatus.Failure:
                    switch (info.LastFetchFailureReason)
                    {
                        case Firebase.RemoteConfig.FetchFailureReason.Error:
                            Debug.Log("Fetch failed for unknown reason");
                            break;
                        case Firebase.RemoteConfig.FetchFailureReason.Throttled:
                            Debug.Log("Fetch throttled until " + info.ThrottledEndTime);
                            break;
                    }
                    break;
                case Firebase.RemoteConfig.LastFetchStatus.Pending:
                    Debug.Log("Latest Fetch call still pending.");
                    break;
            }
            //GameManager.Get.textMeshProToast.text += "-> FetchComplete ";
        }

        private static void SendEventFirebase(string nameEvent, params string[] parameters)
        {
            var arr = new List<Firebase.Analytics.Parameter>();
            if (parameters.Length % 2 != 0) return;
            for (int i = 0; i < parameters.Length; i += 2)
            {
                arr.Add(new Firebase.Analytics.Parameter(parameters[i], parameters[i + 1].Replace(".0", "")));
            }
            Firebase.Analytics.FirebaseAnalytics.LogEvent(nameEvent, arr.ToArray());
        }

        public static void SendEvent(string nameEvent, params string[] parameters)
        {
            if (firebaseInitialized)
                SendEventFirebase(nameEvent, parameters);
        }

        // Log a caught exception.
        public void LogCaughtException(Exception ex)
        {
            Crashlytics.LogException(ex);
        }

        void HandleLog(string logString, string stackTrace, LogType type)
        {
            // Check if the log type is an error or exception
            if (type == LogType.Exception || type == LogType.Error)
            {
                Exception exception = new Exception(logString + "\n" + stackTrace);
                // Call your methods when the application crashes
                LogCaughtException(exception);
            }
        }

        void OnDestroy()
        {
            // Unsubscribe from the application's crash event
            Application.logMessageReceived -= HandleLog;
        }
    }
}
