FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

RUN apt-get update && apt-get install -y \
    clang \
    zlib1g-dev

COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o / -r linux-x64 --self-contained true

FROM debian:bookworm-slim
WORKDIR /
EXPOSE 65432
COPY --from=build / .
ENTRYPOINT ["./Agent"]
