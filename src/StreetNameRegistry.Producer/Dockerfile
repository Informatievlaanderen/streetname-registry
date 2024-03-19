FROM mcr.microsoft.com/dotnet/runtime-deps:8.0.2-bookworm-slim-amd64
LABEL maintainer "Digitaal Vlaanderen <digitaal.vlaanderen@vlaanderen.be>"
LABEL registry="streetname-registry"

COPY / /app
WORKDIR /app

RUN apt-get update && \
    apt-get install curl jq -y && \
    chmod +x ./init.sh

EXPOSE 4014/tcp
ENV ASPNETCORE_URLS http://*:4014

ENTRYPOINT ["./init.sh"]
