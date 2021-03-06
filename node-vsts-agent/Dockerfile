FROM microsoft/vsts-agent:ubuntu-16.04
ARG NODE_VERSION=10

# Install basic command-line utilities
RUN apt-get update \
  && apt-get install -y --no-install-recommends \
  curl \
  dnsutils \
  file \
  ftp \
  iproute2 \
  iputils-ping \
  locales \
  openssh-client \
  rsync\
  shellcheck \
  sudo \
  telnet \
  time \
  unzip \
  wget \
  zip \
  tzdata \
  && rm -rf /var/lib/apt/lists/*

# Install Azure CLI (instructions taken from https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
RUN echo "deb [arch=amd64] https://packages.microsoft.com/repos/azure-cli/ $(lsb_release -cs) main" \
  | tee /etc/apt/sources.list.d/azure-cli.list \
  && curl -L https://packages.microsoft.com/keys/microsoft.asc | apt-key add - \
  && apt-get update \
  && apt-get install -y --no-install-recommends \
  apt-transport-https \
  azure-cli \
  && rm -rf /var/lib/apt/lists/* \
  && rm -rf /etc/apt/sources.list.d/* \
  && az --version

# Install Powershell Core
RUN curl https://packages.microsoft.com/keys/microsoft.asc | apt-key add - \
  && curl https://packages.microsoft.com/config/ubuntu/16.04/prod.list | tee /etc/apt/sources.list.d/microsoft.list \
  && apt-get update \
  && apt-get install -y --no-install-recommends \
  powershell \
  && rm -rf /var/lib/apt/lists/* \
  && rm -rf /etc/apt/sources.list.d/*


# Install NPM & Yarn
RUN apt-get update -yq \
  && apt-get install gnupg -yq \
  && curl -sL https://deb.nodesource.com/setup_$NODE_VERSION.x | bash \
  && apt-get install nodejs -yq

RUN npm install -g yarn

# Install Python
RUN apt-get update \
  && apt-get install -y --no-install-recommends \
  python \
  python-pip \
  python3 \
  python3-pip \
  && rm -rf /var/lib/apt/lists/*
  
RUN apt-get update -qq && apt-get install -qqy \
    apt-transport-https \
    ca-certificates \
    curl \
    lxc \
    iptables

ENV DOCKER_CHANNEL stable
ENV DOCKER_VERSION 18.06.1-ce

RUN set -ex \
 && curl -fL "https://download.docker.com/linux/static/${DOCKER_CHANNEL}/`uname -m`/docker-${DOCKER_VERSION}.tgz" -o docker.tgz \
 && tar --extract --file docker.tgz --strip-components 1 --directory /usr/local/bin \
 && rm docker.tgz \
 && docker -v

ENV DOCKER_COMPOSE_VERSION 1.22.0
ENV dotnetcore 2.2

RUN set -x \
 && curl -fSL "https://github.com/docker/compose/releases/download/$DOCKER_COMPOSE_VERSION/docker-compose-`uname -s`-`uname -m`" -o /usr/local/bin/docker-compose \
 && chmod +x /usr/local/bin/docker-compose \
 && docker-compose -v
 
# Clean system
RUN apt-get clean \
  && rm -rf /var/lib/apt/lists/* \
  && rm -rf /etc/apt/sources.list.d/*
