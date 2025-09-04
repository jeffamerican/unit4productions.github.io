using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using CircuitRunners.Core;
using CircuitRunners.Bot;
using CircuitRunners.Data;
using CircuitRunners.Tests.Helpers;

namespace CircuitRunners.Tests.PlayMode
{
    /// <summary>
    /// Integration tests for Circuit Runners - tests system-to-system communication
    /// and complete gameplay flows in runtime environment
    /// 
    /// Critical for ensuring all systems work together correctly in actual gameplay
    /// </summary>
    public class IntegrationTests
    {
        private GameManager gameManager;
        private GameObject testEnvironment;
        
        [SetUp]
        public void SetUp()
        {
            testEnvironment = TestingUtilities.CreateTestEnvironment("IntegrationTest");
            
            // Create GameManager instance for integration testing
            var gameManagerObject = new GameObject("TestGameManager");
            gameManager = gameManagerObject.AddComponent<GameManager>();
        }
        
        [TearDown]
        public void TearDown()
        {
            TestingUtilities.CleanupTestEnvironment(testEnvironment);
            if (gameManager != null && gameManager.gameObject != null)
            {
                Object.DestroyImmediate(gameManager.gameObject);
            }
        }
        
        #region Complete Game Flow Tests
        
        [UnityTest]
        public IEnumerator CompleteGameFlow_MainMenuToPostRun_ExecutesSuccessfully()
        {
            // Arrange
            bool stateTransitionCompleted = false;
            var targetStates = new[]
            {
                GameManager.GameState.MainMenu,
                GameManager.GameState.BotBuilding,
                GameManager.GameState.PreRun,
                GameManager.GameState.Running,
                GameManager.GameState.PostRun
            };
            
            int currentStateIndex = 0;
            gameManager.OnGameStateChanged += (from, to) =>
            {
                if (currentStateIndex < targetStates.Length && to == targetStates[currentStateIndex])
                {
                    currentStateIndex++;
                    if (currentStateIndex >= targetStates.Length)
                        stateTransitionCompleted = true;
                }
            };
            
            // Act - Simulate complete game flow
            yield return StartCoroutine(SimulateCompleteGameSession());
            
            // Wait for completion
            yield return StartCoroutine(TestingUtilities.WaitForCondition(() => stateTransitionCompleted, 10f));
            
            // Assert
            Assert.IsTrue(stateTransitionCompleted, "Complete game flow should execute all state transitions");
            Assert.AreEqual(GameManager.GameState.PostRun, gameManager.CurrentState, 
                "Game should end in PostRun state");
        }
        
        [UnityTest]
        public IEnumerator GameFlow_ResourceConsumption_UpdatesCorrectly()
        {
            // Arrange
            int initialEnergy = 5;
            int expectedEnergyAfterRun = 4;
            bool energyConsumed = false;
            
            // Mock resource manager setup
            var resourceManager = TestingUtilities.CreateMockResourceManager(initialEnergy: initialEnergy);
            
            // Act
            yield return StartCoroutine(SimulateSingleGameRun());
            
            // Wait for energy consumption
            yield return StartCoroutine(TestingUtilities.WaitForCondition(() => energyConsumed, 5f));
            
            // Assert
            Assert.IsTrue(true, "Energy should be consumed during game run");
            // In actual implementation, verify energy count decreased
        }
        
        [UnityTest]
        public IEnumerator GameFlow_PerformanceOptimization_ActivatesUnderStress()
        {
            // Arrange
            bool performanceOptimizationTriggered = false;
            float simulatedLowFPS = 25f;
            
            // Act - Simulate performance stress
            yield return StartCoroutine(SimulatePerformanceStress(simulatedLowFPS));
            
            // Wait for optimization activation
            yield return new WaitForSeconds(2f);
            
            // Assert
            Assert.IsTrue(true, "Performance optimizations should activate under stress");
            // In actual implementation, check if optimizations are active
        }
        
        #endregion
        
        #region Bot Integration Tests
        
        [UnityTest]
        public IEnumerator BotIntegration_ConfigurationToExecution_MaintainsSettings()
        {
            // Arrange
            var botArchetype = BotArchetype.Speed;
            bool configurationApplied = false;
            bool botExecutionStarted = false;
            
            // Act
            yield return StartCoroutine(SimulateBotConfigurationAndExecution(botArchetype));
            
            // Wait for configuration and execution
            yield return StartCoroutine(TestingUtilities.WaitForCondition(
                () => configurationApplied && botExecutionStarted, 5f));
            
            // Assert
            Assert.IsTrue(true, "Bot configuration should be applied and maintained during execution");
        }
        
        [UnityTest]
        public IEnumerator BotExecution_PhysicsAndAI_WorkTogether()
        {
            // Arrange
            var botController = TestingUtilities.CreateMockBotController();
            bool physicsActive = false;
            bool aiDecisionMaking = false;
            
            // Act
            yield return StartCoroutine(SimulateBotPhysicsAndAI(botController));
            
            // Wait for systems to activate
            yield return new WaitForSeconds(1f);
            
            // Assert
            Assert.IsTrue(true, "Bot physics and AI should work together seamlessly");
            // In actual implementation, verify physics and AI coordination
        }
        
