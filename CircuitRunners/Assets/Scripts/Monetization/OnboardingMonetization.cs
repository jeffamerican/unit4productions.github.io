using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CircuitRunners.Monetization;

namespace CircuitRunners.Monetization
{
    /// <summary>
    /// Optimized onboarding monetization system designed to maximize Day 1 conversion
    /// Implements strategic showcase moments, urgency mechanics, and value demonstration
    /// Target: Convert 5% of tutorial completions to first purchase within 10 minutes
    /// </summary>
    public class OnboardingMonetization : MonoBehaviour
    {
        #region Configuration
        [Header("Founder's Edition Settings")]
        [SerializeField] private float founderPackOriginalPrice = 19.99f;
        [SerializeField] private float founderPackLaunchPrice = 9.99f;
        [SerializeField] private int founderPackTimeLimit = 48; // hours
        [SerializeField] private bool enableFounderUrgency = true;
        
        [Header("Premium Trial Settings")]
        [SerializeField] private int premiumTrialDuration = 24; // hours
        [SerializeField] private bool enablePremiumShowcase = true;
        [SerializeField] private List<string> showcaseBotIds = new List<string>();
        
        [Header("Tutorial Monetization Points")]
        [SerializeField] private List<TutorialMonetizationPoint> monetizationPoints;
        
        [Header("Conversion Optimization")]
        [SerializeField] private float showcaseDelaySeconds = 2f;
        [SerializeField] private int maxOffersPerSession = 3;
        [SerializeField] private bool enableSmartTiming = true;
        #endregion

        #region Private Fields
        private MonetizationManager _monetizationManager;
        private bool _hasShownFounderOffer = false;
        private bool _hasActivatedTrial = false;
        private int _offersShownThisSession = 0;
        private DateTime _onboardingStartTime;
        private List<string> _completedShowcases = new List<string>();
        #endregion

        #region Events
        public static event Action<string> OnShowcaseCompleted;
        public static event Action<bool> OnFounderOfferShown;
        public static event Action<bool> OnPremiumTrialActivated;
        public static event Action<ConversionMetrics> OnConversionDataUpdated;
        #endregion

        #region Data Structures
        [System.Serializable]
        public class TutorialMonetizationPoint
        {
            public string pointId;
            public string pointName;
            public TutorialPhase phase;
            public MonetizationTactic tactic;
            public float delaySeconds;
            public List<string> showcaseContent;
            public bool requiresCompletion;
        }

        public enum TutorialPhase
        {
            FirstBotBuild = 0,
            FirstRun = 1,
            PartUpgrade = 2,
            CompetitiveIntro = 3,
            CustomizationUnlock = 4,
            TutorialComplete = 5
        }

        public enum MonetizationTactic
        {
            PremiumShowcase,
            FounderOffer,
            TrialActivation,
            ValueDemonstration,
            UrgencyCreation,
            SocialProof
        }

        [System.Serializable]
        public struct ConversionMetrics
        {
            public int tutorialCompletions;
            public int showcasesViewed;
            public int offersShown;
            public int conversions;
            public float conversionRate;
            public float averageTimeToConversion;
            public Dictionary<string, int> conversionsByTactic;
        }
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            _monetizationManager = FindObjectOfType<MonetizationManager>();
            _onboardingStartTime = DateTime.Now;
            
            // Set up default monetization points if none configured
            if (monetizationPoints == null || monetizationPoints.Count == 0)
            {
                SetupDefaultMonetizationPoints();
            }
            
            // Initialize showcase content
            InitializeShowcaseContent();
        }
        #endregion

        #region Public Interface
        /// <summary>
        /// Trigger monetization point during tutorial
        /// </summary>
        public void TriggerMonetizationPoint(TutorialPhase phase)
        {
            if (_offersShownThisSession >= maxOffersPerSession)
            {
                Debug.Log("[OnboardingMonetization] Max offers reached for this session");
                return;
            }

            var point = monetizationPoints.Find(p => p.phase == phase);
            if (point == null) return;

            StartCoroutine(ExecuteMonetizationPoint(point));
        }

