resource "azurerm_resource_group" "resource-group" {
  name = "fm-geolocation-${var.environment}"
  location = var.region
}

resource "azurerm_storage_account" "appdata-storage" {
  name = "geolocationappdata${var.environment}"
  resource_group_name = azurerm_resource_group.resource-group.name
  location = azurerm_resource_group.resource-group.location
  account_tier = "Standard"
  account_replication_type = "LRS"
}

resource "azurerm_storage_table" "locations-table" {
  name = "locations"
  storage_account_name = azurerm_storage_account.appdata-storage.name
}

output "appdata_storage_connection" {
  value = azurerm_storage_account.appdata-storage.primary_connection_string
  sensitive = true
}

resource "azurerm_application_insights" "app-insights" {
  name = "fm-geolocation-appinsights-${var.environment}"
  location = azurerm_resource_group.resource-group.location
  resource_group_name = azurerm_resource_group.resource-group.name
  application_type = "web"
  daily_data_cap_in_gb = 10
  retention_in_days = 30
  disable_ip_masking = true
}

output "instrumentation_key" {
  value = azurerm_application_insights.app-insights.instrumentation_key
}

output "app_id" {
  value = azurerm_application_insights.app-insights.app_id
}