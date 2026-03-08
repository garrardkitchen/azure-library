#!/bin/bash
set -e

# Authenticate the Azure CLI using the credentials provided via environment variables.
# This is required when running as a container since there is no interactive az login session.
#
# Supported methods (evaluated in priority order):
#   1. Service principal + client secret:   AZURE_CLIENT_ID + AZURE_CLIENT_SECRET + AZURE_TENANT_ID
#   2. Service principal + certificate:     AZURE_CLIENT_ID + AZURE_CLIENT_CERTIFICATE_PATH + AZURE_TENANT_ID
#   3. Service principal + federated token: AZURE_CLIENT_ID + AZURE_FEDERATED_TOKEN + AZURE_TENANT_ID
#      (use this to pass an OIDC access token from GitHub Actions, Azure AD, or any workload identity provider)
#
# Alternatively, mount your host credential cache read-only at /root/.azure-host to reuse an existing
# az login session without allowing the container to modify your local credentials:
#   docker run -v ~/.azure:/root/.azure-host:ro ...
#
# The entrypoint copies the read-only mount to /root/.azure (writable) so the Azure CLI can write
# its runtime cache files (commandIndex.json, versionCheck.json) without hitting a read-only fs error.

if [ -d /root/.azure-host ]; then
    cp -r /root/.azure-host/. /root/.azure/
fi

if [ -n "$AZURE_CLIENT_ID" ] && [ -n "$AZURE_TENANT_ID" ]; then
    if [ -n "$AZURE_CLIENT_SECRET" ]; then
        az login --service-principal \
            --username "$AZURE_CLIENT_ID" \
            --password "$AZURE_CLIENT_SECRET" \
            --tenant "$AZURE_TENANT_ID" \
            --output none
    elif [ -n "$AZURE_CLIENT_CERTIFICATE_PATH" ]; then
        az login --service-principal \
            --username "$AZURE_CLIENT_ID" \
            --certificate "$AZURE_CLIENT_CERTIFICATE_PATH" \
            --tenant "$AZURE_TENANT_ID" \
            --output none
    elif [ -n "$AZURE_FEDERATED_TOKEN" ]; then
        az login --service-principal \
            --username "$AZURE_CLIENT_ID" \
            --tenant "$AZURE_TENANT_ID" \
            --federated-token "$AZURE_FEDERATED_TOKEN" \
            --output none
    fi
fi

exec "$@"
