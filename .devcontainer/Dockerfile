# .NET 10 SDK with development tools
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview

# Avoid prompts from apt
ENV DEBIAN_FRONTEND=noninteractive

# Install common development dependencies
RUN apt-get update && apt-get install -y --no-install-recommends \
    curl \
    wget \
    git \
    jq \
    unzip \
    sudo \
    ca-certificates \
    gnupg \
    lsb-release \
    procps \
    htop \
    vim \
    nano \
    && rm -rf /var/lib/apt/lists/*

# Create non-root user (handle case where GID/UID already exists)
ARG USERNAME=vscode
ARG USER_UID=1000
ARG USER_GID=$USER_UID

RUN if getent group $USER_GID > /dev/null 2>&1; then \
        groupmod -n $USERNAME $(getent group $USER_GID | cut -d: -f1); \
    else \
        groupadd --gid $USER_GID $USERNAME; \
    fi \
    && if getent passwd $USER_UID > /dev/null 2>&1; then \
        usermod -l $USERNAME -d /home/$USERNAME -m $(getent passwd $USER_UID | cut -d: -f1); \
    else \
        useradd --uid $USER_UID --gid $USER_GID -m $USERNAME; \
    fi \
    && echo $USERNAME ALL=\(root\) NOPASSWD:ALL > /etc/sudoers.d/$USERNAME \
    && chmod 0440 /etc/sudoers.d/$USERNAME

# Create directories for bash history and NuGet cache
RUN mkdir -p /commandhistory \
    && touch /commandhistory/.bash_history \
    && chown -R $USERNAME:$USERNAME /commandhistory

# Set up NuGet cache directory
RUN mkdir -p /home/$USERNAME/.nuget \
    && chown -R $USERNAME:$USERNAME /home/$USERNAME/.nuget

# Install .NET workloads and tools
RUN dotnet workload update \
    && dotnet tool install --global dotnet-ef \
    && dotnet tool install --global dotnet-outdated-tool \
    && dotnet tool install --global dotnet-format

# Add dotnet tools to PATH
ENV PATH="${PATH}:/home/${USERNAME}/.dotnet/tools:/root/.dotnet/tools"

# Configure bash history
RUN echo 'export HISTFILE=/commandhistory/.bash_history' >> /home/$USERNAME/.bashrc \
    && echo 'export PROMPT_COMMAND="history -a"' >> /home/$USERNAME/.bashrc

# Set working directory
WORKDIR /workspace

# Switch to non-root user
USER $USERNAME

# Trust dev certificates
RUN dotnet dev-certs https --trust 2>/dev/null || true
