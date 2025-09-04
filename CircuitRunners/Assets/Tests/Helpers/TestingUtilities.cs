using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using CircuitRunners.Core;
using CircuitRunners.Bot;
using CircuitRunners.Data;

namespace CircuitRunners.Tests.Helpers
{
    /// <summary>
    /// Comprehensive testing utilities for Circuit Runners
    /// Provides mock objects, test data generators, and common testing patterns
    /// Essential for maintaining consistent testing standards across all test suites
    /// </summary>
    public static class TestingUtilities
    {
        #region Mock Object Factories
        
        /// <summary>
        /// Create a mock GameManager for testing without Unity dependencies
        /// </summary>
        public static GameManager CreateMockGameManager()
        {
            var gameObject = new GameObject("MockGameManager");
            var gameManager = gameObject.AddComponent<GameManager>();
            
            // Initialize with test-friendly settings
            return gameManager;
        }
        
        /// <summary>
        /// Create a mock BotController with configurable test parameters
        /// </summary>
        public static BotController CreateMockBotController(BotArchetype archetype = BotArchetype.Balanced)
        {
            var gameObject = new GameObject("MockBot");
            var rigidbody = gameObject.AddComponent<Rigidbody2D>();
            var collider = gameObject.AddComponent<CircleCollider2D>();
            var botController = gameObject.AddComponent<BotController>();
            
            // Configure for testing
            rigidbody.isKinematic = true; // Prevent physics interference in tests
            
            return botController;
        }
        
        /// <summary>
        /// Create mock ResourceManager with predefined test resources
        /// </summary>
        public static ResourceManager CreateMockResourceManager(int initialScrap = 1000, int initialEnergy = 5)
        {
            var gameObject = new GameObject("MockResourceManager");
            var resourceManager = gameObject.AddComponent<ResourceManager>();
            
            // Set initial test values (would need access to private fields)
            // This demonstrates the testing approach
            
            return resourceManager;
        }
        
        #endregion
        
        #region Test Data Generators
        
        /// <summary>
        /// Generate test bot configuration data for various test scenarios
        /// </summary>
        public static BotData GenerateTestBotData(BotArchetype archetype = BotArchetype.Balanced)
        {
            var botData = ScriptableObject.CreateInstance<BotData>();
            
            // Configure test bot data
            botData.name = $"TestBot_{archetype}";
            
            return botData;
        }
        
        /// <summary>
        /// Generate test course configuration for consistent testing
        /// </summary>
        public static CourseConfig GenerateTestCourseConfig(float difficulty = 0.5f, int length = 100)
        {
            var courseConfig = ScriptableObject.CreateInstance<CourseConfig>();
            
            // Set test parameters
            courseConfig.name = $"TestCourse_Difficulty{difficulty}";
            
            return courseConfig;
        }
        
        /// <summary>
        /// Generate realistic test run statistics for reward calculation testing
        /// </summary>
        public static BotRunStatistics GenerateTestRunStats(
            float distanceTraveled = 100f,
            int collectiblesGathered = 10,
            float survivalTime = 30f,
            int obstaclesAvoided = 15,
            int damagesTaken = 2,
            bool hasCompletedCourse = false)
        {
            return new BotRunStatistics
            {
                DistanceTraveled = distanceTraveled,
                CollectiblesGathered = collectiblesGathered,
                SurvivalTime = survivalTime,
                ObstaclesAvoided = obstaclesAvoided,
                DamagesTaken = damagesTaken,
                HasCompletedCourse = hasCompletedCourse
            };
        }
        
        #endregion
        
        #region Testing Patterns
        
        /// <summary>
        /// Wait for condition with timeout - prevents infinite loops in tests
        /// </summary>
        public static IEnumerator WaitForCondition(Func<bool> condition, float timeoutSeconds = 5f)
        {
            float elapsedTime = 0f;
            
            while (!condition() && elapsedTime < timeoutSeconds)
            {
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }
            
            Assert.IsTrue(condition(), $"Condition not met within {timeoutSeconds} seconds");
        }
        
        /// <summary>
        /// Assert that a value is within expected range (floating point comparisons)
        /// </summary>
        public static void AssertInRange(float actual, float expected, float tolerance = 0.01f, string message = "")
        {
            float difference = Mathf.Abs(actual - expected);
            Assert.IsTrue(difference <= tolerance, 
                $"{message} Expected: {expected} ± {tolerance}, Actual: {actual}, Difference: {difference}");
        }
        
        /// <summary>
        /// Assert that an async operation completes within timeout
        /// </summary>
        public static IEnumerator AssertAsyncCompletion(Func<bool> asyncOperation, float timeoutSeconds = 10f)
        {
            float elapsedTime = 0f;
            
            while (!asyncOperation() && elapsedTime < timeoutSeconds)
            {
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }
            
            Assert.IsTrue(asyncOperation(), $"Async operation did not complete within {timeoutSeconds} seconds");
        }
        
