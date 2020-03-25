resource "azurerm_resource_group" "resource-group" {
    name = "FM-GeoLocation-${var.environment}"
    location = "${var.region}"
}

resource "azurerm_storage_account" "function-app-storage-account" {
    name                     = "funcappsa${var.environment}"
    resource_group_name      = "${azurerm_resource_group.resource-group.name}"
    location                 = "${azurerm_resource_group.resource-group.location}"
    account_tier             = "Standard"
    account_replication_type = "LRS"
}

resource "azurerm_app_service_plan" "app-service-plan" {
    name = "GeoLocation-AppPlan-${var.environment}"
    resource_group_name = "${azurerm_resource_group.resource-group.name}"
    location = "${azurerm_resource_group.resource-group.location}"
    kind = "FunctionApp"
    sku {
        tier = "Dynamic"
        size = "Y1"
    }
}

resource "azurerm_function_app" "geolocation-function-app" {
    name = "geolocation-funcapp-${var.environment}"
    location = "${azurerm_resource_group.resource-group.location}"
    resource_group_name = "${azurerm_resource_group.resource-group.name}"
    storage_connection_string = "${azurerm_storage_account.function-app-storage-account.primary_connection_string}"
    app_service_plan_id = "${azurerm_app_service_plan.app-service-plan.id}"
    version = "~3"

    app_settings = {
        "Storage:TableStorageConnectionString" = "${azurerm_storage_account.geolocation-storage.primary_connection_string}"
        "MaxMind:UserId" = "${var.MaxMindUserId}"
        "MaxMind:ApiKey" = "${var.MaxMindApiKey}"
    }
}