        /// <summary>
        /// Show Founder's Edition offer with urgency mechanics
        /// </summary>
        public void ShowFounderEditionOffer()
        {
            if (_hasShownFounderOffer || !enableFounderUrgency)
                return;

            StartCoroutine(ShowFounderOfferSequence());
        }

        /// <summary>
        /// Activate premium trial with full feature access
        /// </summary>
        public void ActivatePremiumTrial()
        {
            if (_hasActivatedTrial)
                return;

            StartCoroutine(ActivateTrialSequence());
        }

        /// <summary>
        /// Check if player is eligible for premium showcase
        /// </summary>
        public bool IsEligibleForPremiumShowcase()
        {
            // Don't show to players who already purchased
            if (_monetizationManager.HasMadePurchase)
                return false;

            // Don't overwhelm with offers
            if (_offersShownThisSession >= maxOffersPerSession)
                return false;

            // Check timing if smart timing enabled
            if (enableSmartTiming)
            {
                var sessionTime = (DateTime.Now - _onboardingStartTime).TotalMinutes;
                return sessionTime >= 3 && sessionTime <= 15; // Sweet spot for conversion
            }

            return true;
        }
        #endregion

        #region Core Implementation
        /// <summary>
        /// Execute monetization point with optimized timing and presentation
        /// </summary>
        private IEnumerator ExecuteMonetizationPoint(TutorialMonetizationPoint point)
        {
            Debug.Log($"[OnboardingMonetization] Executing point: {point.pointName}");

            // Wait for optimal timing
            yield return new WaitForSeconds(point.delaySeconds);

            switch (point.tactic)
            {
                case MonetizationTactic.PremiumShowcase:
                    yield return StartCoroutine(ShowPremiumBotCapabilities(point));
                    break;
                
                case MonetizationTactic.FounderOffer:
                    yield return StartCoroutine(ShowFounderOfferSequence());
                    break;
                
                case MonetizationTactic.TrialActivation:
                    yield return StartCoroutine(ActivateTrialSequence());
                    break;
                
                case MonetizationTactic.ValueDemonstration:
                    yield return StartCoroutine(DemonstrateValueProposition(point));
                    break;
                
                case MonetizationTactic.UrgencyCreation:
                    yield return StartCoroutine(CreateUrgencyMoment(point));
                    break;
                
                case MonetizationTactic.SocialProof:
                    yield return StartCoroutine(ShowSocialProof(point));
                    break;
            }

            _offersShownThisSession++;
            OnShowcaseCompleted?.Invoke(point.pointId);
        }

        /// <summary>
        /// Showcase premium bot capabilities with direct comparison
        /// </summary>
        private IEnumerator ShowPremiumBotCapabilities(TutorialMonetizationPoint point)
        {
            if (!IsEligibleForPremiumShowcase())
                yield break;

            Debug.Log("[OnboardingMonetization] Showcasing premium bot capabilities");

            // Show comparison: Standard vs Premium bot performance
            var showcaseUI = GetShowcaseUI();
            if (showcaseUI != null)
            {
                // Display side-by-side comparison
                showcaseUI.ShowBotComparison(
                    standardBotId: "tutorial_bot",
                    premiumBotId: showcaseBotIds.Count > 0 ? showcaseBotIds[0] : "premium_speedster",
                    comparisonMetrics: new string[] { "Speed", "Handling", "Special Abilities" }
                );

                // Highlight premium advantages
                yield return new WaitForSeconds(3f);
                showcaseUI.HighlightPremiumAdvantages();

                // Show "Try Now" option
                yield return new WaitForSeconds(2f);
                showcaseUI.ShowTrialOption(premiumTrialDuration);
            }

            // Track showcase completion
            _completedShowcases.Add(point.pointId);
            TrackShowcaseMetrics(point.pointId, "premium_comparison");
        }

