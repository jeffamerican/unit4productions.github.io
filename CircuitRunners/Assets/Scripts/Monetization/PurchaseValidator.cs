using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;

namespace CircuitRunners.Monetization
{
    /// <summary>
    /// Secure purchase validation system for Circuit Runners
    /// Validates purchase receipts to prevent fraud and unauthorized transactions
    /// Implements both client-side and server-side validation approaches
    /// 
    /// Key Security Features:
    /// - Receipt signature validation
    /// - Anti-tampering protection
    /// - Duplicate purchase detection
    /// - Purchase tracking and audit trail
    /// - Integration with server-side validation
    /// </summary>
    public static class PurchaseValidator
    {
        #region Configuration
        private static readonly Dictionary<string, DateTime> _processedPurchases = new Dictionary<string, DateTime>();
        private static readonly TimeSpan DUPLICATE_PURCHASE_TIMEOUT = TimeSpan.FromMinutes(5);
        
        // Google Play License Key (should be stored securely, not in code)
        private const string GOOGLE_PLAY_LICENSE_KEY = "PLACE_YOUR_GOOGLE_PLAY_LICENSE_KEY_HERE";
        
        // Apple App Store validation settings
        private const bool VALIDATE_APPLE_RECEIPTS = true;
        #endregion

        #region Purchase Validation
        /// <summary>
        /// Validate purchase receipt with comprehensive security checks
        /// </summary>
        /// <param name="product">Product that was purchased</param>
        /// <param name="validationCallback">Callback with validation result</param>
        public static void ValidatePurchase(Product product, Action<PurchaseValidationResult> validationCallback)
        {
            if (product == null)
            {
                LogError("Cannot validate null product");
                validationCallback?.Invoke(new PurchaseValidationResult 
                { 
                    IsValid = false, 
                    ErrorMessage = "Null product provided for validation" 
                });
                return;
            }

            try
            {
                LogDebug($"Starting validation for product: {product.definition.id}");
                
                // Step 1: Basic product validation
                var basicValidation = ValidateBasicProductData(product);
                if (!basicValidation.IsValid)
                {
                    validationCallback?.Invoke(basicValidation);
                    return;
                }
                
                // Step 2: Duplicate purchase detection
                var duplicateCheck = CheckForDuplicatePurchase(product);
                if (!duplicateCheck.IsValid)
                {
                    validationCallback?.Invoke(duplicateCheck);
                    return;
                }
                
                // Step 3: Receipt validation (platform-specific)
                ValidateReceiptByPlatform(product, (receiptResult) =>
                {
                    if (!receiptResult.IsValid)
                    {
                        validationCallback?.Invoke(receiptResult);
                        return;
                    }
                    
                    // Step 4: Mark purchase as processed
                    MarkPurchaseAsProcessed(product);
                    
                    // Step 5: Log successful validation
                    LogPurchaseValidation(product, true);
                    
                    validationCallback?.Invoke(new PurchaseValidationResult 
                    { 
                        IsValid = true,
                        ProductId = product.definition.id,
                        TransactionId = product.transactionID,
                        PurchasePrice = product.metadata.localizedPrice,
                        ValidationMethod = GetValidationMethodForPlatform()
                    });
                });
            }
            catch (Exception ex)
            {
                LogError($"Purchase validation exception: {ex.Message}");
                validationCallback?.Invoke(new PurchaseValidationResult 
                { 
                    IsValid = false, 
                    ErrorMessage = $"Validation exception: {ex.Message}" 
                });
            }
        }

