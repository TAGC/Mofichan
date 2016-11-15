FROM microsoft/dotnet
WORKDIR /app
COPY . .

# When the app runs it will actually try to find the config file
# at app/mofichan.config for whatever reason
COPY src/Mofichan.Runner/mofichan.config .

RUN dotnet restore
RUN dotnet publish "src/Mofichan.Runner" -c Debug
ENTRYPOINT ["dotnet", "src/Mofichan.Runner/bin/Debug/netcoreapp1.0/Mofichan.Runner.dll"]