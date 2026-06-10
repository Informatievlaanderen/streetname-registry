FROM public.ecr.aws/lambda/dotnet:10-arm64

WORKDIR /var/task

COPY "bin/Release/lambda-publish"  .

CMD ["StreetNameRegistry.Api.BackOffice.Handlers.Lambda::StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Function::Handler"]
