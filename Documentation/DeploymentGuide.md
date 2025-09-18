# 🚀 راهنمای Deployment سیستم بیمه تکمیلی

## 📋 **پیش‌نیازها**

### **سیستم‌عامل**
- Windows Server 2019 یا بالاتر
- .NET Framework 4.8 یا بالاتر
- IIS 10.0 یا بالاتر

### **پایگاه داده**
- SQL Server 2019 یا بالاتر
- SQL Server Management Studio
- Backup و Recovery Plan

### **سخت‌افزار**
- **CPU**: حداقل 4 Core
- **RAM**: حداقل 8 GB
- **Storage**: حداقل 100 GB SSD
- **Network**: حداقل 1 Gbps

---

## 🔧 **مراحل Deployment**

### **مرحله 1: آماده‌سازی محیط**

#### **1.1 نصب .NET Framework**
```powershell
# بررسی نسخه .NET Framework
Get-ItemProperty "HKLM:SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\" -Name Release

# نصب .NET Framework 4.8 (در صورت نیاز)
# Download from: https://dotnet.microsoft.com/download/dotnet-framework
```

#### **1.2 نصب IIS**
```powershell
# فعال‌سازی IIS
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServer
Enable-WindowsOptionalFeature -Online -FeatureName IIS-CommonHttpFeatures
Enable-WindowsOptionalFeature -Online -FeatureName IIS-HttpErrors
Enable-WindowsOptionalFeature -Online -FeatureName IIS-HttpLogging
Enable-WindowsOptionalFeature -Online -FeatureName IIS-RequestFiltering
Enable-WindowsOptionalFeature -Online -FeatureName IIS-StaticContent
Enable-WindowsOptionalFeature -Online -FeatureName IIS-DefaultDocument
Enable-WindowsOptionalFeature -Online -FeatureName IIS-DirectoryBrowsing
Enable-WindowsOptionalFeature -Online -FeatureName IIS-ASPNET45
```

#### **1.3 تنظیم Application Pool**
```powershell
# ایجاد Application Pool
New-WebAppPool -Name "ClinicAppPool" -Force

# تنظیم Application Pool
Set-ItemProperty -Path "IIS:\AppPools\ClinicAppPool" -Name processModel.identityType -Value ApplicationPoolIdentity
Set-ItemProperty -Path "IIS:\AppPools\ClinicAppPool" -Name managedRuntimeVersion -Value "v4.0"
Set-ItemProperty -Path "IIS:\AppPools\ClinicAppPool" -Name enable32BitAppOnWin64 -Value $false
```

---

### **مرحله 2: تنظیم پایگاه داده**

#### **2.1 ایجاد Database**
```sql
-- ایجاد Database
CREATE DATABASE ClinicApp_SupplementaryInsurance
ON 
( NAME = 'ClinicApp_SupplementaryInsurance',
  FILENAME = 'C:\Data\ClinicApp_SupplementaryInsurance.mdf',
  SIZE = 100MB,
  MAXSIZE = 1GB,
  FILEGROWTH = 10MB )
LOG ON 
( NAME = 'ClinicApp_SupplementaryInsurance_Log',
  FILENAME = 'C:\Data\ClinicApp_SupplementaryInsurance_Log.ldf',
  SIZE = 10MB,
  MAXSIZE = 100MB,
  FILEGROWTH = 1MB );
```

#### **2.2 اجرای Migration**
```powershell
# اجرای Migration
Update-Database -ConnectionString "Server=localhost;Database=ClinicApp_SupplementaryInsurance;Trusted_Connection=true;"
```

#### **2.3 ایجاد Indexes**
```sql
-- Indexes برای بهبود عملکرد
CREATE INDEX IX_InsuranceTariff_InsuranceType 
ON InsuranceTariffs(InsuranceType);

CREATE INDEX IX_InsuranceTariff_SupplementaryCoveragePercent 
ON InsuranceTariffs(SupplementaryCoveragePercent);

CREATE INDEX IX_InsuranceTariff_SupplementaryMaxPayment 
ON InsuranceTariffs(SupplementaryMaxPayment);

CREATE INDEX IX_PatientInsurance_PatientId_Active 
ON PatientInsurances(PatientId, IsActive) 
WHERE IsActive = 1;
```