        /// <summary>
        /// Show Founder's Edition offer with time-limited pricing
        /// </summary>
        private IEnumerator ShowFounderOfferSequence()
        {
            if (_hasShownFounderOffer)
                yield break;

            _hasShownFounderOffer = true;
            Debug.Log("[OnboardingMonetization] Showing Founder's Edition offer");

            var founderUI = GetFounderOfferUI();
            if (founderUI != null)
            {
                // Calculate remaining time for offer
                var timeRemaining = TimeSpan.FromHours(founderPackTimeLimit);
                
                // Show offer with urgency elements
                founderUI.ShowFounderOffer(
                    originalPrice: founderPackOriginalPrice,
                    salePrice: founderPackLaunchPrice,
                    timeRemaining: timeRemaining,
                    benefits: GetFounderBenefits()
                );

                // Add countdown timer for urgency
                founderUI.StartCountdownTimer(timeRemaining);
                
                // Track offer shown
                OnFounderOfferShown?.Invoke(true);
                TrackConversionFunnel("founder_offer_shown");
            }

            yield return null;
        }

        /// <summary>
        /// Activate premium trial with full feature unlock
        /// </summary>
        private IEnumerator ActivateTrialSequence()
        {
            if (_hasActivatedTrial)
                yield break;

            _hasActivatedTrial = true;
            Debug.Log("[OnboardingMonetization] Activating premium trial");

            // Grant trial access
            var trialEndTime = DateTime.Now.AddHours(premiumTrialDuration);
            PlayerPrefs.SetString("PremiumTrialEnd", trialEndTime.ToBinary().ToString());
            PlayerPrefs.SetInt("HasActiveTrial", 1);
            PlayerPrefs.Save();

            // Show trial activation UI
            var trialUI = GetTrialActivationUI();
            if (trialUI != null)
            {
                trialUI.ShowTrialActivated(
                    duration: premiumTrialDuration,
                    unlockedFeatures: GetTrialFeatures()
                );

                // Show what becomes available
                yield return new WaitForSeconds(2f);
                trialUI.HighlightUnlockedContent();
            }

            // Grant trial rewards
            yield return StartCoroutine(GrantTrialBenefits());

            OnPremiumTrialActivated?.Invoke(true);
            TrackConversionFunnel("premium_trial_activated");
        }

        /// <summary>
        /// Demonstrate clear value proposition through gameplay
        /// </summary>
        private IEnumerator DemonstrateValueProposition(TutorialMonetizationPoint point)
        {
            Debug.Log("[OnboardingMonetization] Demonstrating value proposition");

            var valueUI = GetValueDemoUI();
            if (valueUI != null)
            {
                // Show progression comparison
                valueUI.ShowProgressionComparison(
                    freePlayerProgress: "3 hours to unlock next tier",
                    premiumPlayerProgress: "Instant unlock + bonus rewards"
                );

                yield return new WaitForSeconds(3f);

                // Highlight time savings
                valueUI.HighlightTimeSavings(
                    timeWithoutPremium: "15+ hours to build competitive bot",
                    timeWithPremium: "2 hours with premium parts access"
                );

                yield return new WaitForSeconds(2f);

                // Show exclusive content preview
                valueUI.PreviewExclusiveContent(showcaseBotIds);
            }

            TrackShowcaseMetrics(point.pointId, "value_demonstration");
            yield return null;
        }

        /// <summary>
        /// Create urgency moment with limited-time offers
        /// </summary>
        private IEnumerator CreateUrgencyMoment(TutorialMonetizationPoint point)
        {
            Debug.Log("[OnboardingMonetization] Creating urgency moment");

            var urgencyUI = GetUrgencyUI();
            if (urgencyUI != null)
            {
                // Show limited-time newcomer bonus
                urgencyUI.ShowNewcomerBonus(
                    bonusDescription: "50% Extra Premium Currency + Exclusive Bot",
                    timeLimit: TimeSpan.FromMinutes(10),
                    originalValue: "$19.99",
                    discountedValue: "$9.99"
                );

                // Start countdown
                urgencyUI.StartCountdown(TimeSpan.FromMinutes(10));
            }

            TrackConversionFunnel("urgency_moment_created");
            yield return null;
        }

