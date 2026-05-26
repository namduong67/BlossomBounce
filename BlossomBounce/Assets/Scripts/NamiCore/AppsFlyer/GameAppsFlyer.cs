using System.Collections;
using System.Collections.Generic;
using AppsFlyerSDK;
using UnityEngine;

namespace Nami.Controller
{
    public class GameAppsFlyer : MonoBehaviour
    {
        [Header("SDK key")]
        public string devKey;

        [Header("Android is package_name, IOS is appId")]
        public string appID;

        public bool isDebug;
        public bool getConversionData;

        // Start is called before the first frame update
        void Start()
        {
            // These fields are set from the editor so do not modify!
            //******************************//
            AppsFlyer.setIsDebug(isDebug);
            AppsFlyer.initSDK(devKey, appID, getConversionData ? this : null);
            //******************************/

            AppsFlyer.startSDK();
        }

        // Mark AppsFlyer CallBacks
        public void onConversionDataSuccess(string conversionData)
        {
            AppsFlyer.AFLog("didReceiveConversionData", conversionData);
            Dictionary<string, object> conversionDataDictionary = AppsFlyer.CallbackStringToDictionary(conversionData);
            // add deferred deeplink logic here
        }

        public void onConversionDataFail(string error)
        {
            AppsFlyer.AFLog("didReceiveConversionDataWithError", error);
        }

        public void onAppOpenAttribution(string attributionData)
        {
            AppsFlyer.AFLog("onAppOpenAttribution", attributionData);
            Dictionary<string, object> attributionDataDictionary = AppsFlyer.CallbackStringToDictionary(attributionData);
            // add direct deeplink logic here
        }

        public void onAppOpenAttributionFailure(string error)
        {
            AppsFlyer.AFLog("onAppOpenAttributionFailure", error);
        }
    }
}