---

### **مرحله 3: تنظیم Application**

#### **3.1 کپی فایل‌ها**
```powershell
# کپی فایل‌های Application
Copy-Item -Path "C:\Build\ClinicApp\*" -Destination "C:\inetpub\wwwroot\ClinicApp" -Recurse -Force
```

#### **3.2 تنظیم Web.config**
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <connectionStrings>
    <add name="DefaultConnection" 
         connectionString="Server=localhost;Database=ClinicApp_SupplementaryInsurance;Trusted_Connection=true;MultipleActiveResultSets=true;" 
         providerName="System.Data.SqlClient" />
  </connectionStrings>
  
  <appSettings>
    <add key="SupplementaryInsurance:CacheSettings:TariffCacheExpiryMinutes" value="30" />
    <add key="SupplementaryInsurance:CacheSettings:SettingsCacheExpiryMinutes" value="60" />
    <add key="SupplementaryInsurance:CacheSettings:CalculationCacheExpiryMinutes" value="15" />
    <add key="SupplementaryInsurance:MonitoringSettings:EnableLogging" value="true" />
    <add key="SupplementaryInsurance:MonitoringSettings:LogLevel" value="Information" />
    <add key="SupplementaryInsurance:CalculationSettings:MaxCalculationTime" value="5000" />
    <add key="SupplementaryInsurance:CalculationSettings:EnableAdvancedSettings" value="true" />
  </appSettings>
  
  <system.web>
    <compilation debug="false" targetFramework="4.8" />
    <httpRuntime targetFramework="4.8" maxRequestLength="51200" executionTimeout="300" />
    <authentication mode="Forms">
      <forms loginUrl="~/Account/Login" timeout="2880" />
    </authentication>
    <authorization>
      <deny users="?" />
    </authorization>
  </system.web>
</configuration>
```

#### **3.3 تنظیم IIS Site**
```powershell
# ایجاد IIS Site
New-Website -Name "ClinicApp" -Port 80 -PhysicalPath "C:\inetpub\wwwroot\ClinicApp" -ApplicationPool "ClinicAppPool"

# تنظیم SSL (اختیاری)
New-WebBinding -Name "ClinicApp" -Protocol "https" -Port 443
```

---

### **مرحله 4: تنظیم Cache و Monitoring**

#### **4.1 تنظیم Redis Cache (اختیاری)**
```powershell
# نصب Redis
choco install redis-64

# تنظیم Redis Service
sc create Redis binpath= "C:\Program Files\Redis\redis-server.exe" start= auto
sc start Redis
```

#### **4.2 تنظیم Serilog**
```xml
<configuration>
  <appSettings>
    <add key="serilog:write-to:File.path" value="C:\Logs\ClinicApp\supplementary-insurance-.log" />
    <add key="serilog:write-to:File.rollingInterval" value="Day" />
    <add key="serilog:write-to:File.retainedFileCountLimit" value="30" />
    <add key="serilog:minimum-level" value="Information" />
  </appSettings>