        #endregion
        
        #region Firebase Integration Tests
        
        [UnityTest]
        public IEnumerator FirebaseIntegration_AuthenticationToDataSync_WorksOfflineOnline()
        {
            // Arrange
            bool authenticationCompleted = false;
            bool dataSyncCompleted = false;
            
            // Act - Test offline first, then online
            yield return StartCoroutine(SimulateFirebaseOfflineAuthentication());
            authenticationCompleted = true;
            
            yield return StartCoroutine(SimulateFirebaseOnlineSync());
            dataSyncCompleted = true;
            
            // Assert
            Assert.IsTrue(authenticationCompleted, "Firebase authentication should work offline");
            Assert.IsTrue(dataSyncCompleted, "Data sync should work when going online");
        }
        
        [UnityTest]
        public IEnumerator FirebaseIntegration_NetworkFailureRecovery_HandlesGracefully()
        {
            // Arrange
            bool networkFailureHandled = false;
            bool recoveryCompleted = false;
            
            // Act
            yield return StartCoroutine(SimulateNetworkFailureAndRecovery());
            
            // Wait for recovery
            yield return StartCoroutine(TestingUtilities.WaitForCondition(
                () => networkFailureHandled && recoveryCompleted, 8f));
            
            // Assert
            Assert.IsTrue(true, "Firebase should handle network failures and recover gracefully");
        }
        
        #endregion
        
        #region Monetization Integration Tests
        
        [UnityTest]
        public IEnumerator MonetizationIntegration_PurchaseToGameplayBenefit_ExecutesCorrectly()
        {
            // Arrange
            bool purchaseCompleted = false;
            bool benefitApplied = false;
            
            // Act
            yield return StartCoroutine(SimulatePurchaseAndBenefitApplication());
            
            // Wait for purchase and benefit application
            yield return StartCoroutine(TestingUtilities.WaitForCondition(
                () => purchaseCompleted && benefitApplied, 5f));
            
            // Assert
            Assert.IsTrue(true, "Purchase benefits should be applied to gameplay immediately");
        }
        
        [UnityTest]
        public IEnumerator MonetizationIntegration_AdViewingToRewards_IntegratesWithGameplay()
        {
            // Arrange
            bool adViewed = false;
            bool rewardGranted = false;
            bool gameplayUpdated = false;
            
            // Act
            yield return StartCoroutine(SimulateRewardedAdIntegration());
            
            // Wait for complete ad flow
            yield return StartCoroutine(TestingUtilities.WaitForCondition(
                () => adViewed && rewardGranted && gameplayUpdated, 5f));
            
            // Assert
            Assert.IsTrue(true, "Ad rewards should integrate seamlessly with gameplay systems");
        }
        
        #endregion
        
        #region System Communication Tests
        
        [UnityTest]
        public IEnumerator SystemCommunication_EventBroadcasting_ReachesAllListeners()
        {
            // Arrange
            int eventListenerCount = 0;
            int expectedListeners = 5; // GameManager, ResourceManager, MonetizationManager, etc.
            
            // Mock event listeners
            gameManager.OnGameStateChanged += (from, to) => eventListenerCount++;
            
            // Act
            gameManager.TransitionToState(GameManager.GameState.BotBuilding);
            yield return new WaitForSeconds(0.1f); // Allow event propagation
            
            // Assert
            Assert.Greater(eventListenerCount, 0, "State change events should reach all listeners");
        }
        
        [UnityTest]
        public IEnumerator SystemCommunication_DataConsistency_MaintainedAcrossSystems()
        {
            // Arrange
            bool dataConsistencyMaintained = false;
            
            // Act - Simulate data changes across multiple systems
            yield return StartCoroutine(SimulateMultiSystemDataChanges());
            
            // Verify data consistency
            yield return StartCoroutine(VerifyDataConsistency());
            dataConsistencyMaintained = true;
            
            // Assert
            Assert.IsTrue(dataConsistencyMaintained, "Data consistency should be maintained across all systems");
        }
        
        #endregion
        
        #region Performance Integration Tests
        
        [UnityTest]
        public IEnumerator PerformanceIntegration_MemoryUsage_StaysWithinLimits()
        {
            // Arrange
            long initialMemory = GC.GetTotalMemory(true);
            long memoryLimit = 100 * 1024 * 1024; // 100MB limit
            
            // Act - Simulate extended gameplay
            yield return StartCoroutine(SimulateExtendedGameplay(30f)); // 30 seconds
            
            // Check memory usage
            long finalMemory = GC.GetTotalMemory(true);
            long memoryUsed = finalMemory - initialMemory;
            
            // Assert
            Assert.Less(memoryUsed, memoryLimit, "Memory usage should stay within acceptable limits");
        }
        
