# iis-service-monitor

This is a .Net port of [IIS ServiceMonitor](https://github.com/Microsoft/IIS.ServiceMonitor). It was originally written because there was a bug with uppercasing environment variable keys in the C++ implementation from Microsoft. This implementation fixes that bug. 

This repo is accompanied by a [blog post](https://solidsoft.works/2019/03/25/porting-iis-servicemonitor-to-net/).

## Usage
    $ docker build --rm -t your_company/dotnet/framework/aspnet:4.7.2 -f Dockerfile .
    $ docker tag your_company/dotnet/framework/aspnet:4.7.2 your_company/dotnet/framework/aspnet:latest  
  
From here you can push it to your own docker hub or use it directly in a Dockerfile on you machine as a base image.

## Better logging
There is better console logging in this version of the IIS ServiceMonitor. If you run a docker container that is using the .Net ServiceMontior with the environment variable __SERVICEMONITOR__Logging__LogLevel__Default__ you can set it to Trace or Debug and get more detailed console output.

## Samples
    $ docker build --rm -t solid/demo/website-with-service-monitor -f Dockerfile_WebsiteWithServiceMonitor .
    $ docker build --rm -t solid/demo/website-without-service-monitor -f Dockerfile_WebsiteWithoutServiceMonitor .
    $ docker run -d --rm -p 8081:80 -e some_environment_variable='value' solid/demo/website-with-service-monitor:latest
    $ docker run -d --rm -p 8082:80 -e some_environment_variable='value' solid/demo/website-without-service-monitor:latest