</configuration>
```

#### **4.3 تنظیم Performance Counters**
```powershell
# ایجاد Performance Counters
New-Item -Path "HKLM:\SYSTEM\CurrentControlSet\Services\ClinicApp" -Force
New-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Services\ClinicApp" -Name "Performance" -Value "1" -PropertyType DWORD
```

---

### **مرحله 5: تست و اعتبارسنجی**

#### **5.1 تست اتصال Database**
```csharp
// تست اتصال
public async Task<bool> TestDatabaseConnection()
{
    try
    {
        using (var context = new ApplicationDbContext())
        {
            await context.Database.CanConnectAsync();
            return true;
        }
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "خطا در اتصال به پایگاه داده");
        return false;
    }
}
```

#### **5.2 تست Cache**
```csharp
// تست Cache
public async Task<bool> TestCache()
{
    try
    {
        var result = await _cacheService.GetCachedSupplementaryTariffsAsync(1);
        return result.Success;
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "خطا در Cache");
        return false;
    }
}
```

#### **5.3 تست محاسبه**
```csharp
// تست محاسبه
public async Task<bool> TestCalculation()
{
    try
    {
        var result = await _supplementaryService.CalculateSupplementaryInsuranceAsync(
            1, 1, 1000000m, 500000m, DateTime.UtcNow);
        return result.Success;
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "خطا در محاسبه");
        return false;
    }
}
```

---

## 🔒 **امنیت**

### **تنظیمات امنیتی**

#### **SSL/TLS**
```powershell
# نصب SSL Certificate
Import-Certificate -FilePath "C:\Certificates\ClinicApp.pfx" -CertStoreLocation Cert:\LocalMachine\My

# تنظیم SSL Binding
New-WebBinding -Name "ClinicApp" -Protocol "https" -Port 443 -SslFlags 1
```

#### **Firewall Rules**
```powershell
# تنظیم Firewall
New-NetFirewallRule -DisplayName "ClinicApp HTTP" -Direction Inbound -Protocol TCP -LocalPort 80 -Action Allow
New-NetFirewallRule -DisplayName "ClinicApp HTTPS" -Direction Inbound -Protocol TCP -LocalPort 443 -Action Allow
New-NetFirewallRule -DisplayName "ClinicApp SQL" -Direction Inbound -Protocol TCP -LocalPort 1433 -Action Allow
```

#### **Authentication**
```xml
<system.web>
  <authentication mode="Forms">
    <forms loginUrl="~/Account/Login" 
           timeout="2880" 
           requireSSL="true" 
           slidingExpiration="true" />
  </authentication>
  
  <authorization>
    <deny users="?" />
  </authorization>
</system.web>
```

---

## 📊 **Monitoring و Alerting**

### **تنظیمات Monitoring**

#### **Application Insights**
```xml
<configuration>
  <appSettings>
    <add key="ApplicationInsights:InstrumentationKey" value="your-instrumentation-key" />
    <add key="ApplicationInsights:EnableAdaptiveSampling" value="false" />
    <add key="ApplicationInsights:EnableQuickPulseMetricStream" value="true" />
  </appSettings>
</configuration>
```

#### **Health Checks**
```csharp
public class SupplementaryInsuranceHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // بررسی Database
            var dbHealthy = await CheckDatabaseHealth();
            if (!dbHealthy)
                return HealthCheckResult.Unhealthy("Database connection failed");
            
            // بررسی Cache
            var cacheHealthy = await CheckCacheHealth();
            if (!cacheHealthy)
                return HealthCheckResult.Degraded("Cache issues detected");
            
            // بررسی Performance
            var performanceHealthy = await CheckPerformanceHealth();
            if (!performanceHealthy)
                return HealthCheckResult.Degraded("Performance issues detected");
            
            return HealthCheckResult.Healthy("All systems operational");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Health check failed", ex);
        }
    }
}
```

### **Alerting Rules**

#### **PowerShell Script برای Alerting**
```powershell
# Script برای بررسی Health
$healthCheck = Invoke-WebRequest -Uri "https://clinicapp.com/health" -UseBasicParsing
if ($healthCheck.StatusCode -ne 200)
{
    # ارسال Alert
    Send-MailMessage -To "admin@clinicapp.com" -Subject "Health Check Failed" -Body "Application is not responding"
}
```

---

## 🔄 **Backup و Recovery**

### **Database Backup**

#### **Full Backup**
```sql
-- Full Backup
BACKUP DATABASE ClinicApp_SupplementaryInsurance
TO DISK = 'C:\Backup\ClinicApp_SupplementaryInsurance_Full.bak'
WITH FORMAT, INIT, NAME = 'ClinicApp_SupplementaryInsurance Full Backup';
```

#### **Differential Backup**
```sql
-- Differential Backup
BACKUP DATABASE ClinicApp_SupplementaryInsurance
TO DISK = 'C:\Backup\ClinicApp_SupplementaryInsurance_Diff.bak'
WITH DIFFERENTIAL, NAME = 'ClinicApp_SupplementaryInsurance Differential Backup';
```

#### **Transaction Log Backup**
```sql
-- Transaction Log Backup
BACKUP LOG ClinicApp_SupplementaryInsurance
TO DISK = 'C:\Backup\ClinicApp_SupplementaryInsurance_Log.trn'
WITH NAME = 'ClinicApp_SupplementaryInsurance Transaction Log Backup';
```

### **Application Backup**

#### **PowerShell Script برای Backup**
```powershell
# Backup Application Files
$sourcePath = "C:\inetpub\wwwroot\ClinicApp"
$backupPath = "C:\Backup\ClinicApp_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
Copy-Item -Path $sourcePath -Destination $backupPath -Recurse

