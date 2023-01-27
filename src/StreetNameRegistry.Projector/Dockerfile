FROM mcr.microsoft.com/dotnet/runtime-deps:7.0.2

LABEL maintainer "Digitaal Vlaanderen <digitaal.vlaanderen@vlaanderen.be>"
LABEL registry="streetname-registry"

COPY / /app
WORKDIR /app

RUN apt-get update && \
    apt-get install curl jq -y && \
    chmod +x ./init.sh

EXPOSE 4006/tcp
ENV ASPNETCORE_URLS http://*:4006

ENTRYPOINT ["./init.sh"]
