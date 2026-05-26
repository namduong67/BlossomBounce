using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using GoogleMobileAds.Ump.Api;
using Nami;

namespace Bacon
{
    public class UMP : MonoBehaviour
    {
        protected string logPrefix = "UMP ";
        public bool HasUnknownError = false;
        public bool CanRequestAds;
        [Header("DEBUG")]
        [SerializeField]
        private DebugGeography debugGeography = DebugGeography.Disabled;
        [SerializeField, Tooltip("https://developers.google.com/admob/unity/test-ads")]
        private List<string> testDeviceIds;
        [SerializeField]
        private Button buttonReset;

        public static UMP Instance = null;

        public bool Initialize => _initialize;
        private bool _initialize = false;

        private void Awake()
        {
            if(Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (buttonReset)
            {
                buttonReset.onClick.AddListener(ResetConsentInformation);
            }
        }

        private void Start()
        {
            CanRequestAds = ConsentInformation.CanRequestAds();
        }

        protected IEnumerator OnCanRequestAd;
        protected bool isChecking = false;
        public IEnumerator DOGatherConsent(IEnumerator onCanRequestAd, Action<bool> consentSuccessAction = null)
        {
            if (Instance == null)
                throw new Exception(logPrefix + "AdmobConsentController NULL");

            if (isChecking)
            {
                Debug.LogError(logPrefix + ConsentInformation.ConsentStatus.ToString().ToUpper() + " CHECKING");
                yield break;
            }

            OnCanRequestAd = onCanRequestAd;
            isChecking = true;

            if (OnCanRequestAd != null)
            {
                ResetConsentInformation();
                ////FirebaseLogger.RegularEvent("ump_check_again");
            }
            else
            {
                ////FirebaseLogger.RegularEvent("ump_check");
            }

            var requestParameters = new ConsentRequestParameters();
            if (Instance.debugGeography != DebugGeography.Disabled)
            {
                requestParameters = new ConsentRequestParameters
                {
                    TagForUnderAgeOfConsent = false,
                    ConsentDebugSettings = new ConsentDebugSettings
                    {
                        DebugGeography = Instance.debugGeography,
                        TestDeviceHashedIds = Instance.testDeviceIds,
                    }
                };
            }


            Debug.Log(logPrefix + ConsentInformation.ConsentStatus.ToString().ToUpper() + " --> UPDATE");

            ConsentInformation.Update(requestParameters, (FormError error) =>
            {
                if (error != null)
                {
                    Debug.LogError(logPrefix + ConsentInformation.ConsentStatus.ToString().ToUpper() + " --> " + error.Message);
                    ////FirebaseLogger.RegularEvent("ump_update_error_" + ConsentInformation.ConsentStatus.ToString());
                    isChecking = false;
                    HasUnknownError = true;

                    return;
                }

                if (CanRequestAds) // Determine the consent-related action to take based on the ConsentStatus.
                {
                    // Consent has already been gathered or not required.
                    // Return control back to the user.
                    Debug.Log(logPrefix + "Update " + ConsentInformation.ConsentStatus.ToString().ToUpper() + " -- Consent has already been gathered or not required");
                    ////FirebaseLogger.RegularEvent("ump_update_success_" + ConsentInformation.ConsentStatus.ToString());
                    isChecking = false;

                    if (onCanRequestAd != null)
                        UnityMainThreadDispatcher.Enqueue(OnCanRequestAd);
                    return;
                }

                // Consent not obtained and is required.
                // Load the initial consent request form for the user.
                Debug.Log(logPrefix + ConsentInformation.ConsentStatus.ToString().ToUpper() + " --> LOAD AND SHOW ConsentForm If Required");
                ////FirebaseLogger.RegularEvent("ump_loadshow");
                ConsentForm.LoadAndShowConsentFormIfRequired((FormError error) =>
                {
                    if (error != null) // Form load failed.
                    {
                        Debug.LogError(logPrefix + ConsentInformation.ConsentStatus.ToString().ToUpper() + " --> " + error.Message);
                        ////FirebaseLogger.RegularEvent("ump_loadshow_error");
                        HasUnknownError = true;
                    }
                    else  // Form showing succeeded.
                    {
                        Debug.Log(logPrefix + ConsentInformation.ConsentStatus.ToString().ToUpper() + " --> LOAD AND SHOW SUCCESS");
                        ////FirebaseLogger.RegularEvent("ump_loadshow_success");
                    }
                    CanRequestAds = ConsentInformation.CanRequestAds();
                    isChecking = false;
                    consentSuccessAction?.Invoke(CanRequestAds);
                });
            });

            Debug.Log(logPrefix + ConsentInformation.ConsentStatus.ToString().ToUpper() + " --> WAIT!");
            while (isChecking && (ConsentInformation.ConsentStatus == ConsentStatus.Required || ConsentInformation.ConsentStatus == ConsentStatus.Unknown))
                yield return new WaitForEndOfFrame();

            ////FirebaseLogger.RegularEvent("ump_status_" + ConsentInformation.ConsentStatus.ToString());
            Debug.Log(logPrefix + ConsentInformation.ConsentStatus.ToString().ToUpper());
            if (CanRequestAds && OnCanRequestAd != null)
            {
                yield return OnCanRequestAd;
            }

            _initialize = true;
        }


        public void ShowPrivacyOptionsForm(Button privacyButton, Action<string> onComplete)
        {
            Debug.Log(logPrefix + "Showing privacy options form...");
            //FirebaseLogger.RegularEvent("ump_option_show");
            ConsentForm.ShowPrivacyOptionsForm((FormError error) =>
            {
                if (error != null)
                {
                    Debug.LogError(logPrefix + "Showing privacy options form - ERROR " + error.Message);
                    onComplete?.Invoke(error.Message);
                    //FirebaseLogger.RegularEvent("ump_option_show_error");
                }
                else  // Form showing succeeded.
                {
                    if (privacyButton)
                        privacyButton.interactable = ConsentInformation.PrivacyOptionsRequirementStatus == PrivacyOptionsRequirementStatus.Required;
                    Debug.Log(logPrefix + "Showing privacy options form - SUCCESS");
                    onComplete?.Invoke(null);
                    //FirebaseLogger.RegularEvent("ump_option_show_success");
                }
            });
        }


        public void ResetConsentInformation()
        {
            //FirebaseLogger.RegularEvent("ump_reset");
            ConsentInformation.Reset();
        }
    }
}