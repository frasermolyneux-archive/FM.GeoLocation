resource "azurerm_storage_account" "geolocation-storage" {
    name = "geolocationsa${var.environment}"
    resource_group_name = "${azurerm_resource_group.resource-group.name}"
    location = "${azurerm_resource_group.resource-group.location}"
    account_tier = "Standard"
    account_replication_type = "LRS"
}

resource "azurerm_storage_table" "geolocation-table" {
  name = "locations"
  resource_group_name  = "${azurerm_resource_group.resource-group.name}"
  storage_account_name = "${azurerm_storage_account.geolocation-storage.name}"
}