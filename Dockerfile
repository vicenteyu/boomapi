# 编译阶段
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
# 复制项目文件
COPY ["src/BoomApi.csproj", "src/"]
RUN dotnet restore "src/BoomApi.csproj"
# 复制所有源码并发布
COPY . .
WORKDIR "/src/src"
RUN dotnet publish "BoomApi.csproj" -c Release -r linux-x64 --self-contained -p:PublishAot=true -o /app/publish

# 运行阶段
FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-jammy-chiseled
WORKDIR /app
COPY --from=build /app/publish .
# 别忘了 AOT 镜像通常需要开放 8080 端口
EXPOSE 8080
ENTRYPOINT ["./BoomApi"]