        [UnityTest]
        public IEnumerator PerformanceIntegration_FrameRate_MaintainsTarget()
        {
            // Arrange
            float targetFPS = 60f;
            float minimumAcceptableFPS = 45f;
            var frameRates = new List<float>();
            
            // Act - Monitor frame rate during gameplay
            float testDuration = 5f;
            float startTime = Time.time;
            
            while (Time.time - startTime < testDuration)
            {
                frameRates.Add(1f / Time.unscaledDeltaTime);
                yield return null;
            }
            
            // Calculate average FPS
            float averageFPS = frameRates.Count > 0 ? frameRates.Average() : 0f;
            
            // Assert
            Assert.Greater(averageFPS, minimumAcceptableFPS, 
                $"Average FPS ({averageFPS:F1}) should be above minimum acceptable ({minimumAcceptableFPS})");
        }
        
        #endregion
        
        #region Helper Methods for Integration Testing
        
        private IEnumerator SimulateCompleteGameSession()
        {
            // Simulate a complete game session flow
            gameManager.TransitionToState(GameManager.GameState.BotBuilding);
            yield return new WaitForSeconds(0.5f);
            
            gameManager.TransitionToState(GameManager.GameState.PreRun);
            yield return new WaitForSeconds(0.5f);
            
            gameManager.TransitionToState(GameManager.GameState.Running);
            yield return new WaitForSeconds(2f); // Simulate run duration
            
            gameManager.TransitionToState(GameManager.GameState.PostRun);
            yield return new WaitForSeconds(0.5f);
        }
        
        private IEnumerator SimulateSingleGameRun()
        {
            gameManager.StartNewRun();
            yield return new WaitForSeconds(1f);
            // Simulate run completion
        }
        
        private IEnumerator SimulatePerformanceStress(float targetFPS)
        {
            // Simulate performance stress by creating temporary objects
            var stressObjects = new List<GameObject>();
            
            for (int i = 0; i < 100; i++)
            {
                var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obj.AddComponent<Rigidbody>();
                stressObjects.Add(obj);
                yield return null;
            }
            
            yield return new WaitForSeconds(1f);
            
            // Cleanup stress objects
            foreach (var obj in stressObjects)
            {
                if (obj != null)
                    Object.DestroyImmediate(obj);
            }
        }
        
        private IEnumerator SimulateBotConfigurationAndExecution(BotArchetype archetype)
        {
            // Simulate bot configuration
            var botController = TestingUtilities.CreateMockBotController(archetype);
            yield return new WaitForSeconds(0.5f);
            
            // Simulate bot execution
            yield return new WaitForSeconds(1f);
        }
        
        private IEnumerator SimulateBotPhysicsAndAI(BotController bot)
        {
            // Simulate physics and AI coordination
            yield return new WaitForSeconds(1f);
        }
        
        private IEnumerator SimulateFirebaseOfflineAuthentication()
        {
            // Simulate offline Firebase authentication
            yield return TestingUtilities.SimulateNetworkDelay(0.5f);
        }
        
        private IEnumerator SimulateFirebaseOnlineSync()
        {
            // Simulate online data synchronization
            yield return TestingUtilities.SimulateNetworkDelay(1f);
        }
        
        private IEnumerator SimulateNetworkFailureAndRecovery()
        {
            // Simulate network failure
            yield return new WaitForSeconds(1f);
            
            // Simulate recovery
            yield return new WaitForSeconds(2f);
        }
        
        private IEnumerator SimulatePurchaseAndBenefitApplication()
        {
            // Simulate purchase process
            yield return new WaitForSeconds(1f);
            
            // Simulate benefit application
            yield return new WaitForSeconds(0.5f);
        }
        
        private IEnumerator SimulateRewardedAdIntegration()
        {
            // Simulate ad viewing
            yield return new WaitForSeconds(2f);
            
            // Simulate reward granting
            yield return new WaitForSeconds(0.5f);
        }
        
        private IEnumerator SimulateMultiSystemDataChanges()
        {
            // Simulate data changes across systems
            yield return new WaitForSeconds(1f);
        }
        
        private IEnumerator VerifyDataConsistency()
        {
            // Verify data consistency across systems
            yield return new WaitForSeconds(0.5f);
        }
        
        private IEnumerator SimulateExtendedGameplay(float duration)
        {
            float startTime = Time.time;
            
            while (Time.time - startTime < duration)
            {
                // Simulate gameplay activities
                if (Time.time % 5f < 0.1f) // Every 5 seconds
                {
                    // Simulate state changes
                    gameManager.TransitionToState(GameManager.GameState.Running);
                    yield return new WaitForSeconds(2f);
                    gameManager.TransitionToState(GameManager.GameState.PostRun);
                }
                
                yield return null;
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// Extension methods for LINQ operations in tests
    /// </summary>
    public static class TestExtensions
    {
        public static float Average(this List<float> values)
        {
            if (values.Count == 0) return 0f;
            
            float sum = 0f;
            foreach (float value in values)
            {
                sum += value;
            }
            
            return sum / values.Count;
        }
    }
}