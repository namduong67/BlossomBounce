using System;
using System.Collections;
using System.Collections.Generic;
using Bacon;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine;

namespace Nami.Controller
{
    public class GameAds : MonoBehaviour
    {
        private static GameAds ads;
        public static GameAds Get => ads;

        private bool _initialize = false;
        public bool Initialize => _initialize;

        private bool _initializing = false;

        public bool _checkUMP = false;

        private float time_count_refesh = 0f;
        private float time_refresh = 1f;

        private bool isShowAds = false;

        public bool IsShowAds => isShowAds;

        // These ad units are configured to always serve test ads.
#if UNITY_ANDROID
        private string _adOpenId = "ca-app-pub-4119271324924135/1350591481";
        private string _adBannerId = "ca-app-pub-4119271324924135/1191196425";
        private string _adInterId = "ca-app-pub-4119271324924135/1206388734";
        private string _adRewardId = "ca-app-pub-4119271324924135/5289836495";

        private string _adInterOpenId = "ca-app-pub-3940256099942544/1033173712";
        private string _adNativeBannerId = "ca-app-pub-5904408074441373/8806767499";

#elif UNITY_IPHONE
        private string _adOpenId = "ca-app-pub-5904408074441373/2638619432";
        private string _adBannerId = "ca-app-pub-5904408074441373/2119243672";
        private string _adInterId = "ca-app-pub-5904408074441373/5136790267";
        private string _adRewardId = "ca-app-pub-5904408074441373/1213356117";

        private string _adInterOpenId = "ca-app-pub-5904408074441373/4777697613";
        private string _adNativeBannerId = "ca-app-pub-5904408074441373/8806767499";

#else
        private string _adOpenId = "unused";
#endif

        // Start is called before the first frame update
        private void Awake()
        {
            if(ads != null)
            {
                Destroy(gameObject);
                return;
            }
            ads = this;
            DontDestroyOnLoad(gameObject);
        }


        private void Start()
        {
            if (_checkUMP)
                RequestConsentUMP();
            else
                InitAd();
        }

        private void RequestConsentUMP()
        {
            StartCoroutine(UMP.Instance.DOGatherConsent(ConsentSuccess(), OnConsentCallback));
        }

        private IEnumerator ConsentSuccess()
        {
            Debug.Log("ConsentSuccess ");
            InitAd();
            yield return null;
        }

        private void OnConsentCallback(bool value)
        {
            Debug.Log("OnConsentCallback ");
            InitAd();
        }

        private void InitAd()
        {
            if (_initialize) return;
            if (_initializing) return;

            _initializing = true;
            // Initialize the Google Mobile Ads SDK.
            MobileAds.Initialize((InitializationStatus initStatus) =>
            {
                // This callback is called once the MobileAds SDK is initialized.
                Debug.Log("MobileAds.Initialized ");

                LoadAllAds();

                _initialize = true;

            });
        }

        public void LoadAllAds()
        {
            //LoadInterstitialAd();
            LoadAppOpenAd(() =>
            {
                ShowAppOpenAd();
            });
            LoadRewardAd();
            LoadBanner();
        }

        private void Update()
        {
            time_count_refesh += Time.deltaTime;
            if (time_count_refesh >= time_refresh)
            {
                time_count_refesh = 0f;
                CheckLoadInterAd();
            }
        }

        #region OpenAd

        public bool isSkipOpenAdAfterIntersAd;

        // ads Open property
        private AppOpenAd _appOpenAd;
        private DateTime _expireTimeAppOpen;
        private Action<bool> onAppOpenCallback;
        private bool _appOpenLoaded = false;
        private bool _justShowInterAd = false;

        public bool IsOpenAdAvailable
        {
            get
            {
                return _appOpenAd != null
                       && _appOpenAd.CanShowAd()
                       && DateTime.Now < _expireTimeAppOpen;
            }
        }

        public bool IsOpenAdLoaded => _appOpenLoaded;

