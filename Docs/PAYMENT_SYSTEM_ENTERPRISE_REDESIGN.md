# 🏦 سیستم پرداخت Enterprise-Grade - بازطراحی کامل

**تاریخ:** 2026-01-06  
**وضعیت:** 🔄 در حال پیاده‌سازی  
**اولویت:** 🔴 CRITICAL (ماژول مالی)

---

## 📋 فهرست مطالب

1. [معماری کلی](#معماری-کلی)
2. [اصول طراحی](#اصول-طراحی)
3. [Validation Layers](#validation-layers)
4. [Security Measures](#security-measures)
5. [Logging & Audit](#logging--audit)
6. [Transaction Management](#transaction-management)
7. [Error Handling](#error-handling)
8. [Concurrency Control](#concurrency-control)
9. [Implementation Plan](#implementation-plan)

---

## 🏗️ معماری کلی

### Layer Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    Presentation Layer                    │
│  (Controllers, Filters, Rate Limiting, CSRF Protection)  │
└─────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────┐
│                    Validation Layer                      │
│  (Input Validation, Business Rules, Security Checks)    │
└─────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────┐
│                    Service Layer                        │
│  (Business Logic, Orchestration, Transaction Mgmt)      │
└─────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────┐
│                    Repository Layer                      │
│  (Data Access, Pessimistic Locking, Concurrency)       │
└─────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────┐
│                    Gateway Layer                        │
│  (ZarinPal Driver, Retry Logic, Circuit Breaker)       │
└─────────────────────────────────────────────────────────┘
```

---

## 🎯 اصول طراحی

### 1. Defense in Depth
- **Multiple Validation Layers:** Input → Business → Security → Gateway
- **Fail-Safe Defaults:** در صورت خطا، همیشه به حالت امن برمی‌گردیم
- **Least Privilege:** هر لایه فقط به داده‌های لازم دسترسی دارد

### 2. Zero Trust
- **Verify Everything:** هیچ داده‌ای بدون اعتبارسنجی پذیرفته نمی‌شود
- **Assume Breach:** همیشه فرض می‌کنیم سیستم در معرض خطر است
- **Continuous Validation:** اعتبارسنجی در هر مرحله

### 3. Financial Integrity
- **ACID Transactions:** تمام عملیات مالی در Transaction
- **Audit Trail:** تمام تغییرات لاگ می‌شوند
- **Idempotency:** جلوگیری از پرداخت‌های تکراری
- **Double Payment Prevention:** جلوگیری از پرداخت دوگانه

### 4. Observability
- **Structured Logging:** تمام عملیات با Correlation ID
- **Metrics:** Performance, Error Rate, Success Rate
- **Tracing:** End-to-End tracing برای هر پرداخت

---

## 🛡️ Validation Layers

### Layer 1: Input Validation (Controller Level)

```csharp
// ✅ Amount Validation
- Amount > 0
- Amount <= MaxAllowedAmount (200M تومان)
- Amount precision: decimal(18, 0) - بدون اعشار
- Amount type: decimal (نه float/double)

// ✅ ID Validation
- AppointmentId > 0
- PatientId > 0
- PaymentGatewayId > 0

// ✅ String Validation
- CallbackUrl: Valid URL format
- UserIpAddress: Valid IP format
- UserAgent: MaxLength(500)
```

### Layer 2: Business Rule Validation (Service Level)

```csharp
// ✅ Appointment Validation
- Appointment exists and not deleted
- Appointment.Status == Scheduled || Pending
- Appointment.PatientId matches current user
- Appointment.Price matches requested amount
- Appointment not expired

// ✅ Payment Gateway Validation
- Gateway exists and active
- Gateway.IsActive == true
- Gateway configuration valid
- Gateway.MerchantId configured

// ✅ Business Rules
- Patient has no pending payment for same appointment
- Amount matches appointment price
- Payment not already completed
```

### Layer 3: Security Validation (Security Service)

```csharp
// ✅ Rate Limiting
- Max 10 payment requests per hour per user
- Max 5 payment requests per minute per IP
- Circuit breaker for gateway failures

// ✅ IP Validation
- IP not in blacklist
- IP matches user's known IPs (optional)
- IP geolocation check (optional)

// ✅ User Agent Validation
- UserAgent not suspicious
- UserAgent matches known patterns

// ✅ Idempotency
- IdempotencyKey not used before
- IdempotencyKey format valid
- IdempotencyKey TTL: 30 minutes
```

### Layer 4: Gateway Validation (Gateway Driver)

```csharp
// ✅ Gateway Configuration
- MerchantId configured
- API URLs valid
- Sandbox/Production mode correct

// ✅ Request Validation
- Amount in valid range
- CallbackUrl accessible
- Request timeout: 30 seconds

// ✅ Response Validation
- Response signature valid (if applicable)
- Response status code valid
- Response data structure valid
```

---

## 🔒 Security Measures

### 1. Anti-Fraud Measures

```csharp
// ✅ Amount Anomaly Detection
- Amount > 10x average payment → Flag for review
- Multiple payments in short time → Rate limit
- Payment from new IP → Additional verification

// ✅ Pattern Detection
- Same card used for multiple payments → Flag
- Payment from multiple IPs → Flag
- Payment outside business hours → Flag (optional)
```

### 2. Rate Limiting

```csharp
// ✅ Per-User Rate Limiting
- Max 10 payment requests per hour
- Max 50 payment requests per day

// ✅ Per-IP Rate Limiting
- Max 5 payment requests per minute
- Max 100 payment requests per hour

// ✅ Per-Appointment Rate Limiting
- Max 3 payment attempts per appointment
- Cooldown: 5 minutes between attempts
```

### 3. Digital Signature Verification

```csharp
// ✅ Gateway Callback Signature
- Verify callback signature (if gateway supports)
- Validate callback data integrity
- Prevent callback replay attacks

// ✅ Webhook Signature
- Verify webhook signature
- Validate webhook timestamp
- Prevent webhook replay attacks
```

### 4. Idempotency

```csharp
// ✅ Idempotency Key
- Format: "payment_{appointmentId}_{userId}_{timestamp}"
- TTL: 30 minutes
- Scope: "appointment_payment"
- Storage: Redis/Memory Cache
```

---

## 📊 Logging & Audit

### Structured Logging با Serilog

```csharp
// ✅ Payment Request Logging
_logger.Information(
    "💰 PAYMENT REQUEST: درخواست پردازش پرداخت - " +
    "AppointmentId: {AppointmentId}, " +
    "PatientId: {PatientId}, " +
    "Amount: {Amount}, " +
    "GatewayType: {GatewayType}, " +
    "UserIpAddress: {UserIpAddress}, " +
    "IdempotencyKey: {IdempotencyKey}, " +
    "CorrelationId: {CorrelationId}",
    appointmentId, patientId, amount, gatewayType, userIp, idempotencyKey, correlationId);

// ✅ Payment Success Logging
_logger.Information(
    "✅ PAYMENT SUCCESS: پرداخت موفق - " +
    "OnlinePaymentId: {OnlinePaymentId}, " +
    "AppointmentId: {AppointmentId}, " +
    "Amount: {Amount}, " +
    "GatewayTransactionId: {GatewayTransactionId}, " +
    "RefId: {RefId}, " +
    "CorrelationId: {CorrelationId}",
    onlinePaymentId, appointmentId, amount, gatewayTxId, refId, correlationId);

// ✅ Payment Failure Logging
_logger.Error(
    ex,
    "❌ PAYMENT FAILURE: خطا در پرداخت - " +
    "OnlinePaymentId: {OnlinePaymentId}, " +
    "AppointmentId: {AppointmentId}, " +
    "ErrorCode: {ErrorCode}, " +
    "ErrorMessage: {ErrorMessage}, " +
    "CorrelationId: {CorrelationId}",
    onlinePaymentId, appointmentId, errorCode, errorMessage, correlationId);
```

### Audit Trail

```csharp
// ✅ تمام تغییرات در OnlinePayment
- Created: Who, When, What
- Updated: Who, When, What, Why
- Status Changed: From → To, Reason
- Amount Changed: Old → New, Reason
- Deleted: Who, When, Why (Soft Delete)
```

### Correlation ID

```csharp
// ✅ هر پرداخت یک Correlation ID دارد
- Format: Guid.NewGuid().ToString("N")
- Passed through all layers
- Used in all logs
- Returned to client for support
```

---

## 💾 Transaction Management

### Transaction Isolation Levels

```csharp
// ✅ Read Committed (Default)
- Prevents dirty reads
- Allows non-repeatable reads
- Suitable for payment processing

// ✅ Serializable (برای Critical Operations)
- Highest isolation
- Prevents all anomalies
- Used for: Payment verification, Status update
```

### Pessimistic Locking

```csharp
// ✅ SQL Server UPDLOCK, ROWLOCK
var appointment = await _context.Appointments
    .SqlQuery("SELECT * FROM Appointments WITH (UPDLOCK, ROWLOCK) WHERE AppointmentId = @p0", 
        new SqlParameter("@p0", appointmentId))
    .FirstOrDefaultAsync();

// ✅ Entity Framework
var appointment = await _context.Appointments
    .Where(a => a.AppointmentId == appointmentId)
    .FirstOrDefaultAsync();
    
// Lock at database level
_context.Database.ExecuteSqlCommand(
    "SELECT * FROM Appointments WITH (UPDLOCK, ROWLOCK) WHERE AppointmentId = @p0",
    new SqlParameter("@p0", appointmentId));
```

### Optimistic Concurrency Control

```csharp
// ✅ RowVersion در OnlinePayment
[Timestamp]
public byte[] RowVersion { get; set; }

// ✅ Check RowVersion before update
var originalRowVersion = onlinePayment.RowVersion;
// ... modify ...
onlinePayment.RowVersion = originalRowVersion; // EF will check
await _context.SaveChangesAsync(); // Throws DbUpdateConcurrencyException if changed
```

---

## ⚠️ Error Handling

### Defensive Programming

```csharp
// ✅ Null Checks
if (appointment == null)
{
    _logger.Warning("Appointment {AppointmentId} not found", appointmentId);
    return ServiceResult<PaymentResult>.Failed("نوبت یافت نشد");
}

// ✅ Range Checks
if (amount <= 0 || amount > 200000000) // 200M تومان
{
    _logger.Warning("Invalid amount: {Amount}", amount);
    return ServiceResult<PaymentResult>.Failed("مبلغ نامعتبر است");
}

// ✅ State Checks
if (appointment.Status != AppointmentStatus.Scheduled && 
    appointment.Status != AppointmentStatus.Pending)
{
    _logger.Warning("Appointment {AppointmentId} in invalid status: {Status}", 
        appointmentId, appointment.Status);
    return ServiceResult<PaymentResult>.Failed("نوبت در وضعیت قابل پرداخت نیست");
}
```

### Retry Logic

```csharp
// ✅ Exponential Backoff
private async Task<T> RetryWithBackoffAsync<T>(
    Func<Task<T>> operation,
    int maxRetries = 3,
    int initialDelayMs = 1000)
{
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            return await operation();
        }
        catch (HttpRequestException ex) when (i < maxRetries - 1)
        {
            var delay = initialDelayMs * (int)Math.Pow(2, i);
            _logger.Warning("Retry {RetryCount}/{MaxRetries} after {Delay}ms", 
                i + 1, maxRetries, delay);
            await Task.Delay(delay);
        }
    }
    throw new Exception("Max retries exceeded");
}
```

### Circuit Breaker

```csharp
// ✅ Circuit Breaker Pattern
private class CircuitBreaker
{
    private int _failureCount = 0;
    private DateTime? _lastFailureTime = null;
    private const int FailureThreshold = 5;
    private const int TimeoutSeconds = 60;
    
    public bool IsOpen()
    {
        if (_failureCount < FailureThreshold)
            return false;
            
        if (_lastFailureTime.HasValue && 
            (DateTime.UtcNow - _lastFailureTime.Value).TotalSeconds > TimeoutSeconds)
        {
            _failureCount = 0;
            _lastFailureTime = null;
            return false;
        }
        
        return true;
    }
}
```

---

## 🔄 Concurrency Control

### Double Payment Prevention

```csharp
// ✅ Idempotency Check
var idempotencyKey = $"payment_{appointmentId}_{userId}_{timestamp}";
var canProcess = await _idempotencyService.TryUseKeyAsync(idempotencyKey, ttlMinutes: 30);

if (!canProcess)
{
    // Check if payment already exists
    var existingPayment = await _context.OnlinePayments
        .FirstOrDefaultAsync(op => 
            op.AppointmentId == appointmentId && 
            op.Status == OnlinePaymentStatus.Pending);
            
    if (existingPayment != null)
        return existingPayment.PaymentUrl;
}

// ✅ Pessimistic Lock
using (var transaction = _context.Database.BeginTransaction(IsolationLevel.ReadCommitted))
{
    var appointment = await _context.Appointments
        .SqlQuery("SELECT * FROM Appointments WITH (UPDLOCK, ROWLOCK) WHERE AppointmentId = @p0",
            new SqlParameter("@p0", appointmentId))
        .FirstOrDefaultAsync();
        
    // Check again after lock
    var existingPayment = await _context.OnlinePayments
        .FirstOrDefaultAsync(op => 
            op.AppointmentId == appointmentId && 
            op.Status == OnlinePaymentStatus.Pending);
            
    if (existingPayment != null)
    {
        transaction.Rollback();
        return existingPayment.PaymentUrl;
    }
    
    // Create payment
    // ...
    
    transaction.Commit();
}
```

### Race Condition Handling

```csharp
// ✅ Optimistic Concurrency
try
{
    var onlinePayment = await _context.OnlinePayments
        .FirstOrDefaultAsync(op => op.OnlinePaymentId == onlinePaymentId);
        
    var originalRowVersion = onlinePayment.RowVersion;
    
    onlinePayment.Status = OnlinePaymentStatus.Successful;
    onlinePayment.RowVersion = originalRowVersion; // EF will check
    
    await _context.SaveChangesAsync();
}
catch (DbUpdateConcurrencyException ex)
{
    _logger.Warning("Concurrency conflict for OnlinePayment {OnlinePaymentId}", onlinePaymentId);
    
    // Reload and retry
    var entry = ex.Entries.Single();
    await entry.ReloadAsync();
    
    // Retry with new data
    // ...
}
```

---

## 📝 Implementation Plan

### Phase 1: Core Validation ✅
- [x] Input Validation Layer
- [x] Business Rule Validation
- [ ] Security Validation Service
- [ ] Gateway Configuration Validation

### Phase 2: Security & Anti-Fraud 🔄
- [ ] Rate Limiting Implementation
- [ ] IP Validation
- [ ] Digital Signature Verification
- [ ] Anti-Fraud Pattern Detection

### Phase 3: Logging & Audit 📊
- [ ] Structured Logging Enhancement
- [ ] Correlation ID Implementation
- [ ] Audit Trail Service
- [ ] Performance Metrics

### Phase 4: Transaction & Concurrency 🔄
- [ ] Transaction Isolation Levels
- [ ] Pessimistic Locking Enhancement
- [ ] Optimistic Concurrency Control
- [ ] Double Payment Prevention

### Phase 5: Error Handling & Resilience ⚠️
- [ ] Retry Logic with Exponential Backoff
- [ ] Circuit Breaker Pattern
- [ ] Graceful Degradation
- [ ] Error Recovery Strategies

---

## ✅ Checklist برای هر Payment Request

### Pre-Processing
- [ ] Input Validation (Amount, IDs, Strings)
- [ ] Business Rule Validation (Appointment, Gateway)
- [ ] Security Validation (Rate Limit, IP, User Agent)
- [ ] Idempotency Check
- [ ] Correlation ID Generation

### Processing
- [ ] Transaction Begin (IsolationLevel.ReadCommitted)
- [ ] Pessimistic Lock on Appointment
- [ ] Double Payment Check (After Lock)
- [ ] OnlinePayment Creation
- [ ] Gateway Request (with Retry Logic)
- [ ] OnlinePayment Update
- [ ] Transaction Commit
- [ ] Post-Save Verification

### Post-Processing
- [ ] Success Logging
- [ ] Audit Trail Entry
- [ ] Notification (Async)
- [ ] Response to Client

### Error Handling
- [ ] Exception Catch
- [ ] Error Logging (with Correlation ID)
- [ ] Transaction Rollback
- [ ] Status Update (Failed)
- [ ] User-Friendly Error Message

---

## 🔍 Testing Strategy

### Unit Tests
- Input Validation Tests
- Business Rule Validation Tests
- Security Validation Tests
- Error Handling Tests

### Integration Tests
- Payment Flow Tests
- Gateway Integration Tests
- Concurrency Tests
- Transaction Tests

### Load Tests
- Rate Limiting Tests
- Concurrent Payment Tests
- Gateway Timeout Tests
- Database Lock Tests

### Security Tests
- Fraud Detection Tests
- Rate Limiting Tests
- IP Validation Tests
- Signature Verification Tests

---

## 📚 References

- [OWASP Payment Security](https://owasp.org/www-project-payment-security/)
- [PCI DSS Compliance](https://www.pcisecuritystandards.org/)
- [Financial Transaction Best Practices](https://www.fdic.gov/)
- [Defensive Programming](https://en.wikipedia.org/wiki/Defensive_programming)

---

**Next Steps:**
1. پیاده‌سازی Validation Layers
2. اضافه کردن Security Measures
3. بهبود Logging & Audit
4. تقویت Transaction Management
5. پیاده‌سازی Error Handling & Resilience

