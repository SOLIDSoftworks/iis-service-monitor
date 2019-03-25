FROM microsoft/dotnet-framework:4.7.2-sdk AS base

# if your company or enterprise is using ssl interception, this is where you need to install certs for the egress
# RUN Import-Certificate -FilePath <path> -CertStoreLocation <location>

FROM base as servicemonitor
WORKDIR /src
COPY ./ServiceMonitor .
RUN dotnet build ServiceMonitor.csproj -c Release -o c:\output

FROM mcr.microsoft.com/dotnet/framework/aspnet:4.7.2
COPY --from=servicemonitor /output /