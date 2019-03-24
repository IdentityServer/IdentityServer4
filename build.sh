mkdir nuget

cd ./src/Storage
./build.sh

if [ $? -ne 0 ]; then
    echo "An error occured."
    exit 1
fi

cd ../..
cp ../src/Storage/artifacts/*.nupkg ./nuget

cd ./src/IdentityServer4
./build.sh

if [ $? -ne 0 ]; then
    echo "An error occured."
    exit 1
fi

cd ../..
cp ../src/IdentityServer4/artifacts/*.nupkg ./nuget

cd ./src/
./build.sh

if [ $? -ne 0 ]; then
    echo "An error occured."
    exit 1
fi

cd ../..
cp ../src/EntityFramework.Storage/artifacts/*.nupkg ./nuget

cd ./src/EntityFramework
./build.sh

if [ $? -ne 0 ]; then
    echo "An error occured."
    exit 1
fi

cd ../..
cp ../src/EntityFramework/artifacts/*.nupkg ./nuget

cd ./src/AspNetIdentity
./build.sh

if [ $? -ne 0 ]; then
    echo "An error occured."
    exit 1
fi

cd ../..
cp ../src/AspNetIdentity/artifacts/*.nupkg ./nuget