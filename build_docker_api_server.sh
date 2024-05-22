#!/bin/bash

#rm -rf vllm

if [ ! -d "vllm" ]; then
    git clone https://github.com/vllm-project/vllm
    #git checkout c7f2cf2b7f67bce5842fedfdba508440fe257375
    cd vllm
    sed -i 's/ENTRYPOINT \[/#ENTRYPOINT \[/g' Dockerfile
    echo "ENTRYPOINT [\"python3\", \"-m\", \"outlines.serve.serve\"]" >> Dockerfile
    cd ..
fi
cd vllm
docker build -t outlines_vllm_server .