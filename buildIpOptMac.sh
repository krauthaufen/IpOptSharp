#!/bin/zsh
set -e

# Build IPOPT for macOS with MUMPS statically linked
# This script creates a self-contained libipopt.dylib

# Usage: build-ipopt-macos.sh [output-dir] [arch] [version]
#   output-dir: Directory where libraries will be placed (default: ./ipopt-libs)
#   arch: arm64 or x86_64 (default: arm64)
#   version: IPOPT git tag/version (default: releases/3.14.16)

OUTPUT_DIR_INPUT="${1:-$(pwd)/ipopt-libs}"
ARCH="${2:-arm64}"
IPOPT_VERSION="${3:-releases/3.14.16}"

# Convert relative path to absolute
if [[ "$OUTPUT_DIR_INPUT" != /* ]]; then
    OUTPUT_DIR="$(pwd)/$OUTPUT_DIR_INPUT"
else
    OUTPUT_DIR="$OUTPUT_DIR_INPUT"
fi

BUILD_DIR="/tmp/ipopt-build-$$"
INSTALL_DIR="/tmp/ipopt-install-$$"

echo "==> Building IPOPT for macOS ${ARCH}"
echo "==> IPOPT version: ${IPOPT_VERSION}"
echo "==> Build directory: ${BUILD_DIR}"
echo "==> Install directory: ${INSTALL_DIR}"
echo "==> Output directory: ${OUTPUT_DIR}"

mkdir -p "${BUILD_DIR}"
mkdir -p "${OUTPUT_DIR}"
cd "${BUILD_DIR}"

# Download MUMPS third-party package
echo "==> Downloading MUMPS..."
curl -L https://github.com/coin-or-tools/ThirdParty-Mumps/archive/refs/tags/releases/3.0.5.tar.gz -o mumps.tar.gz
tar xzf mumps.tar.gz
cd ThirdParty-Mumps-releases-3.0.5

# Get MUMPS source
echo "==> Fetching MUMPS source..."
./get.Mumps

# Configure and build MUMPS
echo "==> Building MUMPS..."
./configure \
  --prefix="${INSTALL_DIR}" \
  CFLAGS="-arch ${ARCH}" \
  CXXFLAGS="-arch ${ARCH}" \
  FFLAGS="-arch ${ARCH} -fallow-argument-mismatch -fPIC" \
  --disable-dependency-tracking \
  --enable-static \
  --disable-shared

make -j$(sysctl -n hw.ncpu)
make install

cd "${BUILD_DIR}"

# Download IPOPT
echo "==> Cloning IPOPT..."
git clone --depth 1 --branch "${IPOPT_VERSION}" https://github.com/coin-or/Ipopt.git
cd Ipopt

# Configure IPOPT with static MUMPS
echo "==> Configuring IPOPT..."
mkdir build && cd build

../configure \
  --prefix="${INSTALL_DIR}" \
  CFLAGS="-arch ${ARCH}" \
  CXXFLAGS="-arch ${ARCH}" \
  FFLAGS="-arch ${ARCH} -fallow-argument-mismatch" \
  --disable-dependency-tracking \
  --with-mumps \
  --with-mumps-lflags="-L${INSTALL_DIR}/lib -lcoinmumps -framework Accelerate" \
  --with-mumps-cflags="-I${INSTALL_DIR}/include/coin-or/mumps" \
  --disable-linear-solver-loader

# Build and install
echo "==> Building IPOPT..."
make -j$(sysctl -n hw.ncpu)
make install

echo "==> Copying libraries to output directory..."
cd "${INSTALL_DIR}/lib"

# Resolve symlink and copy only the actual library file as libipopt.dylib
if [ -L libipopt.dylib ]; then
    # If it's a symlink, copy the target file
    cp -f "$(readlink libipopt.dylib)" "${OUTPUT_DIR}/libipopt.dylib"
elif [ -f libipopt.dylib ]; then
    # If it's a regular file, copy it directly
    cp -f libipopt.dylib "${OUTPUT_DIR}/libipopt.dylib"
else
    # Fall back to copying versioned file if unversioned doesn't exist
    cp -f libipopt.*.dylib "${OUTPUT_DIR}/libipopt.dylib"
fi

# Make libipopt self-contained by fixing paths
cd "${OUTPUT_DIR}"
install_name_tool -id "@rpath/libipopt.dylib" libipopt.dylib

# Check what dependencies we have
echo "==> Library dependencies:"
otool -L libipopt.dylib

# Copy gfortran libraries if needed
if otool -L libipopt.dylib | grep -q libgfortran; then
    echo "==> Copying gfortran runtime libraries..."
    GFORTRAN_LIB=$(gfortran -print-file-name=libgfortran.dylib)
    GFORTRAN_DIR=$(dirname "${GFORTRAN_LIB}")

    cp "${GFORTRAN_DIR}/libgfortran.5.dylib" . || true
    cp "${GFORTRAN_DIR}/libquadmath.0.dylib" . || true
    cp "${GFORTRAN_DIR}/libgcc_s.1.1.dylib" . || true

    # Fix paths to use @loader_path
    install_name_tool -change "@rpath/libgfortran.5.dylib" "@loader_path/libgfortran.5.dylib" libipopt.dylib || true
    install_name_tool -change "@rpath/libquadmath.0.dylib" "@loader_path/libquadmath.0.dylib" libipopt.dylib || true

    if [ -f libgfortran.5.dylib ]; then
        install_name_tool -id "@rpath/libgfortran.5.dylib" libgfortran.5.dylib
        install_name_tool -change "@rpath/libquadmath.0.dylib" "@loader_path/libquadmath.0.dylib" libgfortran.5.dylib || true
        install_name_tool -change "@rpath/libgcc_s.1.1.dylib" "@loader_path/libgcc_s.1.1.dylib" libgfortran.5.dylib || true
    fi

    if [ -f libquadmath.0.dylib ]; then
        install_name_tool -id "@rpath/libquadmath.0.dylib" libquadmath.0.dylib
    fi

    if [ -f libgcc_s.1.1.dylib ]; then
        install_name_tool -id "@rpath/libgcc_s.1.1.dylib" libgcc_s.1.1.dylib
    fi

    # Re-sign all libraries
    codesign --force --sign - *.dylib 2>/dev/null || true
fi

# Remove sIPOPT if present (optional library)
setopt NULL_GLOB 2>/dev/null || true
rm -f libsipopt*.dylib 2>/dev/null || true
unsetopt NULL_GLOB 2>/dev/null || true

echo ""
echo "==> Build complete!"
echo "==> Libraries copied to: ${OUTPUT_DIR}"
echo ""
echo "Files:"
ls -lh "${OUTPUT_DIR}"/*.dylib

# Cleanup
echo ""
echo "==> Cleaning up build and install directories..."
rm -rf "${BUILD_DIR}"
rm -rf "${INSTALL_DIR}"

echo "==> Done!"
