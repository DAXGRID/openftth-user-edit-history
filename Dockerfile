ARG PROJECT_NAME=OpenFTTH.UserEditHistory
ARG DOTNET_VERSION=7.0

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build-env

# Renew the ARG argument for it to be available in this build context.
ARG PROJECT_NAME

WORKDIR /app

COPY ./*sln ./

COPY ./src/**/*.csproj ./src/${PROJECT_NAME}/

RUN dotnet restore --packages ./packages

COPY . ./
WORKDIR /app/src/${PROJECT_NAME}
RUN dotnet publish -c Release -o out --packages ./packages

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:${DOTNET_VERSION}

# Renew the ARG argument for it to be available in this build context.
ARG PROJECT_NAME

# Cannot reference ARG in CMD, so we set it in ENV instead.
ENV EXECUTEABLE=${PROJECT_NAME}.dll

WORKDIR /app

COPY --from=build-env /app/src/${PROJECT_NAME}/out .

CMD dotnet $(echo ${EXECUTEABLE})
