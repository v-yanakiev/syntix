#!/bin/bash

# Set variables
IMAGE_NAME="vasil2000yanakiev/elk"
TAG="latest"

# Print start message
echo "Starting Docker build and push process..."

# Build the Docker image
echo "Building Docker image..."
docker build -t $IMAGE_NAME:$TAG .

# Check if build was successful
if [ $? -eq 0 ]; then
    echo "Docker image built successfully"
else
    echo "Docker build failed"
    exit 1
fi

# Push the image to Docker Hub
echo "Pushing image to Docker Hub..."
docker push $IMAGE_NAME:$TAG

# Check if push was successful
if [ $? -eq 0 ]; then
    echo "Docker image pushed successfully"
else
    echo "Docker push failed"
    exit 1
fi

echo "Process completed successfully!"

