#!/bin/bash

#rm -rf vllm

if [ ! -d "vllm" ]; then
    git clone https://github.com/vllm-project/vllm
    #git checkout 919770957f26d71a5a6eda7a1a7443dfeb5ba0ee
    cd vllm
    #sed -i 's/ENTRYPOINT \[/#ENTRYPOINT \[/g' Dockerfile
    #echo "ENTRYPOINT [\"python3\", \"-m\", \"outlines.serve.serve\"]" >> Dockerfile
    #echo "ENTRYPOINT [\"python3\", \"-m\", \"vllm.entrypoints.api_server\"]" >> Dockerfile
    cd ..
fi
cd vllm
docker build -t outlines_vllm_server .