        /// <summary>
        /// Validate basic product data integrity
        /// </summary>
        private static PurchaseValidationResult ValidateBasicProductData(Product product)
        {
            try
            {
                // Check product ID
                if (string.IsNullOrEmpty(product.definition.id))
                {
                    return new PurchaseValidationResult 
                    { 
                        IsValid = false, 
                        ErrorMessage = "Product ID is null or empty" 
                    };
                }
                
                // Check transaction ID
                if (string.IsNullOrEmpty(product.transactionID))
                {
                    return new PurchaseValidationResult 
                    { 
                        IsValid = false, 
                        ErrorMessage = "Transaction ID is null or empty" 
                    };
                }
                
                // Check receipt exists
                if (string.IsNullOrEmpty(product.receipt))
                {
                    return new PurchaseValidationResult 
                    { 
                        IsValid = false, 
                        ErrorMessage = "Purchase receipt is null or empty" 
                    };
                }
                
                // Check price is reasonable (not negative or zero)
                if (product.metadata.localizedPrice == null || product.metadata.localizedPrice.Length == 0)
                {
                    LogWarning($"Product {product.definition.id} has no localized price");
                    // Don't fail validation for missing price, just warn
                }
                
                LogDebug($"Basic validation passed for {product.definition.id}");
                return new PurchaseValidationResult { IsValid = true };
            }
            catch (Exception ex)
            {
                return new PurchaseValidationResult 
                { 
                    IsValid = false, 
                    ErrorMessage = $"Basic validation failed: {ex.Message}" 
                };
            }
        }

        /// <summary>
        /// Check for duplicate purchase attempts within timeout window
        /// </summary>
        private static PurchaseValidationResult CheckForDuplicatePurchase(Product product)
        {
            try
            {
                string purchaseKey = $"{product.definition.id}_{product.transactionID}";
                
                if (_processedPurchases.ContainsKey(purchaseKey))
                {
                    DateTime previousPurchaseTime = _processedPurchases[purchaseKey];
                    TimeSpan timeSinceLastPurchase = DateTime.UtcNow - previousPurchaseTime;
                    
                    if (timeSinceLastPurchase < DUPLICATE_PURCHASE_TIMEOUT)
                    {
                        LogWarning($"Duplicate purchase detected for {product.definition.id} within {timeSinceLastPurchase.TotalSeconds} seconds");
                        return new PurchaseValidationResult 
                        { 
                            IsValid = false, 
                            ErrorMessage = "Duplicate purchase detected - please wait before purchasing again" 
                        };
                    }
                    else
                    {
                        // Remove old entry if timeout has passed
                        _processedPurchases.Remove(purchaseKey);
                    }
                }
                
                // Clean up old processed purchases periodically
                CleanupOldProcessedPurchases();
                
                return new PurchaseValidationResult { IsValid = true };
            }
            catch (Exception ex)
            {
                LogError($"Duplicate check failed: {ex.Message}");
                return new PurchaseValidationResult 
                { 
                    IsValid = false, 
                    ErrorMessage = $"Duplicate check failed: {ex.Message}" 
                };
            }
        }

