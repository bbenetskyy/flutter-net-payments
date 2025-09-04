# micro_app

Simple Flutter app with Login, Items list, and Logout using an API at http://localhost:5247.

## Features
- Login screen (no registration) using POST http://localhost:5247/auth/login (ApiClient auto-uses http://10.0.2.2:5247 on Android emulators and http://127.0.0.1:5247 on macOS/iOS).
- Items screen fetching list using GET http://localhost:5247/items.
- Logout clears token and returns to Login.
- Token persisted via shared_preferences.
- After a successful login, the app stores the used email/password locally and shows a "Saved logins" list on the Login screen to quickly sign in again.
- Saved logins persist between app launches.

> Important: For development convenience, saved credentials (email and password) are stored in plain text in SharedPreferences on the device. Do NOT use this approach for production apps. Use the platform keystore/keychain and never store raw passwords.

## Endpoints (assumed)
- POST /auth/login -> { "token": "..." }
  - Request body: { "email": string, "password": string }
- GET /items -> [ { "id": any, "name": string }, ... ]
  - Authorization: Bearer <token>

If your API uses different paths/fields, update lib/services/api_client.dart accordingly.

## Run
1. Start your backend on http://localhost:5247.
2. Flutter: `flutter pub get` then `flutter run`.

## Android HTTP (cleartext)
- AndroidManifest enables INTERNET and usesCleartextTraffic for http://localhost:5247.
- On Android emulators, use http://10.0.2.2:5247 to reach your host machine. ApiClient already defaults to that base URL on Android.

## iOS HTTP note
- If your backend doesn’t use HTTPS, you may need to allow App Transport Security exceptions in ios/Runner/Info.plist for development.

## macOS HTTP note
- HTTP (cleartext) is blocked by default on macOS due to App Transport Security (ATS). We added scoped ATS exceptions for localhost and 127.0.0.1 in macos/Runner/Info.plist to allow development against http://localhost:5247. For production, remove or tighten these exceptions and use HTTPS.

## Code structure
- lib/pages/login_page.dart: Login UI + saved credentials list
- lib/pages/items_page.dart: Items list + logout
- lib/services/api_client.dart: API calls
- lib/services/auth_storage.dart: Token & saved credentials persistence
- lib/models/item.dart: Item model
- lib/main.dart: App entry and AuthGate