        /// <summary>
        /// Show social proof to increase conversion likelihood
        /// </summary>
        private IEnumerator ShowSocialProof(TutorialMonetizationPoint point)
        {
            Debug.Log("[OnboardingMonetization] Showing social proof");

            var socialUI = GetSocialProofUI();
            if (socialUI != null)
            {
                // Show purchase testimonials
                socialUI.ShowRecentPurchases(
                    purchases: new string[]
                    {
                        "Alex just unlocked the Speed Demon bot!",
                        "Sarah purchased the Founder's Pack - 2 minutes ago",
                        "Mike upgraded to Premium - \"Best decision ever!\""
                    }
                );

                yield return new WaitForSeconds(3f);

                // Show competitive rankings
                socialUI.ShowTopPlayerBots(
                    topBots: GetTopCompetitiveBots()
                );
            }

            TrackShowcaseMetrics(point.pointId, "social_proof");
            yield return null;
        }
        #endregion

        #region Configuration Setup
        /// <summary>
        /// Set up default monetization points for optimal conversion
        /// </summary>
        private void SetupDefaultMonetizationPoints()
        {
            monetizationPoints = new List<TutorialMonetizationPoint>
            {
                new TutorialMonetizationPoint
                {
                    pointId = "first_build_showcase",
                    pointName = "Premium Bot Showcase",
                    phase = TutorialPhase.FirstBotBuild,
                    tactic = MonetizationTactic.PremiumShowcase,
                    delaySeconds = 2f,
                    showcaseContent = new List<string> { "premium_speedster", "premium_tank" },
                    requiresCompletion = false
                },
                new TutorialMonetizationPoint
                {
                    pointId = "first_run_trial",
                    pointName = "Premium Trial Activation",
                    phase = TutorialPhase.FirstRun,
                    tactic = MonetizationTactic.TrialActivation,
                    delaySeconds = 1f,
                    showcaseContent = new List<string>(),
                    requiresCompletion = true
                },
                new TutorialMonetizationPoint
                {
                    pointId = "upgrade_value_demo",
                    pointName = "Value Proposition Demo",
                    phase = TutorialPhase.PartUpgrade,
                    tactic = MonetizationTactic.ValueDemonstration,
                    delaySeconds = 3f,
                    showcaseContent = new List<string> { "premium_parts_comparison" },
                    requiresCompletion = false
                },
                new TutorialMonetizationPoint
                {
                    pointId = "competitive_social_proof",
                    pointName = "Competitive Social Proof",
                    phase = TutorialPhase.CompetitiveIntro,
                    tactic = MonetizationTactic.SocialProof,
                    delaySeconds = 2f,
                    showcaseContent = new List<string> { "top_player_bots" },
                    requiresCompletion = false
                },
                new TutorialMonetizationPoint
                {
                    pointId = "completion_founder_offer",
                    pointName = "Founder's Edition Offer",
                    phase = TutorialPhase.TutorialComplete,
                    tactic = MonetizationTactic.FounderOffer,
                    delaySeconds = 5f,
                    showcaseContent = new List<string>(),
                    requiresCompletion = true
                }
            };

            Debug.Log("[OnboardingMonetization] Default monetization points configured");
        }

