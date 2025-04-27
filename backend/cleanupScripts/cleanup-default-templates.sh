#!/bin/bash

# Function to check if app name starts with any of the defined prefixes
starts_with_prefix() {
    local app_name=$1
    local prefixes=("asdf" "environmentdefining" "postgresql" "nodejs" "rust" "nodets" "go" "csharp" "python" "java")
    
    for prefix in "${prefixes[@]}"; do
        if [[ $app_name == $prefix* ]]; then
            return 0
        fi
    done
    return 1
}

# Get all apps and store them in an array
echo "Fetching all fly.io apps..."
apps=$(fly apps list --json | jq -r '.[].Name')

# Counter for deleted apps
deleted_count=0

# Iterate through each app
for app in $apps; do
    if starts_with_prefix "$app"; then
        echo "Found app with matching prefix: $app"
        echo "Deleting app..."

        # Delete the app with -y flag to automatically confirm
        if fly apps destroy "$app" -y; then
            echo "Successfully deleted app: $app"
            ((deleted_count++))
        else
            echo "Failed to delete app: $app"
        fi
        
        # Add a small delay to prevent rate limiting
        sleep 1
    fi
done

echo "Clean up complete. Deleted $deleted_count apps with matching prefixes."

