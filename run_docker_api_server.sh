#!/bin/bash

#HF_TOKEN=''

HF_CACHE_FOLDER_PATH=./cache

docker run --rm -it \
    -v $HF_CACHE_FOLDER_PATH:/root/.cache/huggingface \
    --gpus='"device=0"' \
    -p 8000:8000 \
    --ipc=host \
    outlines_vllm_server \
    --dtype="float16" \
    --model nvidia/Llama3-ChatQA-1.5-8B

#    --env "HUGGING_FACE_HUB_TOKEN=$HF_TOKEN"

#    --model NousResearch/Hermes-2-Theta-Llama-3-8B
#    --model nvidia/Llama3-ChatQA-1.5-8B
#    --model casperhansen/llama-3-8b-instruct-awq
#    --model TheBloke/Mistral-7B-Instruct-v0.2-AWQ
#    --model casperhansen/mixtral-instruct-awq
#    --model TheBloke/OpenHermes-2.5-Mistral-7B-16k-AWQ
