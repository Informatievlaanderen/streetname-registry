FROM mcr.microsoft.com/dotnet/core/runtime-deps:3.1.2

LABEL maintainer "agentschap Informatie Vlaanderen <informatie.vlaanderen@vlaanderen.be>"

COPY / /app
WORKDIR /app

RUN apt-get update && \
    apt-get install curl jq -y && \
    chmod +x ./init.sh

EXPOSE 4004/tcp
ENV ASPNETCORE_URLS http://*:4004

ENTRYPOINT ["./init.sh"]