        /// <summary>
        /// Loads the app open ad.
        /// </summary>
        private void LoadAppOpenAd(System.Action onLoadDone = null)
        {
            // Clean up the old ad before loading a new one.
            if (_appOpenAd != null)
            {
                _appOpenAd.Destroy();
                _appOpenAd = null;
            }

            Debug.Log("Loading the app open ad.");

            // Create our request used to load the ad.
            var adRequest = new AdRequest();

            // send the request to load the ad.
            AppOpenAd.Load(_adOpenId, adRequest,
                (AppOpenAd ad, LoadAdError error) =>
                {
              // if error is not null, the load request failed.
              if (error != null || ad == null)
                    {
                        Debug.LogWarning("app open ad failed to load an ad " +
                                       "with error : " + error);
                        _appOpenLoaded = true;
                        return;
                    }

                    Debug.Log("App open ad loaded with response : "
                              + ad.GetResponseInfo());

                    // App open ads can be preloaded for up to 4 hours.
                    _expireTimeAppOpen = DateTime.Now + TimeSpan.FromHours(4);

                    _appOpenAd = ad;
                    AppOpen_RegisterEventHandlers(ad);
                    _appOpenLoaded = true;

                    onLoadDone?.Invoke();
                });
        }

        /// <summary>
        /// Shows the app open ad.
        /// </summary>
        public void ShowAppOpenAd(Action<bool> callback = null)
        {
            onAppOpenCallback = callback;
            if (_appOpenAd != null && _appOpenAd.CanShowAd())
            {
                Debug.Log("Showing app open ad.");
                isShowAds = true;
                _appOpenAd.Show();
            }
            else
            {
                isShowAds = false;
                onAppOpenCallback?.Invoke(false);
            }
        }

        private void AppOpen_RegisterEventHandlers(AppOpenAd ad)
        {
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                isShowAds = false;
                Debug.Log("App open ad full screen content closed.");

                // Reload the ad so that we can show another as soon as possible.
                LoadAppOpenAd();
                onAppOpenCallback?.Invoke(true);
                ResetTimeInterAd();
            };

            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogWarning("App open ad failed to open full screen content " +
                                "with error : " + error);

