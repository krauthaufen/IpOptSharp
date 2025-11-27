#!/bin/bash

OS=`uname -s`

OS=`uname -s`

VCPKG_TRIPLET=""
ARCH=""
ARCH_FLAGS=""

a="/$0"; a=${a%/*}; a=${a#/}; a=${a:-.}; BASEDIR=$(cd "$a"; pwd)

# cd ./.vcpkg/vcpkg
# git reset --hard e52999ee1a61bfea654733712288c5d4469d38bc
# cd ../..

PLATFORM=""
ARCHNAME=""
ARCH="arm64"

if [ "$OS" = "Darwin" ];
then
    echo "MacOS"
    PLATFORM="mac"
    if [ "$1" = "x86_64" ]; then
        ARCHNAME="AMD64"
        ARCH="x86_64"
    elif [ "$1" = "arm64" ]; then
        ARCHNAME="ARM64"
    else
        ARCH=`uname -m | tail -1`
        if [ "$ARCH" = "x86_64" ]; then 
            ARCHNAME="AMD64"
            ARCH="x86_64"
        elif [ "$ARCH" = "arm64" ]; then
            ARCHNAME="ARM64"
        fi
    fi

    ./buildIpOptMac.sh $BASEDIR/libs/Native/IpOptSharp/mac/$ARCHNAME/ $ARCH releases/3.14.16
else
    echo "Linux"
    PLATFORM="linux"
    ARCHNAME="AMD64"
fi