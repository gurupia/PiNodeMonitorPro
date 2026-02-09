# Changelog

All notable changes to Pi Node Monitor Pro will be documented in this file.

## [2.0.1] - 2026-02-09

### Documentation
- Added `GURUPIA_DEV_GUIDE.md`: 프로젝트 개발 지침서 (`gurupia-dev` 워크플로우 기반).

---

## [2.0.0] - 2026-01-27

### Security Enhancements (P0)

#### DPAPI Encryption for Sensitive Data
- Added `SecureStorageService` class using Windows DPAPI for encrypting sensitive data
- API keys, Telegram tokens, and SMS credentials are now encrypted at rest
- Automatic migration from plaintext to encrypted format for existing configurations
- `CurrentUser` scope ensures only the current Windows user can decrypt

#### Enhanced Remote Authentication
- Upgraded PIN from 4-digit numeric to 6-character alphanumeric
- Implemented cryptographically secure random PIN generation using `RNGCryptoServiceProvider`
- Excluded confusing characters (0, O, 1, I) for better readability

#### Rate Limiting
- Added brute-force protection with 5-attempt limit
- 15-minute lockout after exceeding failed attempts
- IP-based tracking for per-client rate limiting
- HTTP 429 response with `Retry-After` header for locked-out clients

### Architecture Improvements (P1)

#### Async Logging System
- Implemented `AsyncLogger` with background queue processing
- Eliminated UI thread blocking during file I/O operations
- Batch writing with 1-second flush intervals
- Automatic log rotation (10MB max, 7-day retention)

### Changed Files
- `Services/Security/SecureStorageService.cs` - NEW: DPAPI encryption service
- `Services/Security/AuthenticationService.cs` - NEW: Enhanced auth + rate limiting
- `Services/Logging/AsyncLogger.cs` - NEW: Async logging system
- `Services/Sms/SmsConfigModel.cs` - MODIFIED: Encrypted storage properties
- `MobileServer.cs` - MODIFIED: Integrated new authentication system
- `PiNodeMonitorWinForm.csproj` - MODIFIED: Added System.Security reference, v2.0.0

### Technical Notes
- Requires .NET Framework 4.8
- System.Security assembly required for DPAPI
- Backward compatible with v1.x configuration files (auto-migration)

---

## [1.8.26] - Previous Stable Release

- Initial stable release with monitoring features
- Mobile server with screen sharing
- Telegram/SMS notifications
- Docker container management
