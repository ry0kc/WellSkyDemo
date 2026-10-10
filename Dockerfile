# Build stage: full SDK image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first so the restore layer is cached
# unless dependencies change.
COPY ReferralDemo/ReferralDemo.csproj ReferralDemo/
COPY ReferralDemo.Rules/ReferralDemo.Rules.vbproj ReferralDemo.Rules/
RUN dotnet restore ReferralDemo/ReferralDemo.csproj

COPY ReferralDemo/ ReferralDemo/
COPY ReferralDemo.Rules/ ReferralDemo.Rules/
RUN dotnet publish ReferralDemo/ReferralDemo.csproj -c Release -o /app --no-restore

# Runtime stage: smaller image with only the ASP.NET runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "ReferralDemo.dll"]