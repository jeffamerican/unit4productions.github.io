# Circuit Runners - Firebase Infrastructure Setup Guide

## 1. Firebase Project Configuration (FREE Tier Optimized)

### Initial Project Setup
1. **Create Firebase Project**
   - Go to [Firebase Console](https://console.firebase.google.com)
   - Click "Add project"
   - Project name: `circuit-runners-prod`
   - Disable Google Analytics for now (can enable later if needed)
   - Choose "Spark Plan" (FREE)

2. **Project Settings Configuration**
   ```
   Project ID: circuit-runners-prod
   Web API Key: [Auto-generated]
   Project Number: [Auto-generated]
   Storage Bucket: circuit-runners-prod.appspot.com
   ```

### Multi-Platform Setup

#### iOS Configuration
1. Add iOS app to project
   - iOS bundle ID: `com.botinc.circuitrunners`
   - App nickname: `Circuit Runners iOS`
   - Download `GoogleService-Info.plist`
   - Place in Unity project: `Assets/StreamingAssets/`

#### Android Configuration
1. Add Android app to project
   - Android package name: `com.botinc.circuitrunners`
   - App nickname: `Circuit Runners Android`
   - SHA-1 certificate fingerprint: [Your debug/release keys]
   - Download `google-services.json`
   - Place in Unity project: `Assets/StreamingAssets/`

## 2. Firebase Services Configuration

### Authentication
- **Providers to Enable:**
  - Anonymous: ✅ (Primary for instant play)
  - Google: ✅ (Optional upgrade path)
  - Apple: ✅ (iOS requirement for App Store)
  
- **Settings:**
  - One account per email address: Disabled
  - Account deletion: Enabled (GDPR compliance)

### Firestore Database
- **Mode:** Production mode
- **Location:** us-central1 (best for free tier)
- **Security Rules:** Custom (see security rules section)

### Cloud Storage
- **Location:** us-central1
- **Security Rules:** Custom for bot sharing features

### Analytics
- **Configuration:**
  - Data retention: 2 months (free tier)
  - Google Ads linking: Disabled initially
  - Data sharing: Minimal for privacy

### Remote Config
- **Parameters to set up:**
  - `daily_energy_limit`: 100
  - `tournament_enabled`: true
  - `new_bot_unlock_level`: 5
  - `ad_frequency`: 3
  - `premium_discount`: 0

## 3. Free Tier Limits and Optimization

### Firestore Limits (FREE Tier)
- **Reads:** 50,000/day
- **Writes:** 20,000/day  
- **Deletes:** 20,000/day
- **Storage:** 1 GiB
- **Bandwidth:** 10 GiB/month

### Cloud Functions Limits
- **Invocations:** 125,000/month
- **Compute time:** 40,000 GB-seconds/month
- **Outbound networking:** 5GB/month

### Authentication Limits
- **Users:** Unlimited on free tier
- **Custom tokens:** 1,000/hour

### Optimization Strategies
1. **Batch Operations:** Group multiple writes into single transactions
2. **Offline First:** Cache data locally, sync only when needed
3. **Smart Queries:** Use compound indices, limit query results
4. **Data Denormalization:** Optimize for read performance over storage
5. **Scheduled Functions:** Use time-triggered functions for maintenance

## 4. Unity SDK Installation

### Required Packages
```
Firebase Authentication: 11.7.0+
Firebase Firestore: 11.7.0+
Firebase Analytics: 11.7.0+
Firebase Crashlytics: 11.7.0+
Firebase Remote Config: 11.7.0+
Firebase Functions: 11.7.0+ (if using Cloud Functions)
```

### Installation Steps
1. Download Firebase Unity SDK
2. Import packages in order:
   - FirebaseAnalytics.unitypackage
   - FirebaseAuth.unitypackage
   - FirebaseFirestore.unitypackage
   - FirebaseCrashlytics.unitypackage
   - FirebaseRemoteConfig.unitypackage

3. Configure build settings:
   - Android: Enable custom gradle template
   - iOS: Set minimum iOS version to 10.0+

## 5. Initial Security Configuration

### Basic Security Rules Preview
```javascript
// Firestore Security Rules (Basic)
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {
    // Users can only access their own data
    match /users/{userId} {
      allow read, write: if request.auth != null && request.auth.uid == userId;
    }
    
    // Public leaderboards (read-only for clients)
    match /leaderboards/{document=**} {
      allow read: if request.auth != null;
      allow write: if false; // Only server can write
    }
  }
}
```

## 6. Monitoring and Alerts

### Usage Monitoring Setup
1. **Firebase Console Monitoring**
   - Set up quota alerts at 80% of free tier limits
   - Monitor daily Firestore operations
   - Track authentication user counts

2. **Custom Monitoring Dashboard**
   - Daily active users
   - Database operations per user
   - Function invocation patterns
   - Error rates and crashes

### Scaling Preparation
- **10K Users Projection:**
  - Firestore reads: ~30K/day (within limits)
  - Firestore writes: ~15K/day (within limits)
  - Authentication: No limits
  - Functions: ~50K invocations/month (within limits)

**Next Steps After Setup:**
1. Run through Unity integration scripts
2. Test authentication flow
3. Implement core data models
4. Set up monitoring dashboard
5. Deploy initial Cloud Functions

This configuration supports 10K+ users on the free tier with smart optimization strategies.