                // Reload the ad so that we can show another as soon as possible.
                LoadAppOpenAd();
                onAppOpenCallback?.Invoke(false);
                ResetTimeInterAd();

            };
        }
        #endregion

        #region BannerAd
        BannerView _bannerView;

        /// <summary>
        /// Creates a adaptiveSize(landscape) banner view at bottom of the screen.
        /// </summary>
        private void CreateBannerView()
        {
            Debug.Log("Creating banner view ");

            // If we already have a banner, destroy the old one.
            if (_bannerView != null)
            {
                DestroyBannerView();
            }
            AdSize adaptiveSize =
                AdSize.GetLandscapeAnchoredAdaptiveBannerAdSizeWithWidth(Mathf.RoundToInt(AdSize.FullWidth));

            //AdSize adaptiveSize = AdSize.Banner;
            _bannerView = new BannerView(_adBannerId, adaptiveSize, AdPosition.Bottom);
            Banner_RegisterReloadHandler();

        }

        /// <summary>
        /// Destroys the banner view.
        /// </summary>
        private void DestroyBannerView()
        {
            if (_bannerView != null)
            {
                Debug.Log("Destroying banner view.");
                _bannerView.Destroy();
                _bannerView = null;
            }
        }

        /// <summary>
        /// Creates the banner view and loads a banner ad.
        /// </summary>
        private void LoadBanner()
        {
            //return;
            // Create an instance of a banner view first.
            if (_bannerView == null)
            {
                CreateBannerView();
            }

            // Create our request used to load the ad.
            var adRequest = new AdRequest();

            // Send the request to load the ad.
            Debug.Log("Loading banner ad.");
            _bannerView.LoadAd(adRequest);
        }

        private void Banner_RegisterReloadHandler()
        {
            // Raised when an ad is loaded into the banner view.
            _bannerView.OnBannerAdLoaded += () =>
            {
                Debug.Log("Banner view loaded an ad with response : "
                    + _bannerView.GetResponseInfo());
            };
            // Raised when an ad fails to load into the banner view.
            _bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
            {
                Debug.LogWarning("Banner view failed to load an ad with error : " + error);
            };
            // Raised when the ad is estimated to have earned money.
            _bannerView.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log(String.Format("Banner view paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));
            };
            // Raised when an impression is recorded for an ad.
            _bannerView.OnAdImpressionRecorded += () =>
            {
                Debug.Log("Banner view recorded an impression.");
            };
            // Raised when a click is recorded for an ad.
            _bannerView.OnAdClicked += () =>
            {
                Debug.Log("Banner view was clicked.");
                ResetTimeInterAd();
            };
            // Raised when an ad opened full screen content.
            _bannerView.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Banner view full screen content opened.");
            };
            // Raised when the ad closed full screen content.
            _bannerView.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Banner view full screen content closed.");
            };
        }

        /// <summary>
        /// Shows the ad.
        /// </summary>
        public void ShowBanner()
        {
            if (_bannerView != null)
            {
                Debug.Log("Showing banner view.");
                _bannerView.Show();
            }
            else
            {
                LoadBanner();
            }
        }

        /// <summary>
        /// Hides the ad.
        /// </summary>
        public void HideBanner()
        {
            if (_bannerView != null)
            {
                Debug.Log("Hiding banner view.");
                _bannerView.Hide();
            }
        }

        #endregion

        #region InterAd

        private InterstitialAd _interstitialAd;
        public float time_inter_ad = 60f;
        [SerializeField]
        private float time_reload_inter_ad = 15f;
        private float time_count_inter_ad = 0f;
        private float time_count_wait = 0f;
        private bool inter_ads_loading = false;
        private bool inter_ads_waiting_show = false;

        /// <summary>
        /// Loads the interstitial ad.
        /// </summary>
        private void LoadInterstitialAd()
        {
            // Clean up the old ad before loading a new one.
            if (_interstitialAd != null)
            {
                _interstitialAd.Destroy();
                _interstitialAd = null;
            }
            inter_ads_loading = true;
            Debug.Log("Loading the interstitial ad.");
            //return;
            // create our request used to load the ad.
            var adRequest = new AdRequest();

            // send the request to load the ad.
            InterstitialAd.Load(_adInterId, adRequest,
                (InterstitialAd ad, LoadAdError error) =>
                {
              // if error is not null, the load request failed.
              if (error != null || ad == null)
                    {
                        Debug.LogWarning("interstitial ad failed to load an ad " +
                                       "with error : " + error);
                        inter_ads_loading = false;
                        return;
                    }

                    Debug.Log("Interstitial ad loaded with response : "
                              + ad.GetResponseInfo());

                    _interstitialAd = ad;
                    inter_ads_waiting_show = true;
                    Inter_RegisterReloadHandler(ad);
                    inter_ads_loading = false;
                });
        }

        /// <summary>
        /// Shows the interstitial ad.
        /// </summary>
        public void ShowInterstitialAd()
        {
            //#if UNITY_EDITOR
            //            return;
            //#endif
            if (ConditionShowInterAd() == false) return;

            if (_interstitialAd != null && _interstitialAd.CanShowAd())
            {
                Debug.Log("Showing interstitial ad.");
                isShowAds = true;
                _justShowInterAd = true;
                _interstitialAd.Show();
            }
            else
            {
                isShowAds = false;
                Debug.LogWarning("Interstitial ad is not ready yet.");
            }
        }

        private void Inter_RegisterReloadHandler(InterstitialAd interstitialAd)
        {
            // Raised when the ad closed full screen content.
            interstitialAd.OnAdFullScreenContentClosed += () =>
            {
                isShowAds = false;
                Debug.Log("Interstitial Ad full screen content closed.");

                // Reload the ad so that we can show another as soon as possible.
                //LoadInterstitialAd();
                //GameFirebase.SendEvent("inter_ads");
                ResetTimeInterAd();
                inter_ads_waiting_show = false;
            };
            // Raised when the ad failed to open full screen content.
            interstitialAd.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogWarning("Interstitial ad failed to open full screen content " +
                               "with error : " + error);

                // Reload the ad so that we can show another as soon as possible.
                //LoadInterstitialAd();
                //GameFirebase.SendEvent("inter_ads");
                ResetTimeInterAd();
                inter_ads_waiting_show = false;

            };
        }

        private bool ConditionShowInterAd()
        {
            var time_check = Time.realtimeSinceStartup - (time_count_inter_ad - time_count_wait);

            return (time_check >= time_inter_ad);
        }

        public void StartCountInterAd(bool is_first_time)
        {
            if (is_first_time)
            {
                time_count_wait = 0;
                time_count_inter_ad = Time.realtimeSinceStartup;
            }
            else
            {
                time_count_wait += Time.realtimeSinceStartup - time_count_inter_ad;
                time_count_inter_ad = Time.realtimeSinceStartup;
            }
            //start_count_inter_ads = start_count;
        }

        private void ResetTimeInterAd()
        {
            //time_count_inter_ad = 0;
            time_count_inter_ad = Time.realtimeSinceStartup;
            time_count_wait = 0;
        }

        private void CheckLoadInterAd()
        {

            var time_check = Time.realtimeSinceStartup - (time_count_inter_ad - time_count_wait);
            if (time_check >= (time_inter_ad - time_reload_inter_ad))
            {
                if (inter_ads_loading == false && inter_ads_waiting_show == false)
                    LoadInterstitialAd();
            }

        }
        #endregion

        #region Inter_OpenAd
        private InterstitialAd _interstitialOpenAd;
        private bool _interstitialOpenLoaded = false;

        public bool IsInterstitialOpenLoaded => _interstitialOpenLoaded;
        public bool IsInterstitialOpenAvaiable
        {
            get
            {
                return _interstitialOpenAd != null
                       && _interstitialOpenAd.CanShowAd();
            }
        }

        private void LoadInterstitialOpenAd()
        {
            // Clean up the old ad before loading a new one.
            if (_interstitialOpenAd != null)
            {
                _interstitialOpenAd.Destroy();
                _interstitialOpenAd = null;
            }

            Debug.Log("Loading the interstitial open ad.");

            // create our request used to load the ad.
            var adRequest = new AdRequest();

            // send the request to load the ad.
            InterstitialAd.Load(_adInterOpenId, adRequest,
                (InterstitialAd ad, LoadAdError error) =>
                {
                    // if error is not null, the load request failed.
                    if (error != null || ad == null)
                    {
                        Debug.LogWarning("interstitial ad failed to load an ad " +
                                       "with error : " + error);
                        _interstitialOpenLoaded = true;
                        return;
                    }

                    Debug.Log("Interstitial open ad loaded with response : "
                              + ad.GetResponseInfo());

                    _interstitialOpenAd = ad;

                    _interstitialOpenLoaded = true;
                });
        }

        public void ShowInterstitialOpenAd()
        {
            if (_interstitialOpenAd != null && _interstitialOpenAd.CanShowAd())
            {
                Debug.Log("Showing interstitial open ad.");
                _interstitialOpenAd.Show();
            }
            else
            {
                Debug.LogWarning("Interstitial ad is not ready yet.");
            }
        }

        #endregion

        #region NativeBannerAd
        //private int countLoadFailed = 0;
        //NativeAd _nativeBannerAd;
        //public System.Action<NativeAd> OnLoadNativeBannerAd;

        //public void ShowNativeBanner()
        //{
        //    if(_nativeBannerAd != null)
        //    {
        //        _nativeBannerAd.Destroy();
        //        _nativeBannerAd = null;
        //    }

        //    LoadNativeBanner();
        //}

        //private void LoadNativeBanner()
        //{
        //    AdLoader adLoader = new AdLoader.Builder(_adNativeBannerId)
        //                .ForNativeAd()
        //                .Build();
        //    adLoader.OnNativeAdLoaded += this.HandleNativeAdLoaded;
        //    adLoader.OnAdFailedToLoad += this.HandleNativeAdFailedToLoad;
        //    adLoader.OnNativeAdClicked += HandleNativeAdClick;
        //    adLoader.OnNativeAdOpening += AdLoader_OnNativeAdOpening;
        //    adLoader.OnNativeAdImpression += AdLoader_OnNativeAdImpression;
        //    adLoader.LoadAd(new AdRequest());
        //    Debug.Log("LoadNativeBanner ");
        //}

        //private void AdLoader_OnNativeAdImpression(object sender, EventArgs e)
        //{
        //    Debug.Log("AdLoader_OnNativeAdImpression");

        //}

        //private void AdLoader_OnNativeAdOpening(object sender, EventArgs e)
        //{
        //    Debug.Log("AdLoader_OnNativeAdOpening");

        //}

        //private void HandleNativeAdLoaded(object sender, NativeAdEventArgs args)
        //{
        //    countLoadFailed = 0;
        //    Debug.Log("Native ad loaded.");
        //    this._nativeBannerAd = args.nativeAd;

        //    OnLoadNativeBannerAd?.Invoke(_nativeBannerAd);
        //}

        //private void HandleNativeAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
        //{
        //    if (args.LoadAdError != null)
        //        Debug.Log("Native ad failed to load: " + args.LoadAdError.GetMessage());

        //    countLoadFailed++;
        //    if(countLoadFailed <= 5)
        //    {
        //        //reload
        //        Invoke(nameof(LoadNativeBanner), 3f);
        //    }
        //}

        //private void HandleNativeAdClick(object sender, EventArgs args)
        //{
        //    ResetTimeInterAd();
        //}

        #endregion

        #region RewardAd

        private RewardedAd _rewardAd;
        private bool _rewardImpression = false;
        private System.Action<bool> _rewardAdComplete;

        /// <summary>
        /// Loads the interstitial ad.
        /// </summary>
        private void LoadRewardAd()
        {
            // Clean up the old ad before loading a new one.
            if (_rewardAd != null)
            {
                _rewardAd.Destroy();
                _rewardAd = null;
            }

            Debug.Log("Loading the interstitial ad.");
            //return;
            // create our request used to load the ad.
            var adRequest = new AdRequest();

            // send the request to load the ad.
            RewardedAd.Load(_adRewardId, adRequest,
                (RewardedAd ad, LoadAdError error) =>
                {
                    // if error is not null, the load request failed.
                    if (error != null || ad == null)
                    {
                        Debug.LogWarning("reward ad failed to load an ad " +
                                       "with error : " + error);
                        _rewardImpression = false;
                        _rewardAdComplete?.Invoke(_rewardImpression);
                        return;
                    }

                    Debug.Log("Interstitial ad loaded with response : "
                              + ad.GetResponseInfo());

                    _rewardAd = ad;

                    Reward_RegisterReloadHandler(ad);

                    if (_rewardAdComplete != null)
                    {
                        _justShowInterAd = true;
                        _rewardAd.Show((reward) =>
                        {
                            Debug.Log("Rewarded ad granted a reward");
                        });
                    }
                });
        }

        /// <summary>
        /// Shows the reward ad.
        /// </summary>
        public void ShowRewardAd(System.Action<bool> onComplete)
        {
            if (_rewardAd != null && _rewardAd.CanShowAd())
            {
                Debug.Log("Showing reward ad.");
                isShowAds = true;
                _justShowInterAd = true;
                _rewardImpression = false;
                _rewardAdComplete = onComplete;
                LoadRewardAd();
                _rewardAd.Show((reward)=>
                {
                    Debug.Log(string.Format("Rewarded ad granted a reward: {0} {1}",
                                            reward.Amount,
                                            reward.Type));
                });
            }
            else
            {
                isShowAds = false;
                _rewardAdComplete = null;
                _rewardImpression = false;
                onComplete?.Invoke(false);
                Debug.LogWarning("Reward ad is not ready yet.");
            }
        }

        public void LoadAndShowRewardAd(System.Action<bool> onComplete)
        {
            _rewardImpression = false;
            _rewardAdComplete = onComplete;

            LoadRewardAd();
        }

        private void Reward_RegisterReloadHandler(RewardedAd rewardAd)
        {
            // Raised when an impression is recorded for an ad.
            rewardAd.OnAdImpressionRecorded += () =>
            {
                _rewardImpression = true;
                Debug.Log("Rewarded ad recorded an impression.");
            };

            // Raised when the ad closed full screen content.
            rewardAd.OnAdFullScreenContentClosed += () =>
            {
                isShowAds = false;
                Debug.Log("Reward Ad full screen content closed.");

                _rewardAdComplete?.Invoke(_rewardImpression);
                // Reload the ad so that we can show another as soon as possible.
                //LoadRewardAd();
                //GameFirebase.SendEvent("reward_ads");
                ResetTimeInterAd();
            };
            // Raised when the ad failed to open full screen content.
            rewardAd.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogWarning("Reward ad failed to open full screen content " +
                               "with error : " + error);

                _rewardAdComplete?.Invoke(_rewardImpression);
                // Reload the ad so that we can show another as soon as possible.
                //LoadRewardAd();
                //GameFirebase.SendEvent("reward_ads");
                ResetTimeInterAd();

            };
        }


        #endregion

        private void OnApplicationPause(bool isPaused)
        {
            OnAppStateChanged(isPaused ? AppState.Background : AppState.Foreground);
        }

        private void OnAppStateChanged(AppState state)
        {
            //Debug.Log("App State changed to : " + state);
            // if the app is Foregrounded and the ad is available, show it.
            if (state == AppState.Foreground)
            { 
                if(isSkipOpenAdAfterIntersAd && _justShowInterAd) {
                    _justShowInterAd = false;
                    return;
                }

                if (IsOpenAdAvailable && Time.realtimeSinceStartup > 60f)
                {
                    ShowAppOpenAd();
                }
            }
        }

        private void OnDestroy()
        {
            // Always unlisten to events when complete.
            //AppStateEventNotifier.AppStateChanged -= OnAppStateChanged;
        }
    }
}
