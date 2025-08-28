# API Testing Suite Documentation

This directory contains comprehensive HTTP test files for all RadiusConnect API endpoints. These tests are designed to validate API functionality before frontend integration.

## 📁 Test Files Overview

### Core Test Files
- **`api-tests-master.http`** - Master configuration with environment variables and common setup
- **`api-tests-auth.http`** - Authentication and authorization endpoints (8 endpoints, 25+ test scenarios)
- **`api-tests-users.http`** - User management endpoints (10 endpoints, 50+ test scenarios)
- **`api-tests-dashboard.http`** - Dashboard and statistics endpoints (6 endpoints, 30+ test scenarios)
- **`api-tests-radius.http`** - RADIUS management endpoints (18 endpoints, 100+ test scenarios)
- **`api-tests-audit.http`** - Audit logging endpoints (4 endpoints, 70+ test scenarios)

## 🚀 Getting Started

### Prerequisites
1. **API Server Running**: Ensure your RadiusConnect API is running (typically on `https://localhost:7001`)
2. **HTTP Client**: Use VS Code with REST Client extension, or any HTTP client that supports `.http` files
3. **Database**: Ensure your database is properly configured and accessible

### Setup Instructions

1. **Configure Environment Variables**
   - Open `api-tests-master.http`
   - Update the `@baseUrl` variable to match your API server URL
   - Verify other environment variables match your setup

2. **Authentication Setup**
   - Run authentication tests first to obtain valid tokens
   - Copy the returned JWT tokens from login responses
   - Update token variables in each test file:
     ```
     @adminToken = Bearer your_actual_admin_token_here
     @managerToken = Bearer your_actual_manager_token_here
     @userToken = Bearer your_actual_user_token_here
     ```

3. **Test Data Preparation**
   - Review and update test data variables in each file
   - Ensure test usernames, group names, and IDs don't conflict with existing data

## 📋 Test Execution Order

### Recommended Testing Sequence:

1. **Start with Master Configuration**
   ```
   api-tests-master.http → Health Check
   ```

2. **Authentication Tests (Required First)**
   ```
   api-tests-auth.http → Register → Login → Get Tokens
   ```

3. **Core Functionality Tests**
   ```
   api-tests-users.http → User Management
   api-tests-dashboard.http → Statistics & Monitoring
   api-tests-radius.http → RADIUS Operations
   api-tests-audit.http → Audit Logging
   ```

## 🔐 Authentication & Authorization

### User Roles and Permissions

| Role | Auth | Users | Dashboard | RADIUS | Audit |
|------|------|-------|-----------|--------|---------|
| **Admin** | ✅ Full | ✅ Full | ✅ Full | ✅ Full | ✅ Full |
| **Manager** | ✅ Limited | ✅ Limited | ✅ Full | ✅ Full | ✅ Read-only |
| **User** | ✅ Own Profile | ❌ Denied | ✅ Limited | ❌ Denied | ❌ Denied |

### Token Management
- **Access Tokens**: Valid for 15 minutes (configurable)
- **Refresh Tokens**: Valid for 7 days (configurable)
- **Token Refresh**: Use refresh token to get new access token
- **Token Revocation**: Logout invalidates both tokens

## 📊 Test Categories

### 1. Authentication Tests (`api-tests-auth.http`)
- **User Registration**: Account creation with validation
- **User Login**: Credential verification and token generation
- **Token Management**: Refresh and logout operations
- **Password Operations**: Change, forgot, and reset functionality
- **Profile Management**: Current user information retrieval

**Key Test Scenarios:**
- Valid/invalid credentials
- Password strength validation
- TOTP (2FA) authentication
- Token expiration handling
- Rate limiting protection

### 2. User Management Tests (`api-tests-users.http`)
- **CRUD Operations**: Create, read, update, delete users
- **Role Management**: Assign and remove user roles
- **Account Status**: Activate and deactivate users
- **TOTP Management**: Setup, enable, and disable 2FA

**Key Test Scenarios:**
- User creation with different roles
- Email uniqueness validation
- Password complexity requirements
- Role-based access control
- TOTP setup and verification

### 3. Dashboard Tests (`api-tests-dashboard.http`)
- **Overview Statistics**: System-wide metrics
- **System Health**: Server and database status
- **Session Analytics**: User session statistics
- **Authentication Metrics**: Login success/failure rates
- **Time-based Reports**: Hourly and daily statistics

**Key Test Scenarios:**
- Real-time statistics accuracy
- Date range filtering
- Performance with large datasets
- Role-based data visibility
- Cache behavior validation

