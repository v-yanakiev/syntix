#!/bin/bash

# Enable error reporting
set -e

echo "Starting deployment process..."

# Clean up existing dist directories
echo "Cleaning up directories..."
rm -rf dist/chrome dist/firefox

# Create dist directories
echo "Creating fresh directories..."
mkdir -p dist/chrome dist/firefox

# Build the extension for both browsers
echo "Building Chrome extension..."
npm run build:chrome

echo "Building Firefox extension..."
npm run build:firefox

# Copy manifest files
echo "Copying manifest files..."
cp manifest.json dist/chrome/
cp manifest.firefox.json dist/firefox/manifest.json


# List contents of browser-specific directories
echo "Final contents of dist/chrome:"
ls -la dist/chrome/
echo "Final contents of dist/firefox:"
ls -la dist/firefox/

echo "Deployment complete!"