# Supplementary Insurance Tariff Module Dependencies

## Overview
This document outlines the dependencies and architecture of the Supplementary Insurance Tariff Module in the ClinicApp system.

## Controller Dependencies

### SupplementaryTariffController

The `SupplementaryTariffController` depends on the following services and repositories:

#### Core Services
- **SupplementaryTariffSeederService**: Service for seeding supplementary tariff data
- **IInsuranceTariffService**: Service for managing insurance tariffs
- **IInsurancePlanService**: Service for managing insurance plans
- **IInsuranceProviderService**: Service for managing insurance providers
- **BulkSupplementaryTariffService**: Service for bulk tariff operations
- **ICombinedInsuranceCalculationService**: Service for combined insurance calculations
- **IFactorSettingService**: Service for managing factor settings
- **IServiceCalculationService**: Service for service calculations

#### Repositories
- **IServiceRepository**: Repository for service data access
- **IDepartmentRepository**: Repository for department data access
- **IServiceCategoryRepository**: Repository for service category data access

#### Infrastructure Services
- **ILogger**: Logger for application logging (Serilog)
- **ICurrentUserService**: Service for current user information
- **IMessageNotificationService**: Service for message notifications
- **ISupplementaryInsuranceCacheService**: Service for caching operations

## Architecture Layers

### 1. Presentation Layer
- **Controller**: `SupplementaryTariffController`
- **Views**: 
  - `Index.cshtml` - Main listing page
  - `Create.cshtml` - Create new tariff form
  - `Edit.cshtml` - Edit existing tariff form
  - `CreateBulk.cshtml` - Bulk creation form
  - `Delete.cshtml` - Delete confirmation
  - `Details.cshtml` - Tariff details view
- **Partial Views**:
  - `_SupplementaryTariffCard.cshtml`
  - `_SupplementaryTariffFilters.cshtml`
  - `_SupplementaryTariffTable.cshtml`

### 2. Business Logic Layer
- **Services**: All services listed above
- **ViewModels**: 
  - `SupplementaryTariffCreateEditViewModel`
  - `SupplementaryTariffIndexPageViewModel`
  - `BulkSupplementaryTariffViewModel`

### 3. Data Access Layer
- **Repositories**: All repositories listed above
- **Entities**: Database entities for tariffs, services, plans, etc.

### 4. Infrastructure Layer
- **Logging**: Serilog integration
- **Caching**: Redis/Memory caching
- **Validation**: Model validation and business rules
- **Error Handling**: Centralized error management

## JavaScript Dependencies

### Core Modules
- **MedicalSupplementaryTariff**: Main module for tariff management
- **MedicalUI**: UI interaction and display management
- **MedicalAPI**: API communication layer
- **MedicalValidation**: Client-side validation
- **MedicalConfig**: Configuration management

### Helper Functions
- **MedicalHelpers**: Utility functions for code reusability
  - `safeUICall()`: Safe UI method calls
  - `safeAPICall()`: Safe API method calls
  - `safeValidationCall()`: Safe validation calls
  - `showError()`: Error message display
  - `showLoading()`: Loading indicator
  - `hideLoading()`: Hide loading indicator

## CSS Dependencies

### External Stylesheets
- **medical-environment-styles.css**: Base medical theme
- **supplementary-tariff-styles.css**: Specific tariff styles
- **supplementary-tariff-views.css**: View-specific styles (moved from inline)

### CSS Variables
- `--medical-primary`: Primary color
- `--medical-white`: White color
- `--medical-gray`: Gray color
- `--medical-border-radius`: Border radius
- `--medical-shadow`: Box shadow
- `--medical-transition`: Transition effects

## Error Handling

### Error Message Resources
- **ErrorMessages.resx**: Centralized error message resources
- **ErrorMessageHelper.cs**: Helper class for error message management

### Error Types
- System errors
- Validation errors
- Network errors
- Permission errors
- Timeout errors
- Data not found errors

## Security Features

### CSRF Protection
- Anti-forgery tokens in all forms
- CSRF token validation in AJAX requests
- `[ValidateAntiForgeryToken]` attribute on all POST actions

### Input Validation
- Server-side model validation
- Client-side JavaScript validation
- Business rule validation

## Performance Optimizations

### Caching
- Output caching on Index action (5 minutes)
- Service-level caching for frequently accessed data
- Client-side caching for static resources

### Database Optimization
- Efficient queries with proper indexing
- Pagination for large datasets
- Lazy loading where appropriate

## Testing Considerations

### Unit Testing
- Service layer testing
- Repository testing
- Controller action testing

### Integration Testing
- End-to-end workflow testing
- Database integration testing
- API integration testing

### UI Testing
- Form validation testing
- User interaction testing
- Responsive design testing

## Deployment Notes

### Configuration
- Database connection strings
- Cache configuration
- Logging configuration
- Security settings

### Dependencies
- .NET Framework 4.8
- ASP.NET MVC 5
- Entity Framework 6
- Serilog
- jQuery
- Bootstrap

## Maintenance Guidelines

### Code Organization
- Separation of concerns
- Single responsibility principle
- Dependency injection
- Clean architecture

### Documentation
- XML comments for all public methods
- Inline comments for complex logic
- README files for setup instructions
- API documentation

### Monitoring
- Application logging
- Performance monitoring
- Error tracking
- User activity monitoring

## Future Enhancements

### Planned Features
- Real-time updates
- Advanced filtering
- Export functionality
- Audit trail
- Bulk operations

### Technical Debt
- Code refactoring opportunities
- Performance improvements
- Security enhancements
- UI/UX improvements

---

*Last Updated: December 2024*
*Version: 1.0.0*
*Author: ClinicApp Medical Team*
