# 📦 Inventory Management System

A powerful, flexible web application for managing custom inventories with real-time collaboration, customizable fields, and advanced search capabilities. Built with modern ASP.NET Core MVC architecture and designed for scalability and user experience.

## 🚀 Overview

This inventory management system solves the rigidity problem of traditional inventory software by providing complete flexibility in how you structure and manage your data. Unlike systems that force predefined categories, this application allows users to create fully customized inventory types with any combination of field types, collaborate in real-time, and maintain full control over access permissions.

## ✨ Key Features

### 🎯 **Flexible Custom Fields**

- **5 Field Types**: String (single-line), Text (multi-line), Number (decimal), Boolean (checkbox), File/Image URL
- **3 Fields Per Type**: Up to 15 custom fields per inventory
- **Dynamic Configuration**: Enable/disable fields as needed
- **Field Ordering**: Customizable display order for optimal workflow

### 👥 **Multi-User Collaboration**

- **Access Control**: Public/private inventories with granular user permissions
- **Role Management**: Creators can grant access to specific users
- **Real-time Updates**: Live synchronization across all connected users
- **User Management**: Complete admin panel for user oversight

### 💬 **Real-time Communication**

- **Inventory Discussions**: Built-in chat for each inventory using SignalR
- **Live Updates**: Instant message delivery and status updates
- **Discussion History**: Persistent conversation threads
- **User Presence**: See who's currently viewing/editing

### 🔍 **Advanced Search & Discovery**

- **Full-text Search**: Search across all inventories, items, and custom fields
- **Cross-inventory Search**: Find items across multiple inventories simultaneously
- **Intelligent Filtering**: Category-based and tag-based filtering
- **Search Statistics**: Track popular searches and optimize discovery

### 🎨 **Modern User Experience**

- **Responsive Design**: Bootstrap 5-based UI that works on all devices
- **Intuitive Interface**: Clean, professional design with excellent UX
- **Bulk Operations**: Checkbox selection with toolbar actions
- **Progressive Enhancement**: Works without JavaScript, enhanced with it

### 🔐 **Security & Authentication**

- **Multiple Auth Methods**: ASP.NET Core Identity, Google OAuth, Facebook OAuth
- **Secure by Design**: CSRF protection, secure headers, input validation
- **Privacy Controls**: Fine-grained access control for sensitive inventories
- **Admin Dashboard**: Complete user and system management

### 📊 **Analytics & Insights**

- **Inventory Statistics**: View counts, item counts, user engagement metrics
- **Like System**: Users can like items with aggregate statistics
- **Usage Analytics**: Track popular inventories and user activity
- **Performance Metrics**: Optimized queries with no N+1 problems

### ⚡ **Technical Excellence**

- **Optimistic Locking**: Prevent data conflicts in multi-user scenarios
- **Clean Architecture**: Service-based design with clear separation of concerns
- **Efficient Database**: PostgreSQL with Entity Framework Core, optimized queries
- **Scalable Design**: Built for growth with proper indexing and caching strategies

## 🛠️ Technology Stack

### Backend

- **Framework**: ASP.NET Core 8.0 MVC
- **Language**: C# with nullable reference types enabled
- **Architecture**: Clean architecture with service layer pattern
- **Database**: PostgreSQL 16+ with Entity Framework Core 8.0
- **Real-time**: SignalR for live communications
- **Authentication**: ASP.NET Core Identity with OAuth 2.0 (Google, Facebook)

### Frontend

- **UI Framework**: Bootstrap 5.3+ for responsive design
- **JavaScript**: Vanilla JavaScript with jQuery for enhanced interactivity
- **Templating**: Razor Views with strongly-typed models
- **Icons**: Bootstrap Icons and emoji for visual appeal
- **Styling**: Custom CSS with modern design principles

### Development & Deployment

- **Package Management**: NuGet packages with dependency management
- **Migration System**: Entity Framework Code-First migrations
- **Cloud Ready**: Azure Container Apps deployment configuration
- **Infrastructure as Code**: Bicep templates for Azure resources
- **Security**: HTTPS, CSRF protection, secure headers, input validation

### Key Dependencies

