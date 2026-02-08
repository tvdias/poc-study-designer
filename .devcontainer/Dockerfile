FROM mcr.microsoft.com/devcontainers/base:ubuntu

# Install necessary tools that might not be included via features
RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
    && apt-get -y install --no-install-recommends \
    git \
    curl \
    ca-certificates \
    && apt-get clean -y && rm -rf /var/lib/apt/lists/*

# The rest of the tools will be installed via devcontainer features
