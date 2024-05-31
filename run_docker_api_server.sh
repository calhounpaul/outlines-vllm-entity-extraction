#!/bin/bash

HF_TOKEN=""
if [ -f secrets.json ]; then
    HF_TOKEN=$(jq -r '.HF_TOKEN' secrets.json)
fi

HF_CACHE_FOLDER_PATH=./cache

docker run --rm -it \
    -v $HF_CACHE_FOLDER_PATH:/root/.cache/huggingface \
    --gpus='"device=0"' \
    -p 8000:8000 \
    --env "HUGGING_FACE_HUB_TOKEN=$HF_TOKEN" \
    --ipc=host \
    -m 90g \
    outlines_vllm_server \
    --guided-decoding-backend outlines \
    --dtype auto --api-key a-super-secret-token \
    --max-model-len 32000 \
    --model mistralai/Mistral-7B-Instruct-v0.3

#    --model solidrust/Mistral-7B-Instruct-v0.3-AWQ
#    --model neuralmagic/Mistral-7B-Instruct-v0.3-GPTQ-4bit
#    --model NousResearch/Hermes-2-Theta-Llama-3-8B
#    --model nvidia/Llama3-ChatQA-1.5-8B
#    --model casperhansen/llama-3-8b-instruct-awqd
#    --model TheBloke/Mistral-7B-Instruct-v0.2-AWQ
#    --model casperhansen/mixtral-instruct-awq
#    --model TheBloke/OpenHermes-2.5-Mistral-7B-16k-AWQ
