#!/usr/bin/env bash
# Build script for CliCoreKit NuGet packages (Linux/macOS)

set -e

VERSION="${1:-1.0.0}"
CONFIGURATION="${2:-Release}"
OUTPUT="./artifacts"

echo "====================================="
echo "  CliCoreKit Build Script"
echo "====================================="
echo "Version: $VERSION"
echo "Configuration: $CONFIGURATION"
echo "Output: $OUTPUT"
echo ""

# Clean
if [ "$3" == "--clean" ]; then
    echo "Cleaning..."
    dotnet clean -c "$CONFIGURATION"
    rm -rf "$OUTPUT"
fi

# Create output directory
mkdir -p "$OUTPUT"

# Restore dependencies
echo "Restoring dependencies..."
dotnet restore

# Build solution
echo "Building solution..."
dotnet build -c "$CONFIGURATION" --no-restore /p:Version="$VERSION"

# Run tests
echo "Running tests..."
dotnet test -c "$CONFIGURATION" --no-build --verbosity normal

# Pack Core library
echo "Packing Monbsoft.CliCoreKit.Core..."
dotnet pack src/CliCoreKit.Core/CliCoreKit.Core.csproj \
    -c "$CONFIGURATION" \
    --no-build \
    -o "$OUTPUT" \
    /p:Version="$VERSION" \
    /p:PackageVersion="$VERSION"

# Pack Hosting library
echo "Packing Monbsoft.CliCoreKit.Hosting..."
dotnet pack src/CliCoreKit.Hosting/CliCoreKit.Hosting.csproj \
    -c "$CONFIGURATION" \
    --no-build \
    -o "$OUTPUT" \
    /p:Version="$VERSION" \
    /p:PackageVersion="$VERSION"

echo ""
echo "====================================="
echo "  Build Completed Successfully!"
echo "====================================="
echo "Packages created in: $OUTPUT"
echo ""

ls -1 "$OUTPUT"/*.nupkg

echo ""
echo "To publish to NuGet.org:"
echo "  dotnet nuget push $OUTPUT/*.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json"
echo ""
