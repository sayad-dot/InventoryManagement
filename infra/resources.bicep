@description('The location used for all deployed resources')
param location string = resourceGroup().location

@description('Tags that will be applied to all resources')
param tags object = {}


param inventoryManagementExists bool

@description('Id of the user or app to assign application roles')
param principalId string

@description('Principal type of user or app')
param principalType string

var abbrs = loadJsonContent('./abbreviations.json')
var resourceToken = uniqueString(subscription().id, resourceGroup().id, location)

// PostgreSQL admin password (will be provided as parameter or generated)
@secure()
param postgresAdminPassword string

// Google OAuth Configuration
param googleClientId string = ''
@secure()
param googleClientSecret string = ''

// Facebook OAuth Configuration
param facebookAppId string = ''
@secure()
param facebookAppSecret string = ''

// PostgreSQL Database
module postgresDB './modules/postgres.bicep' = {
  name: 'postgres-database'
  params: {
    location: location
    tags: tags
    serverName: '${abbrs.dBforPostgreSQLServers}${resourceToken}'
    administratorLogin: 'pgadmin'
    administratorLoginPassword: postgresAdminPassword
    databaseName: 'inventorydb'
    skuName: 'Standard_B1ms'  // Burstable tier - good for dev/test
    tier: 'Burstable'
    storageSizeGB: 32
    version: '16'
  }
}

// Monitor application with Azure Monitor
module monitoring 'br/public:avm/ptn/azd/monitoring:0.1.0' = {
  name: 'monitoring'
  params: {
    logAnalyticsName: '${abbrs.operationalInsightsWorkspaces}${resourceToken}'
    applicationInsightsName: '${abbrs.insightsComponents}${resourceToken}'
    applicationInsightsDashboardName: '${abbrs.portalDashboards}${resourceToken}'
    location: location
    tags: tags
  }
}
// Container registry
module containerRegistry 'br/public:avm/res/container-registry/registry:0.1.1' = {
  name: 'registry'
  params: {
    name: '${abbrs.containerRegistryRegistries}${resourceToken}'
    location: location
    tags: tags
    publicNetworkAccess: 'Enabled'
    roleAssignments:[
      {
        principalId: inventoryManagementIdentity.outputs.principalId
        principalType: 'ServicePrincipal'
        roleDefinitionIdOrName: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
      }
    ]
  }
}

// Container apps environment
module containerAppsEnvironment 'br/public:avm/res/app/managed-environment:0.4.5' = {
  name: 'container-apps-environment'
  params: {
    logAnalyticsWorkspaceResourceId: monitoring.outputs.logAnalyticsWorkspaceResourceId
    name: '${abbrs.appManagedEnvironments}${resourceToken}'
    location: location
    zoneRedundant: false
  }
}

module inventoryManagementIdentity 'br/public:avm/res/managed-identity/user-assigned-identity:0.2.1' = {
  name: 'inventoryManagementidentity'
  params: {
    name: '${abbrs.managedIdentityUserAssignedIdentities}inventoryManagement-${resourceToken}'
    location: location
  }
}
module inventoryManagementFetchLatestImage './modules/fetch-container-image.bicep' = {
  name: 'inventoryManagement-fetch-image'
  params: {
    exists: inventoryManagementExists
    name: 'inventory-management'
  }
}

module inventoryManagement 'br/public:avm/res/app/container-app:0.8.0' = {
  name: 'inventoryManagement'
  params: {
    name: 'inventory-management'
    ingressTargetPort: 8080
    scaleMinReplicas: 1
    scaleMaxReplicas: 10
    secrets: {
      secureList:  [
      ]
    }
    containers: [
      {
        image: inventoryManagementFetchLatestImage.outputs.?containers[?0].?image ?? 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'
        name: 'main'
        resources: {
          cpu: json('0.5')
          memory: '1.0Gi'
        }
        env: [
          {
            name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
            value: monitoring.outputs.applicationInsightsConnectionString
          }
          {
            name: 'AZURE_CLIENT_ID'
            value: inventoryManagementIdentity.outputs.clientId
          }
          {
            name: 'PORT'
            value: '8080'
          }
          {
            name: 'ConnectionStrings__DefaultConnection'
            value: postgresDB.outputs.connectionString
          }
          {
            name: 'ASPNETCORE_ENVIRONMENT'
            value: 'Production'
          }
          {
            name: 'Authentication__Google__ClientId'
            value: googleClientId
          }
          {
            name: 'Authentication__Google__ClientSecret'
            value: googleClientSecret
          }
          {
            name: 'Authentication__Facebook__AppId'
            value: facebookAppId
          }
          {
            name: 'Authentication__Facebook__AppSecret'
            value: facebookAppSecret
          }
        ]
      }
    ]
    managedIdentities:{
      systemAssigned: false
      userAssignedResourceIds: [inventoryManagementIdentity.outputs.resourceId]
    }
    registries:[
      {
        server: containerRegistry.outputs.loginServer
        identity: inventoryManagementIdentity.outputs.resourceId
      }
    ]
    environmentResourceId: containerAppsEnvironment.outputs.resourceId
    location: location
    tags: union(tags, { 'azd-service-name': 'inventory-management' })
  }
}
output AZURE_CONTAINER_REGISTRY_ENDPOINT string = containerRegistry.outputs.loginServer
output AZURE_RESOURCE_INVENTORY_MANAGEMENT_ID string = inventoryManagement.outputs.resourceId
output POSTGRES_SERVER_NAME string = postgresDB.outputs.serverName
output POSTGRES_SERVER_FQDN string = postgresDB.outputs.serverFqdn
output POSTGRES_DATABASE_NAME string = postgresDB.outputs.databaseName
