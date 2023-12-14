#get a base image to work from, here we need  image that contains the .NET SDK 
#https://hub.docker.com/_/microsoft-dotnet-sdk  
#https://viblo.asia/p/docker-tao-docker-images-tu-dockerfile-3P0lPORvZox
#https://docs.docker.com/engine/reference/builder/
FROM mcr.microsoft.com/dotnet/sdk:7.0 as build 
WORKDIR /app
EXPOSE 80

#copy all .csproj files and restore as distinct layers. Use the same copy command
#for every dockerfile in the project to take advantage of docker caching
COPY Carsties.sln Carsties.sln
COPY src/AuctionService/AuctionService.csproj src/AuctionService/AuctionService.csproj
COPY src/SearchService/SearchService.csproj src/SearchService/SearchService.csproj
COPY src/GatewayService/GatewayService.csproj src/GatewayService/GatewayService.csproj
COPY src/Contracts/Contracts.csproj src/Contracts/Contracts.csproj
COPY src/IdentityService/IdentityService.csproj src/IdentityService/IdentityService.csproj

#restore package dependencies
#it gonna take a look at Carsties.sln file and all the .csproj file
#and run the dotnet restore command on that
RUN dotnet restore Carsties.sln

#copy the app folders over
#copy Contracts since AuctionService depends on it
COPY src/AuctionService src/AuctionService
COPY src/Contracts src/Contracts
#vì đã copy ở trên nên giờ đã có Auction Service trong /app/src/AuctionService
#change work directory 
WORKDIR /app/src/AuctionService
#publish the auction service application and the public file
#gonna be stored in /app/src/out directory
RUN dotnet publish -c Release -o /app/src/out

#get a .net runtime image instead of sdk since it's smaller
#and we already had a publish version of auction service
#build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
#https://stackoverflow.com/questions/66353510/what-is-from-used-in-copy-command-in-dockerfile
COPY --from=build /app/src/out .
ENTRYPOINT [ "dotnet", "AuctionService.dll" ]