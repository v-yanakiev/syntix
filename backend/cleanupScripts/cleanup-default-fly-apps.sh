#!/bin/bash

# Function to validate if a string is a GUID starting with "a"
is_a_guid() {
    local guid_pattern='^a[0-9a-fA-F]{7}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$'
    if [[ $1 =~ $guid_pattern ]]; then
        return 0
    else
        return 1
    fi
}

# Get all apps and store them in an array
echo "Fetching all fly.io apps..."
apps=$(fly apps list --json | jq -r '.[].Name')

# Counter for deleted apps
deleted_count=0

# Iterate through each app
for app in $apps; do
    if is_a_guid "$app"; then
        echo "Found GUID app starting with 'a': $app"
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

echo "Clean up complete. Deleted $deleted_count apps with GUID names starting with 'a'."

