FROM mcr.microsoft.com/dotnet/runtime-deps:5.0.6

LABEL maintainer "Digitaal Vlaanderen <digitaal.vlaanderen@vlaanderen.be>"

COPY / /app
WORKDIR /app

RUN apt-get update && \
    apt-get install curl jq -y && \
    chmod +x ./init.sh

EXPOSE 4004/tcp
ENV ASPNETCORE_URLS http://*:4004

ENTRYPOINT ["./init.sh"]
