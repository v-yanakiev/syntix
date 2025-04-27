FROM debian:12-slim

# Install required packages
RUN apt-get update && \
    apt-get install -y \
    curl \
    bash \
    ca-certificates \
    && rm -rf /var/lib/apt/lists/*

# Install jbang
RUN curl -Ls https://sh.jbang.dev | JBANG_DEFAULT_JAVA_VERSION=21 bash -s - app setup

# Add jbang to PATH
ENV PATH="/root/.jbang/bin:${PATH}"

# Verify installation
RUN jbang --version