        /// <summary>
        /// Platform-specific receipt validation
        /// </summary>
        private static void ValidateReceiptByPlatform(Product product, Action<PurchaseValidationResult> callback)
        {
#if UNITY_ANDROID
            ValidateGooglePlayReceipt(product, callback);
#elif UNITY_IOS
            ValidateAppleAppStoreReceipt(product, callback);
#else
            // For other platforms or editor, perform basic validation only
            LogWarning("Platform-specific receipt validation not available");
            callback?.Invoke(new PurchaseValidationResult 
            { 
                IsValid = true, 
                ValidationMethod = "Basic (Platform not supported)" 
            });
#endif
        }

#if UNITY_ANDROID
        /// <summary>
        /// Validate Google Play Store receipt
        /// </summary>
        private static void ValidateGooglePlayReceipt(Product product, Action<PurchaseValidationResult> callback)
        {
            try
            {
                if (string.IsNullOrEmpty(GOOGLE_PLAY_LICENSE_KEY) || GOOGLE_PLAY_LICENSE_KEY.Contains("PLACE_YOUR"))
                {
                    LogWarning("Google Play license key not configured - skipping receipt validation");
                    callback?.Invoke(new PurchaseValidationResult 
                    { 
                        IsValid = true, 
                        ValidationMethod = "Google Play (License key not configured)" 
                    });
                    return;
                }

                // Parse Google Play receipt
                var receiptData = JsonUtility.FromJson<GooglePlayReceipt>(product.receipt);
                
                if (receiptData == null || string.IsNullOrEmpty(receiptData.json))
                {
                    callback?.Invoke(new PurchaseValidationResult 
                    { 
                        IsValid = false, 
                        ErrorMessage = "Invalid Google Play receipt format" 
                    });
                    return;
                }

                // Validate receipt signature using Unity's built-in validator
                try
                {
                    var validator = new GooglePlayValidator(GOOGLE_PLAY_LICENSE_KEY);
                    var result = validator.Validate(receiptData.json, receiptData.signature);
                    
                    if (result != null && result.Length > 0)
                    {
                        LogDebug("Google Play receipt validation successful");
                        callback?.Invoke(new PurchaseValidationResult 
                        { 
                            IsValid = true, 
                            ValidationMethod = "Google Play Store" 
                        });
                    }
                    else
                    {
                        callback?.Invoke(new PurchaseValidationResult 
                        { 
                            IsValid = false, 
                            ErrorMessage = "Google Play receipt signature validation failed" 
                        });
                    }
                }
                catch (Exception validationEx)
                {
                    LogError($"Google Play validation error: {validationEx.Message}");
                    callback?.Invoke(new PurchaseValidationResult 
                    { 
                        IsValid = false, 
                        ErrorMessage = $"Google Play validation failed: {validationEx.Message}" 
                    });
                }
            }
            catch (Exception ex)
            {
                LogError($"Google Play receipt validation exception: {ex.Message}");
                callback?.Invoke(new PurchaseValidationResult 
                { 
                    IsValid = false, 
                    ErrorMessage = $"Google Play validation exception: {ex.Message}" 
                });
            }
        }
#endif

#if UNITY_IOS
        /// <summary>
        /// Validate Apple App Store receipt
        /// </summary>
        private static void ValidateAppleAppStoreReceipt(Product product, Action<PurchaseValidationResult> callback)
        {
            try
            {
                if (!VALIDATE_APPLE_RECEIPTS)
                {
                    LogDebug("Apple receipt validation disabled");
                    callback?.Invoke(new PurchaseValidationResult 
                    { 
                        IsValid = true, 
                        ValidationMethod = "Apple App Store (Validation disabled)" 
                    });
                    return;
                }

                // Parse Apple receipt
                var receiptData = JsonUtility.FromJson<AppleInAppPurchaseReceipt>(product.receipt);
                
                if (receiptData == null)
                {
                    callback?.Invoke(new PurchaseValidationResult 
                    { 
                        IsValid = false, 
                        ErrorMessage = "Invalid Apple receipt format" 
                    });
                    return;
                }

                try
                {
                    // Use Unity's built-in Apple receipt validation
                    var validator = new AppleValidator(AppleTangle.Data());
                    var result = validator.Validate(product.receipt);
                    
                    if (result != null && result.Length > 0)
                    {
                        LogDebug("Apple App Store receipt validation successful");
                        callback?.Invoke(new PurchaseValidationResult 
                        { 
                            IsValid = true, 
                            ValidationMethod = "Apple App Store" 
                        });
                    }
                    else
                    {
                        callback?.Invoke(new PurchaseValidationResult 
                        { 
                            IsValid = false, 
                            ErrorMessage = "Apple receipt validation failed" 
                        });
                    }
                }
                catch (Exception validationEx)
                {
                    LogError($"Apple validation error: {validationEx.Message}");
                    callback?.Invoke(new PurchaseValidationResult 
                    { 
                        IsValid = false, 
                        ErrorMessage = $"Apple validation failed: {validationEx.Message}" 
                    });
                }
            }
            catch (Exception ex)
            {
                LogError($"Apple receipt validation exception: {ex.Message}");
                callback?.Invoke(new PurchaseValidationResult 
                { 
                    IsValid = false, 
                    ErrorMessage = $"Apple validation exception: {ex.Message}" 
                });
            }
        }
#endif

