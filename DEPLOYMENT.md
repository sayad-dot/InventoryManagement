# Deployment Configuration Guide

This document explains how to configure the Inventory Management application for different environments (development, staging, production).

## Development Environment

For local development, the application uses **User Secrets** to store sensitive configuration data. These secrets are already configured if you're working on this project.

### Setting up User Secrets (for new developers)

If you're setting up the project for the first time, you'll need to configure user secrets:

```bash
# Navigate to the project directory
cd InventoryManagement

# Initialize user secrets (if not already done)
dotnet user-secrets init

# Set the database connection string
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=InventoryManagementDB;Username=postgres;Password=your_password;Port=5432"

# Set Google OAuth credentials
dotnet user-secrets set "Authentication:Google:ClientId" "your_google_client_id"
dotnet user-secrets set "Authentication:Google:ClientSecret" "your_google_client_secret"

# Set Facebook OAuth credentials
dotnet user-secrets set "Authentication:Facebook:AppId" "your_facebook_app_id"
dotnet user-secrets set "Authentication:Facebook:AppSecret" "your_facebook_app_secret"
```

### Viewing Current Secrets

```bash
# List all configured secrets
dotnet user-secrets list

# Clear all secrets (if needed)
dotnet user-secrets clear
```

## Production Environment

For production deployment, use **Environment Variables** instead of user secrets. The application will automatically read from environment variables when user secrets are not available.

### Required Environment Variables

Set these environment variables in your production environment:

```bash
# Database Configuration
ConnectionStrings__DefaultConnection="Host=your_prod_host;Database=InventoryManagementDB;Username=your_user;Password=your_secure_password;Port=5432"

# Google OAuth Configuration
Authentication__Google__ClientId="your_google_client_id"
Authentication__Google__ClientSecret="your_google_client_secret"

# Facebook OAuth Configuration
Authentication__Facebook__AppId="your_facebook_app_id"
Authentication__Facebook__AppSecret="your_facebook_app_secret"
```

### Azure App Service Configuration

If deploying to Azure App Service, add these in the Application Settings:

1. Go to your App Service in Azure Portal
2. Navigate to **Configuration** → **Application settings**
3. Add the following settings:

| Name | Value |
|------|-------|
| `ConnectionStrings__DefaultConnection` | `Host=your_azure_postgres;Database=InventoryManagementDB;Username=your_user;Password=your_password;Port=5432;SSL Mode=Require` |
| `Authentication__Google__ClientId` | Your Google OAuth Client ID |
| `Authentication__Google__ClientSecret` | Your Google OAuth Client Secret |
| `Authentication__Facebook__AppId` | Your Facebook App ID |
| `Authentication__Facebook__AppSecret` | Your Facebook App Secret |

### Docker Configuration

If using Docker, create a `.env` file (DO NOT commit this to Git):

```bash
# .env file (for Docker Compose)
CONNECTIONSTRINGS_DEFAULTCONNECTION=Host=postgres_db;Database=InventoryManagementDB;Username=postgres;Password=your_password;Port=5432
AUTHENTICATION_GOOGLE_CLIENTID=your_google_client_id
AUTHENTICATION_GOOGLE_CLIENTSECRET=your_google_client_secret
AUTHENTICATION_FACEBOOK_APPID=your_facebook_app_id
AUTHENTICATION_FACEBOOK_APPSECRET=your_facebook_app_secret
```

Then reference these in your `docker-compose.yml`:

```yaml
version: '3.8'
services:
  web:
    build: .
    ports:
      - "80:8080"
    environment:
      - ConnectionStrings__DefaultConnection=${CONNECTIONSTRINGS_DEFAULTCONNECTION}
      - Authentication__Google__ClientId=${AUTHENTICATION_GOOGLE_CLIENTID}
      - Authentication__Google__ClientSecret=${AUTHENTICATION_GOOGLE_CLIENTSECRET}
      - Authentication__Facebook__AppId=${AUTHENTICATION_FACEBOOK_APPID}
      - Authentication__Facebook__AppSecret=${AUTHENTICATION_FACEBOOK_APPSECRET}
```

## Security Best Practices

1. **Never commit secrets to version control**
2. **Use different OAuth applications for different environments** (dev, staging, prod)
3. **Rotate secrets regularly**
4. **Use Azure Key Vault or similar services for production secrets**
5. **Enable SSL/TLS in production**
6. **Use strong database passwords**

## OAuth Application Configuration

### Google OAuth Setup
1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select existing one
3. Enable Google+ API
4. Create OAuth 2.0 credentials
5. Add authorized redirect URIs:
   - Development: `https://localhost:5001/signin-google`
   - Production: `https://yourdomain.com/signin-google`

### Facebook OAuth Setup
1. Go to [Facebook Developers](https://developers.facebook.com/)
2. Create a new app
3. Add Facebook Login product
4. Configure Valid OAuth Redirect URIs:
   - Development: `https://localhost:5001/signin-facebook`
   - Production: `https://yourdomain.com/signin-facebook`

## Troubleshooting

### Common Issues

1. **OAuth redirects not working**: Ensure redirect URIs are correctly configured in both Google/Facebook consoles and match your domain
2. **Database connection issues**: Verify connection string format and network access
3. **Missing configuration**: Check that all required environment variables are set

### Checking Configuration

```bash
# Test if configuration is loaded correctly
dotnet run --environment Production

# Check specific configuration values (remove sensitive parts)
dotnet run -- --configuration
```

## Migration to Production

1. Set up your production OAuth applications with production URLs
2. Configure environment variables in your hosting platform
3. Update database connection strings for production database
4. Test OAuth flows with production URLs
5. Monitor logs for any configuration issues