resource "azurerm_storage_account" "funcapp-storage-account" {
  name                     = "geolocationfuncapp${var.environment}"
  resource_group_name      = azurerm_resource_group.resource-group.name
  location                 = azurerm_resource_group.resource-group.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

resource "azurerm_app_service_plan" "funcapp-service-plan" {
  name                = "geolocation-appsvcplan-${var.environment}"
  resource_group_name = azurerm_resource_group.resource-group.name
  location            = azurerm_resource_group.resource-group.location
  kind                = "FunctionApp"
  sku {
    tier = "Dynamic"
    size = "Y1"
  }
}

resource "azurerm_function_app" "function-app" {
  name                       = "geolocation-funcapp-${var.environment}"
  location                   = azurerm_resource_group.resource-group.location
  resource_group_name        = azurerm_resource_group.resource-group.name
  storage_account_name       = azurerm_storage_account.funcapp-storage-account.name
  storage_account_access_key = azurerm_storage_account.funcapp-storage-account.primary_access_key
  app_service_plan_id        = azurerm_app_service_plan.funcapp-service-plan.id

  version = "~4"

  site_config {
    dotnet_framework_version = "v6.0"
  }
}