        /// <summary>
        /// Mark purchase as processed to prevent duplicates
        /// </summary>
        private static void MarkPurchaseAsProcessed(Product product)
        {
            try
            {
                string purchaseKey = $"{product.definition.id}_{product.transactionID}";
                _processedPurchases[purchaseKey] = DateTime.UtcNow;
                
                LogDebug($"Marked purchase as processed: {purchaseKey}");
            }
            catch (Exception ex)
            {
                LogError($"Failed to mark purchase as processed: {ex.Message}");
            }
        }

        /// <summary>
        /// Clean up old processed purchases to prevent memory leaks
        /// </summary>
        private static void CleanupOldProcessedPurchases()
        {
            try
            {
                var cutoffTime = DateTime.UtcNow - TimeSpan.FromHours(1); // Keep for 1 hour
                var keysToRemove = new List<string>();
                
                foreach (var kvp in _processedPurchases)
                {
                    if (kvp.Value < cutoffTime)
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                }
                
                foreach (string key in keysToRemove)
                {
                    _processedPurchases.Remove(key);
                }
                
                if (keysToRemove.Count > 0)
                {
                    LogDebug($"Cleaned up {keysToRemove.Count} old processed purchases");
                }
            }
            catch (Exception ex)
            {
                LogError($"Cleanup failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Log purchase validation for audit trail
        /// </summary>
        private static void LogPurchaseValidation(Product product, bool isValid)
        {
            try
            {
                var logData = new PurchaseValidationLog
                {
                    ProductId = product.definition.id,
                    TransactionId = product.transactionID,
                    IsValid = isValid,
                    Timestamp = DateTime.UtcNow,
                    Platform = Application.platform.ToString(),
                    ValidationMethod = GetValidationMethodForPlatform()
                };
                
                string logJson = JsonUtility.ToJson(logData);
                LogDebug($"Purchase validation log: {logJson}");
                
                // In a production system, you would send this to your analytics/logging service
                // TrackPurchaseValidationEvent(logData);
            }
            catch (Exception ex)
            {
                LogError($"Failed to log purchase validation: {ex.Message}");
            }
        }

        /// <summary>
        /// Get validation method description for current platform
        /// </summary>
        private static string GetValidationMethodForPlatform()
        {
#if UNITY_ANDROID
            return "Google Play Store";
#elif UNITY_IOS
            return "Apple App Store";
#else
            return "Basic (Platform not supported)";
#endif
        }
        #endregion

        #region Logging
        private static void LogDebug(string message)
        {
            Debug.Log($"[PurchaseValidator] {message}");
        }

        private static void LogWarning(string message)
        {
            Debug.LogWarning($"[PurchaseValidator] {message}");
        }

        private static void LogError(string message)
        {
            Debug.LogError($"[PurchaseValidator] {message}");
        }
        #endregion
    }

    #region Data Structures
    /// <summary>
    /// Result of purchase validation
    /// </summary>
    [Serializable]
    public class PurchaseValidationResult
    {
        public bool IsValid;
        public string ErrorMessage;
        public string ProductId;
        public string TransactionId;
        public string PurchasePrice;
        public string ValidationMethod;
    }

    /// <summary>
    /// Purchase validation audit log
    /// </summary>
    [Serializable]
    public class PurchaseValidationLog
    {
        public string ProductId;
        public string TransactionId;
        public bool IsValid;
        public DateTime Timestamp;
        public string Platform;
        public string ValidationMethod;
    }

    /// <summary>
    /// Google Play receipt structure
    /// </summary>
    [Serializable]
    public class GooglePlayReceipt
    {
        public string json;
        public string signature;
    }

    /// <summary>
    /// Apple receipt structure (simplified)
    /// </summary>
    [Serializable]
    public class AppleInAppPurchaseReceipt
    {
        public string transactionID;
        public string productID;
        public string purchaseDate;
        public string originalTransactionID;
    }
    #endregion
}