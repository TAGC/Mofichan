##########################################################################################
#
# NOTE: When running a container, you need to supply required configuration settings
#       either by mounting a configuration file containing them or by passing them in
#       as environment variables.
#

FROM microsoft/dotnet
MAINTAINER David Fallah <davidfallah1@gmail.com>
WORKDIR /app
COPY . .
RUN dotnet restore
RUN dotnet test "test/Mofichan.Spec"
RUN dotnet publish "src/Mofichan.Runner" -c Debug
ENTRYPOINT ["dotnet", "src/Mofichan.Runner/bin/Debug/netcoreapp1.0/Mofichan.Runner.dll"]