- **Npgsql.EntityFrameworkCore.PostgreSQL** - PostgreSQL provider for EF Core
- **Microsoft.AspNetCore.SignalR** - Real-time web functionality
- **Microsoft.AspNetCore.Identity** - User management and authentication
- **System.Linq.Dynamic.Core** - Dynamic LINQ for flexible queries
- **Humanizer.Core** - Human-friendly text formatting
- **Newtonsoft.Json** - JSON serialization for complex data structures

## 📋 Prerequisites

- **[.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)** - Required for building and running the application
- **[PostgreSQL 16+](https://www.postgresql.org/download/)** - Primary database (or compatible version)
- **[Git](https://git-scm.com/downloads)** - For version control and deployment
- **[Visual Studio Code](https://code.visualstudio.com/)** or **[Visual Studio](https://visualstudio.microsoft.com/)** - Recommended IDEs

### Optional for Cloud Deployment

- **[Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli)** - For Azure deployments
- **[Azure Developer CLI (azd)](https://aka.ms/install-azd)** - For streamlined Azure deployment
- **[Docker](https://docker.com/get-started)** - For containerized deployments

## 🚀 Quick Start Guide

### 1. Clone and Navigate

```bash
git clone https://github.com/sayad-dot/InventoryManagement.git
cd InventoryManagement
```

### 2. Database Configuration

Create or edit `appsettings.Development.json` with your PostgreSQL connection:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=InventoryDB;Username=postgres;Password=your_secure_password"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

**🔐 Security Note**: Never commit passwords to source control. Use environment variables or Azure Key Vault for production.

### 3. Install Dependencies & Setup Database

```bash
# Restore NuGet packages
dotnet restore

# Create and migrate database
dotnet ef database update

# Optional: Add sample data (if seeding is configured)
dotnet run --seed-data
```

### 4. Configure OAuth (Optional)

For social authentication, add to your `appsettings.Development.json`:

```json
{
  "Authentication": {
    "Google": {
      "ClientId": "your-google-client-id",
      "ClientSecret": "your-google-client-secret"
    },
    "Facebook": {
      "AppId": "your-facebook-app-id",
      "AppSecret": "your-facebook-app-secret"
    }
  }
}
```

### 5. Run the Application

```bash
dotnet run
```

🌐 **Default URLs:**

- **HTTPS**: [https://localhost:7290](https://localhost:7290)
- **HTTP**: [http://localhost:5238](http://localhost:5238)

The application will automatically open in your default browser!

## 📁 Project Structure

```text
InventoryManagement/
├── 📁 Controllers/          # MVC Controllers (API endpoints and page routing)
│   ├── AccountController.cs     # User authentication and registration
│   ├── AdminController.cs       # Admin panel and user management
│   ├── HomeController.cs        # Landing page and public routes
│   ├── InventoryController.cs   # Core inventory CRUD operations
│   ├── ProfileController.cs     # User profile management
│   └── SearchController.cs      # Search functionality
├── 📁 Data/                 # Database context and configuration
│   └── ApplicationDbContext.cs  # EF Core DbContext with entity configuration
├── 📁 Models/               # Domain models and entities
│   ├── ApplicationUser.cs       # Extended Identity user model
│   ├── Inventory.cs            # Core inventory entity with custom fields
│   ├── Item.cs                 # Item entity with flexible custom fields
│   ├── Discussion.cs           # Real-time chat messages
│   └── ...                     # Other domain models
├── 📁 ViewModels/           # Data transfer objects for views
├── 📁 Services/             # Business logic and service layer
│   ├── ISearchService.cs        # Full-text search abstraction
│   ├── IAccessControlService.cs # Permission management
│   ├── IStatisticsService.cs    # Analytics and reporting
│   └── ...                     # Other service interfaces and implementations
├── 📁 Views/                # Razor view templates
│   ├── 📁 Inventory/           # Inventory-related views
│   ├── 📁 Shared/              # Shared layout and partial views
│   └── ...                     # Other view folders
├── 📁 wwwroot/              # Static web assets
│   ├── 📁 css/                 # Custom stylesheets
│   ├── 📁 js/                  # Client-side JavaScript
│   └── 📁 lib/                 # Third-party libraries
├── 📁 Migrations/           # Entity Framework database migrations
├── 📁 infra/                # Azure deployment infrastructure
│   ├── main.bicep              # Azure resources definition
│   └── main.parameters.json    # Deployment parameters
└── 📁 Hubs/                 # SignalR hubs for real-time features
    └── DiscussionHub.cs        # Real-time chat hub
```

## 💡 Usage Guide

### Creating Your First Inventory

1. **Register/Login**: Create an account or sign in with Google/Facebook
2. **Create Inventory**: Click "Create New Inventory" from the dashboard
3. **Configure Fields**: Set up custom fields for your specific needs
4. **Add Items**: Start adding items with your custom field values
5. **Collaborate**: Invite users or make inventory public for collaboration

### Custom Field Configuration

The system supports 5 types of custom fields, with up to 3 of each type:

| Field Type | Use Case | Example |
|------------|----------|---------|
| **String** | Short text values | Product codes, names, categories |
| **Text** | Long descriptions | Detailed notes, specifications |
| **Number** | Numerical values | Prices, quantities, weights |
| **Boolean** | Yes/No values | In stock, needs repair, featured |
| **File/Image** | URLs to files | Product photos, manuals, certificates |

### Real-time Collaboration

- **Live Discussions**: Click the chat icon on any inventory to start discussions
- **User Presence**: See who's currently viewing/editing inventories
- **Instant Updates**: Changes appear immediately for all connected users
- **Notifications**: Get notified of new messages and updates

### Advanced Search

Use the global search to find content across all accessible inventories:

- **Simple Search**: `laptop` - finds items containing "laptop"
- **Field-Specific**: Search within custom fields and categories
- **Cross-Inventory**: Results span multiple inventories you have access to
- **Filters**: Use category and tag filters to narrow results

## 🔧 Development Guide

### Database Migrations

When modifying models, create and apply migrations:

```bash
# Add a new migration
dotnet ef migrations add DescriptiveMigrationName

# Apply pending migrations
dotnet ef database update

# Rollback to specific migration
dotnet ef database update PreviousMigrationName

# Remove last migration (if not applied)
dotnet ef migrations remove
```

### Adding New Features

1. **Model Changes**: Update entities in `Models/`
2. **Database**: Create migration with `dotnet ef migrations add`
3. **Service Layer**: Add business logic to `Services/`
4. **Controller**: Add endpoints to appropriate controller
5. **Views**: Create Razor templates in `Views/`
6. **Tests**: Add unit/integration tests

## 🌐 Deployment Options

### Azure Container Apps (Recommended)

The project includes Azure infrastructure-as-code for seamless cloud deployment:

```bash
# Install Azure Developer CLI
curl -fsSL https://aka.ms/install-azd.sh | bash

# Login to Azure
azd auth login

# Initialize and deploy
azd init
azd up
```

This will:

- Create Azure Container Apps environment
- Set up PostgreSQL Flexible Server
- Configure networking and security
- Deploy your application with auto-scaling

### Manual Docker Deployment

```bash
# Build the application
dotnet publish -c Release -o out

# Create Docker image
docker build -t inventory-management:latest .

# Run with environment variables
docker run -d \
  -p 80:8080 \
  -e "ConnectionStrings__DefaultConnection=Host=your-db;Database=inventory;Username=user;Password=pass" \
  -e "ASPNETCORE_ENVIRONMENT=Production" \
  inventory-management:latest
```

### Environment Configuration

| Environment Variable | Description | Example |
|---------------------|-------------|---------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string | `Host=db.example.com;Database=inventory;Username=user;Password=***` |
| `ASPNETCORE_ENVIRONMENT` | Deployment environment | `Production`, `Staging`, `Development` |
| `Authentication__Google__ClientId` | Google OAuth client ID | `123456789-abc.apps.googleusercontent.com` |
| `Authentication__Google__ClientSecret` | Google OAuth secret | `GOCSPX-***` |
| `Authentication__Facebook__AppId` | Facebook app ID | `123456789012345` |
| `Authentication__Facebook__AppSecret` | Facebook app secret | `***` |

## 🚨 Troubleshooting

### Database Issues

**Migration Errors:**

```bash
# Reset database (⚠️ DESTRUCTIVE - only for development)
dotnet ef database drop
dotnet ef database update

# Check migration status
dotnet ef migrations list
```

**Connection Issues:**

- Verify PostgreSQL is running: `pg_isready -h localhost -p 5432`
- Check firewall settings and network connectivity
- Validate connection string format and credentials
- Ensure database exists and user has proper permissions

### Application Errors

**Port Already in Use:**

```bash
# Kill process using port 5238
sudo lsof -ti:5238 | xargs kill -9

# Or use different port
dotnet run --urls "https://localhost:7291;http://localhost:5239"
```

**Missing Dependencies:**

```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore packages
dotnet restore --force
```

**Authentication Issues:**

- Verify OAuth credentials in configuration
- Check redirect URIs in Google/Facebook developer consoles
- Ensure HTTPS is enabled for OAuth callbacks

### Performance Optimization

**Database Performance:**

- Monitor slow queries with PostgreSQL logging
- Add indexes for frequently searched custom fields
- Use connection pooling for high-traffic scenarios

**Application Performance:**

- Enable response compression in production
- Configure caching for static assets
- Monitor with Application Insights (Azure) or similar APM tools

### Development Tips

**Hot Reload Issues:**

```bash
# Restart with clean build
dotnet clean
dotnet build
dotnet run
```

**Database Schema Conflicts:**

- Always backup before major migrations
- Test migrations on copy of production data
- Use migration bundles for production deployments

## 🤝 Contributing

We welcome contributions! Here's how you can help improve the project:

### Development Process

1. **Fork & Clone**: Fork the repository and clone your fork
2. **Branch**: Create a feature branch (`git checkout -b feature/amazing-feature`)
3. **Develop**: Make your changes with proper testing
4. **Test**: Run tests and ensure everything works (`dotnet test`)
5. **Commit**: Use conventional commits (`feat: add custom field sorting`)
6. **Push**: Push to your fork (`git push origin feature/amazing-feature`)
7. **PR**: Open a Pull Request with detailed description

### Contribution Guidelines

- **Code Style**: Follow C# and ASP.NET Core conventions
- **Documentation**: Update README and inline docs for new features
- **Testing**: Add unit tests for new functionality
- **Security**: Follow security best practices, never commit secrets
- **Performance**: Consider performance impact of changes

### Areas for Contribution

- 🎨 **UI/UX Improvements**: Enhanced user interface and experience
- 🔍 **Search Enhancement**: Advanced search filters and algorithms
- 📊 **Analytics**: More detailed statistics and reporting
- 🌍 **Internationalization**: Multi-language support expansion
- 📱 **Mobile Experience**: Mobile-specific optimizations
- 🔒 **Security Features**: Additional security hardening
- ⚡ **Performance**: Query optimization and caching improvements
- 🧪 **Testing**: Unit tests, integration tests, and automation

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

### MIT License Summary

- ✅ Commercial use allowed
- ✅ Modification allowed  
- ✅ Distribution allowed
- ✅ Private use allowed
- ❌ No warranty provided
- ❌ No liability accepted

## 👨‍💻 Author & Acknowledgments

**Created by:** [Sayad Ibna Azad](https://github.com/sayad-dot)

### Acknowledgments

- **ASP.NET Core Team** - For the excellent framework
- **Bootstrap Team** - For the responsive UI framework  
- **PostgreSQL Community** - For the robust database system
- **SignalR Team** - For real-time functionality
- **Entity Framework Team** - For the ORM that powers our data layer
- **Open Source Community** - For the countless libraries that make this possible

---

## 🚀 Ready to Get Started?

1. **Star this repository** ⭐ if you find it useful
2. **Follow the Quick Start Guide** above to get running in minutes
3. **Join the discussion** by opening issues for questions or suggestions
4. **Contribute** to make this project even better

### 🔗 Useful Links

- **[Live Demo](https://your-demo-url.azurecontainerapps.io)** - See it in action
- **[Documentation](https://github.com/sayad-dot/InventoryManagement/wiki)** - Detailed guides
- **[Issues](https://github.com/sayad-dot/InventoryManagement/issues)** - Bug reports and feature requests
- **[Discussions](https://github.com/sayad-dot/InventoryManagement/discussions)** - Community conversations

Happy inventory managing! 📦✨