### 4. RADIUS Management Tests (`api-tests-radius.http`)
- **RADIUS Users**: Create and manage RADIUS accounts
- **User Attributes**: RADIUS attribute management
- **Groups**: RADIUS group operations
- **Group Membership**: User-group relationships
- **Active Sessions**: Session monitoring and management
- **Authentication Logs**: RADIUS auth attempt tracking
- **Statistics**: RADIUS-specific analytics

**Key Test Scenarios:**
- RADIUS user lifecycle management
- Attribute validation and assignment
- Group-based policy enforcement
- Session disconnection capabilities
- Authentication log analysis

### 5. Audit Logging Tests (`api-tests-audit.http`)
- **Audit Log Retrieval**: System activity tracking
- **Filtering**: By actor, entity, action, date range
- **Search Capabilities**: Complex query combinations
- **Performance**: Large dataset handling

**Key Test Scenarios:**
- Comprehensive activity logging
- Advanced filtering options
- Performance with large log volumes
- Data retention compliance
- Security event tracking

## 🔍 Expected HTTP Status Codes

### Success Responses
- **200 OK**: Successful GET, PUT operations
- **201 Created**: Successful POST operations
- **204 No Content**: Successful DELETE operations

### Client Error Responses
- **400 Bad Request**: Invalid request data or parameters
- **401 Unauthorized**: Missing or invalid authentication
- **403 Forbidden**: Insufficient permissions
- **404 Not Found**: Resource doesn't exist
- **409 Conflict**: Resource already exists or conflict
- **422 Unprocessable Entity**: Validation errors

### Server Error Responses
- **500 Internal Server Error**: Unexpected server errors
- **503 Service Unavailable**: Service temporarily unavailable

## 📝 Response Format

All API responses follow a consistent JSON structure:

```json
{
  "success": true,
  "data": {
    // Response data here
  },
  "message": "Operation completed successfully",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### Error Response Format
```json
{
  "success": false,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Validation failed",
    "details": [
      {
        "field": "email",
        "message": "Email is required"
      }
    ]
  },
  "timestamp": "2024-01-15T10:30:00Z"
}
```

## 🛡️ Security Testing

### Included Security Tests
- **SQL Injection**: Parameter injection attempts
- **XSS Prevention**: Cross-site scripting protection
- **Path Traversal**: Directory traversal attempts
- **Authentication Bypass**: Unauthorized access attempts
- **Token Manipulation**: Invalid token handling
- **Rate Limiting**: Brute force protection

### Security Expectations
- All injection attempts should be safely handled
- Proper input validation and sanitization
- Secure token handling and validation
- Appropriate error messages (no sensitive data leakage)

## 🚨 Common Issues & Troubleshooting

### Authentication Issues
- **Invalid Token**: Ensure tokens are current and properly formatted
- **Token Expiration**: Refresh tokens when access tokens expire
- **Role Permissions**: Verify user has required role for endpoint

### Database Issues
- **Connection Errors**: Check database connectivity
- **Migration Status**: Ensure database schema is up to date
- **Test Data Conflicts**: Use unique test data to avoid conflicts

### Performance Issues
- **Slow Responses**: Check database indexing and query optimization
- **Memory Usage**: Monitor server resources during large dataset tests
- **Timeout Errors**: Adjust timeout settings for long-running operations

## 📈 Performance Benchmarks

### Expected Response Times
- **Authentication**: < 200ms
- **Simple CRUD**: < 100ms
- **Complex Queries**: < 500ms
- **Statistics/Reports**: < 1000ms
- **Large Dataset Operations**: < 2000ms

### Load Testing Recommendations
- Test with realistic user loads (10-100 concurrent users)
- Validate performance under stress conditions
- Monitor database connection pooling
- Check memory usage and garbage collection

## 🔧 Customization

### Adding New Tests
1. Follow existing naming conventions
2. Include both positive and negative test cases
3. Test different user roles and permissions
4. Add appropriate comments and documentation
5. Update this README with new test information

### Environment Configuration
- Modify variables in `api-tests-master.http`
- Create environment-specific test files if needed
- Use different base URLs for different environments

## 📞 Support

For issues or questions regarding the API testing suite:
1. Check the troubleshooting section above
2. Review API documentation for endpoint specifications
3. Verify database schema and migrations
4. Check server logs for detailed error information

---

**Note**: Always run tests in a development or testing environment. Never run these tests against production data without proper safeguards and backups.