# Backup Configuration
Copy-Item -Path "C:\inetpub\wwwroot\ClinicApp\Web.config" -Destination "$backupPath\Web.config.backup"
```

### **Recovery Procedures**

#### **Database Recovery**
```sql
-- Restore Full Backup
RESTORE DATABASE ClinicApp_SupplementaryInsurance
FROM DISK = 'C:\Backup\ClinicApp_SupplementaryInsurance_Full.bak'
WITH REPLACE, NORECOVERY;

-- Restore Transaction Log
RESTORE LOG ClinicApp_SupplementaryInsurance
FROM DISK = 'C:\Backup\ClinicApp_SupplementaryInsurance_Log.trn'
WITH RECOVERY;
```

#### **Application Recovery**
```powershell
# Restore Application Files
$backupPath = "C:\Backup\ClinicApp_20240115_143000"
$targetPath = "C:\inetpub\wwwroot\ClinicApp"
Copy-Item -Path $backupPath -Destination $targetPath -Recurse -Force

# Restart Application Pool
Restart-WebAppPool -Name "ClinicAppPool"
```

---

## 🚀 **Performance Optimization**

### **IIS Optimization**

#### **Application Pool Settings**
```powershell
# تنظیمات Application Pool
Set-ItemProperty -Path "IIS:\AppPools\ClinicAppPool" -Name processModel.idleTimeout -Value "00:20:00"
Set-ItemProperty -Path "IIS:\AppPools\ClinicAppPool" -Name processModel.maxIdleTime -Value "00:20:00"
Set-ItemProperty -Path "IIS:\AppPools\ClinicAppPool" -Name processModel.loadUserProfile -Value $true
Set-ItemProperty -Path "IIS:\AppPools\ClinicAppPool" -Name processModel.setApplicationPoolCredentials -Value $true
```

#### **Output Caching**
```xml
<system.webServer>
  <caching>
    <profiles>
      <add extension=".aspx" policy="CacheUntilChange" kernelCachePolicy="CacheUntilChange" />
      <add extension=".css" policy="CacheUntilChange" kernelCachePolicy="CacheUntilChange" />
      <add extension=".js" policy="CacheUntilChange" kernelCachePolicy="CacheUntilChange" />
    </profiles>
  </caching>
</system.webServer>
```

### **Database Optimization**

#### **Query Optimization**
```sql
-- بررسی Query Performance
SELECT 
    query_hash,
    query_plan_hash,
    execution_count,
    total_elapsed_time,
    total_elapsed_time / execution_count AS avg_elapsed_time
