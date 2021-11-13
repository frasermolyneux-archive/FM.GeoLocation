provider "azurerm" { 
    version = "~> 2.85.0"
    features {}
}

terraform {
    backend "azurerm" {}
}