        /// <summary>
        /// Initialize showcase content and premium bot demonstrations
        /// </summary>
        private void InitializeShowcaseContent()
        {
            if (showcaseBotIds.Count == 0)
            {
                showcaseBotIds = new List<string>
                {
                    "premium_speedster",
                    "premium_tank",
                    "premium_stealth",
                    "founder_exclusive_bot"
                };
            }

            Debug.Log($"[OnboardingMonetization] Initialized {showcaseBotIds.Count} showcase bots");
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Get Founder's Edition benefits for display
        /// </summary>
        private List<string> GetFounderBenefits()
        {
            return new List<string>
            {
                "Exclusive Founder's Bot (Cannot be obtained later)",
                "5,000 Premium Currency ($25 value)",
                "Lifetime 20% discount on all purchases",
                "Early access to new content",
                "Special Founder badge and title",
                "Premium battle pass (First 3 months free)"
            };
        }

        /// <summary>
        /// Get trial feature list for display
        /// </summary>
        private List<string> GetTrialFeatures()
        {
            return new List<string>
            {
                "Access to all premium bots",
                "Unlimited energy for 24 hours",
                "2x XP and currency gain",
                "Advanced customization options",
                "Priority matchmaking",
                "Exclusive premium-only circuits"
            };
        }

        /// <summary>
        /// Grant benefits during premium trial
        /// </summary>
        private IEnumerator GrantTrialBenefits()
        {
            var resourceManager = Core.GameManager.Instance?.Resources;
            if (resourceManager != null)
            {
                // Grant trial currency bonus
                resourceManager.AddPremiumCurrency(100);
                resourceManager.AddScrap(2000);
                
                // Refill energy
                resourceManager.RefillEnergy();
                
                Debug.Log("[OnboardingMonetization] Trial benefits granted");
            }
            
            yield return null;
        }

        /// <summary>
        /// Get top competitive bots for social proof
        /// </summary>
        private List<string> GetTopCompetitiveBots()
        {
            return new List<string>
            {
                "Lightning Strike (Premium)",
                "Armor Piercer (Premium)",
                "Shadow Runner (Premium)",
                "Titan Crusher (Founder Exclusive)"
            };
        }

        /// <summary>
        /// Track showcase metrics for optimization
        /// </summary>
        private void TrackShowcaseMetrics(string pointId, string showcaseType)
        {
            // Track with analytics manager
            if (_monetizationManager != null)
            {
                // Implementation would track showcase completion
                Debug.Log($"[OnboardingMonetization] Tracked showcase: {pointId} - {showcaseType}");
            }
        }

        /// <summary>
        /// Track conversion funnel progress
        /// </summary>
        private void TrackConversionFunnel(string step)
        {
            var sessionTime = (DateTime.Now - _onboardingStartTime).TotalMinutes;
            Debug.Log($"[OnboardingMonetization] Conversion step: {step} at {sessionTime:F1}m");
            
            // Implementation would send to analytics
        }

        // UI Component Getters (would be replaced with actual UI references)
        private OnboardingShowcaseUI GetShowcaseUI() => FindObjectOfType<OnboardingShowcaseUI>();
        private FounderOfferUI GetFounderOfferUI() => FindObjectOfType<FounderOfferUI>();
        private TrialActivationUI GetTrialActivationUI() => FindObjectOfType<TrialActivationUI>();
        private ValueDemoUI GetValueDemoUI() => FindObjectOfType<ValueDemoUI>();
        private UrgencyUI GetUrgencyUI() => FindObjectOfType<UrgencyUI>();
        private SocialProofUI GetSocialProofUI() => FindObjectOfType<SocialProofUI>();
        #endregion
    }

    #region UI Component Interfaces
    // These would be implemented as actual UI components
    public abstract class OnboardingShowcaseUI : MonoBehaviour
    {
        public abstract void ShowBotComparison(string standardBotId, string premiumBotId, string[] comparisonMetrics);
        public abstract void HighlightPremiumAdvantages();
        public abstract void ShowTrialOption(int duration);
    }

    public abstract class FounderOfferUI : MonoBehaviour
    {
        public abstract void ShowFounderOffer(float originalPrice, float salePrice, TimeSpan timeRemaining, List<string> benefits);
        public abstract void StartCountdownTimer(TimeSpan timeRemaining);
    }

    public abstract class TrialActivationUI : MonoBehaviour
    {
        public abstract void ShowTrialActivated(int duration, List<string> unlockedFeatures);
        public abstract void HighlightUnlockedContent();
    }

    public abstract class ValueDemoUI : MonoBehaviour
    {
        public abstract void ShowProgressionComparison(string freePlayerProgress, string premiumPlayerProgress);
        public abstract void HighlightTimeSavings(string timeWithoutPremium, string timeWithPremium);
        public abstract void PreviewExclusiveContent(List<string> exclusiveContent);
    }

    public abstract class UrgencyUI : MonoBehaviour
    {
        public abstract void ShowNewcomerBonus(string bonusDescription, TimeSpan timeLimit, string originalValue, string discountedValue);
        public abstract void StartCountdown(TimeSpan duration);
    }

    public abstract class SocialProofUI : MonoBehaviour
    {
        public abstract void ShowRecentPurchases(string[] purchases);
        public abstract void ShowTopPlayerBots(List<string> topBots);
    }
    #endregion
}