FROM sys.dm_exec_query_stats
WHERE sql_handle IN (
    SELECT sql_handle 
    FROM sys.dm_exec_sql_text(sql_handle) 
    WHERE text LIKE '%InsuranceTariff%'
)
ORDER BY total_elapsed_time DESC;
```

#### **Index Maintenance**
```sql
-- بررسی Index Fragmentation
SELECT 
    OBJECT_NAME(ips.object_id) AS table_name,
    i.name AS index_name,
    ips.avg_fragmentation_in_percent
FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') ips
INNER JOIN sys.indexes i ON ips.object_id = i.object_id AND ips.index_id = i.index_id
WHERE ips.avg_fragmentation_in_percent > 10
ORDER BY ips.avg_fragmentation_in_percent DESC;
```

---

## 🔧 **Troubleshooting**

### **مشکلات رایج**

#### **Database Connection Issues**
```powershell
# بررسی اتصال Database
Test-NetConnection -ComputerName localhost -Port 1433

# بررسی SQL Server Service
Get-Service -Name "MSSQLSERVER"

# بررسی Connection String
$connectionString = "Server=localhost;Database=ClinicApp_SupplementaryInsurance;Trusted_Connection=true;"
$connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
try
{
    $connection.Open()
    Write-Host "Database connection successful"
}
catch
{
    Write-Host "Database connection failed: $($_.Exception.Message)"
}
finally
{
    $connection.Close()
}
```

#### **Cache Issues**
```powershell
# بررسی Cache Service
Get-Service -Name "Redis"

# بررسی Cache Performance
$cacheStats = Invoke-WebRequest -Uri "https://clinicapp.com/api/cache/stats" -UseBasicParsing
$cacheStats.Content
```

#### **Performance Issues**
```powershell
# بررسی CPU Usage
Get-Counter -Counter "\Processor(_Total)\% Processor Time" -SampleInterval 1 -MaxSamples 10

# بررسی Memory Usage
Get-Counter -Counter "\Memory\Available MBytes" -SampleInterval 1 -MaxSamples 10

# بررسی Disk Usage
Get-Counter -Counter "\LogicalDisk(C:)\% Free Space" -SampleInterval 1 -MaxSamples 10
```

### **Log Analysis**

#### **Event Log Analysis**
```powershell
# بررسی Application Logs
Get-EventLog -LogName Application -Source "ClinicApp" -Newest 10

# بررسی IIS Logs
Get-Content "C:\inetpub\logs\LogFiles\W3SVC1\*.log" | Select-String "ERROR" | Select-Object -Last 10
```

#### **Performance Log Analysis**
```powershell
# بررسی Performance Logs
Get-Content "C:\Logs\ClinicApp\supplementary-insurance-*.log" | Select-String "ERROR" | Select-Object -Last 10
```

---

## 📋 **Checklist Deployment**

### **Pre-Deployment**
- [ ] بررسی پیش‌نیازهای سیستم
- [ ] نصب .NET Framework 4.8
- [ ] نصب و تنظیم IIS
- [ ] نصب SQL Server
- [ ] ایجاد Database
- [ ] اجرای Migration

### **Deployment**
- [ ] کپی فایل‌های Application
- [ ] تنظیم Web.config
- [ ] تنظیم Application Pool
- [ ] تنظیم IIS Site
- [ ] تنظیم SSL Certificate
- [ ] تنظیم Firewall Rules

### **Post-Deployment**
- [ ] تست اتصال Database
- [ ] تست Cache
- [ ] تست محاسبه
- [ ] تست Authentication
- [ ] تست Performance
- [ ] تنظیم Monitoring
- [ ] تنظیم Backup

### **Go-Live**
- [ ] تست کامل سیستم
- [ ] بررسی Logs
- [ ] تنظیم Alerting
- [ ] مستندسازی
- [ ] آموزش تیم
- [ ] راه‌اندازی Production

---

**آخرین به‌روزرسانی**: 15 دی 1403  
**نسخه**: 1.0.0  
**نویسنده**: تیم توسعه سیستم‌های درمانی
