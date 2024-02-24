/*namespace Nexx 
{
    using GoogleMobileAds.Api;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.Events;

    public class AdManager : MonoBehaviour
    {
        private static AdManager instance;


        private RewardedAd rewardedAd;
        private BannerView bannerAd;
        private InterstitialAd interstitialAd;


        private UnityAction onAdFail;
        private UnityAction onAdComplete;
        private UnityAction onAdLoad;

        private Canvas[] generatedCanvases;
        private AdRequest req;

//#if UNITY_EDITOR
        private string id_reward = "ca-app-pub-3940256099942544/5224354917";
        private string id_banner = "ca-app-pub-3940256099942544/6300978111";
        private string id_intersitial = "ca-app-pub-3940256099942544/1033173712";
*//*#else
        private string id_reward = "ca-app-pub-8888188277244583/5766824894";
        private string id_banner = "ca-app-pub-8888188277244583/7670006494";
        private string id_intersitial = "ca-app-pub-8888188277244583/8203524817";
#endif*//*

        public void Setup(ref UnityAction onSceneLoad) 
        {
            instance = this;

            MobileAds.Initialize(onComplete => GenerateAds());
            onSceneLoad += () => generatedCanvases = FindObjectsOfType<Canvas>(true);
        }


        private void GenerateAds()
        {
            req = new AdRequest.Builder().Build();

            rewardedAd = new RewardedAd(id_reward);
            rewardedAd.LoadAd(req);



            rewardedAd.OnAdFailedToLoad += (object sender, AdFailedToLoadEventArgs args) => { Debug.Log("Ad failed to load, details - \n" + args.ToString()); onAdFail?.Invoke(); ClearEvents(); };
            rewardedAd.OnAdFailedToShow += (object sender, AdErrorEventArgs args) => { Debug.Log("Ad failed to show, details - \n" + args.ToString()); onAdFail?.Invoke(); ClearEvents(); };

            rewardedAd.OnUserEarnedReward += (object sender, Reward reward) => { onAdComplete?.Invoke(); ClearEvents(); };
            rewardedAd.OnAdClosed += (object sender, System.EventArgs args) => { Debug.Log("Ad closed before reward?"); onAdComplete?.Invoke(); ClearEvents(); };




            bannerAd = new BannerView(id_banner, AdSize.Banner, AdPosition.Bottom);
            bannerAd.LoadAd(req);
            bannerAd.Hide();


            bannerAd.OnAdFailedToLoad += (object sender, AdFailedToLoadEventArgs args) => { Debug.Log("Ad failed to load, details - \n" + args.ToString()); ClearEvents(); };




            interstitialAd = new InterstitialAd(id_intersitial);
            interstitialAd.LoadAd(req);

            interstitialAd.OnAdLoaded += (a, b) => onAdLoad?.Invoke();

            interstitialAd.OnAdClosed += (a, b) => { onAdComplete?.Invoke(); ClearEvents(); };
            interstitialAd.OnPaidEvent += (a, b) => { onAdComplete?.Invoke(); ClearEvents(); };
            interstitialAd.OnAdFailedToLoad += (object sender, AdFailedToLoadEventArgs args) => { Debug.Log("Ad failed to load, details - \n" + args.ToString()); onAdComplete?.Invoke(); ClearEvents(); };
            interstitialAd.OnAdFailedToShow += (object sender, AdErrorEventArgs args) => { Debug.Log("Ad failed to show, details - \n" + args.ToString()); onAdComplete?.Invoke(); ClearEvents(); };
        }


        private void ClearEvents()
        {
            onAdFail = null;
            onAdComplete = null;
        }


        private List<Canvas> TryCaptureAdCanvs()
        {
            List<Canvas> found = new List<Canvas>() ;
            Canvas[] active = FindObjectsOfType<Canvas>();

            foreach (Canvas c in active)
               //if (c.sortingOrder == 0 && c.GetComponent<ButtonBehaviour>() && !generatedCanvases.Contains(c)) found.Add(c);
               if (c.sortingOrder == 0 && !generatedCanvases.Contains(c)) found.Add(c);

            return found;
        }

        private void UpdateAdCanvasSortOrder()
        {
            List<Canvas> search = TryCaptureAdCanvs();
            foreach (Canvas c in search) c.sortingOrder = 9999;
        }


        public static void LoadRewardedAd(UnityAction onAdFail, UnityAction onAdComplete)
        {
            if (instance)
                instance.Internal_LoadRewardedAd(onAdFail, onAdComplete);
            else
                onAdFail?.Invoke();
        }

        private void Internal_LoadRewardedAd(UnityAction onAdFail, UnityAction onAdComplete)
        {
            ClearEvents();

            this.onAdFail = onAdFail;
            this.onAdComplete = onAdComplete;

            rewardedAd.LoadAd(req);
            rewardedAd.Show();
            UpdateAdCanvasSortOrder();
        }




        public static void LoadBannerAd()
        {
            if (instance) instance.Internal_LoadBannerAd();
        }

        private void Internal_LoadBannerAd()
        {
            ClearEvents();
            bannerAd.Hide();
            bannerAd.LoadAd(req);
            bannerAd.Show();
            UpdateAdCanvasSortOrder();
        }

        public static void HideBannerAd()
        {
            if (instance) instance.Internal_HideBannerAd();
        }

        private void Internal_HideBannerAd()
        {
            ClearEvents();
            bannerAd.Hide();
            Debug.Log("test");
        }





        public static void LoadInterstitialAd(UnityAction onAnything, UnityAction onLoad)
        {
            if (instance) instance.Internal_LoadInterstitialAd(onAnything, onLoad);
        }
        private void Internal_LoadInterstitialAd(UnityAction onAnything, UnityAction onLoad)
        {
            ClearEvents();
            onAdComplete = onAnything;
            onAdLoad += onLoad;

            interstitialAd.LoadAd(req);
            interstitialAd.Show();

            UpdateAdCanvasSortOrder();
        }
    }
}*/