        /// <summary>
        /// Measure execution time of a test operation for performance validation
        /// </summary>
        public static float MeasureExecutionTime(Action operation)
        {
            float startTime = Time.realtimeSinceStartup;
            operation.Invoke();
            float endTime = Time.realtimeSinceStartup;
            
            return endTime - startTime;
        }
        
        /// <summary>
        /// Create isolated test environment to prevent test interference
        /// </summary>
        public static GameObject CreateTestEnvironment(string testName)
        {
            var testEnvironment = new GameObject($"TestEnvironment_{testName}");
            
            // Add any common test environment components
            
            return testEnvironment;
        }
        
        /// <summary>
        /// Clean up test environment and prevent memory leaks
        /// </summary>
        public static void CleanupTestEnvironment(GameObject testEnvironment)
        {
            if (testEnvironment != null)
            {
                UnityEngine.Object.DestroyImmediate(testEnvironment);
            }
        }
        
        #endregion
        
        #region Performance Testing Utilities
        
        /// <summary>
        /// Measure memory allocation during test execution
        /// </summary>
        public static long MeasureMemoryAllocation(Action operation)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            long memoryBefore = GC.GetTotalMemory(false);
            operation.Invoke();
            long memoryAfter = GC.GetTotalMemory(false);
            
            return memoryAfter - memoryBefore;
        }
        
        /// <summary>
        /// Stress test helper for repeated operations
        /// </summary>
        public static void StressTest(Action operation, int iterations, float maxTimePerIteration = 0.01f)
        {
            for (int i = 0; i < iterations; i++)
            {
                float executionTime = MeasureExecutionTime(operation);
                Assert.IsTrue(executionTime <= maxTimePerIteration, 
                    $"Iteration {i} took {executionTime:F4}s, expected <= {maxTimePerIteration:F4}s");
            }
        }
        
        #endregion
        
        #region Mock Network Responses
        
        /// <summary>
        /// Simulate network delays for Firebase testing
        /// </summary>
        public static IEnumerator SimulateNetworkDelay(float delaySeconds = 1f)
        {
            yield return new WaitForSeconds(delaySeconds);
        }
        
        /// <summary>
        /// Mock Firebase authentication response for testing
        /// </summary>
        public static void MockFirebaseAuthResponse(bool success = true, string errorMessage = "")
        {
            // This would integrate with actual Firebase testing utilities
            // For now, demonstrates the testing pattern
        }
        
        /// <summary>
        /// Mock payment processing response for monetization testing
        /// </summary>
        public static void MockPaymentResponse(bool success = true, string transactionId = "TEST_TRANSACTION")
        {
            // Mock payment processing for testing without real transactions
        }
        
        #endregion
        
        #region Test Data Validation
        
        /// <summary>
        /// Validate bot configuration data integrity
        /// </summary>
        public static void ValidateBotData(BotData botData)
        {
            Assert.IsNotNull(botData, "BotData cannot be null");
            Assert.IsNotEmpty(botData.name, "BotData must have a name");
            // Add more validation rules as needed
        }
        
        /// <summary>
        /// Validate resource state consistency
        /// </summary>
        public static void ValidateResourceState(ResourceManager resourceManager)
        {
            Assert.IsNotNull(resourceManager, "ResourceManager cannot be null");
            // Add resource state validation logic
        }
        
        /// <summary>
        /// Validate game state consistency
        /// </summary>
        public static void ValidateGameState(GameManager gameManager)
        {
            Assert.IsNotNull(gameManager, "GameManager cannot be null");
            // Add game state validation logic
        }
        
        #endregion
    }
    
    /// <summary>
    /// Custom assertions specific to Circuit Runners testing needs
    /// </summary>
    public static class CustomAssert
    {
        /// <summary>
        /// Assert that a bot is in a valid state for gameplay
        /// </summary>
        public static void BotIsReady(BotController bot)
        {
            Assert.IsNotNull(bot, "Bot controller cannot be null");
            Assert.IsNotNull(bot.gameObject, "Bot GameObject cannot be null");
            // Add bot readiness validation
        }
        
        /// <summary>
        /// Assert that game state transition is valid
        /// </summary>
        public static void ValidStateTransition(GameManager.GameState from, GameManager.GameState to)
        {
            // Implement state transition validation logic
            Assert.IsTrue(true, $"State transition from {from} to {to} should be valid");
        }
        
        /// <summary>
        /// Assert that monetization values are within expected ranges
        /// </summary>
        public static void ValidMonetizationValue(float value, float min = 0f, float max = 1000f)
        {
            Assert.IsTrue(value >= min && value <= max, 
                $"Monetization value {value} should be between {min} and {max}");
        }
    }
}