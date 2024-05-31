import os, sys, json, re, time, datetime, random, string, requests, hashlib, multiprocessing, json_repair
from openai import OpenAI


oai_client = OpenAI(
    base_url="http://localhost:8000/v1",
    api_key="a-super-secret-token",
)

parent_dir = os.path.dirname(os.path.realpath(__file__))
docker_data_dir = parent_dir
inputs_dir = os.path.join(parent_dir, "inputs")

truncate_middle_at = 15000

FILETYPES = [
        "cs",
        #"py"
    ]

def one_oai_request(prompt, schema, max_tokens,oai_opts={},other_opts={}):
    kwargs = {
            "model":"mistralai/Mistral-7B-Instruct-v0.3",
            "messages":[
                {"role": "user", "content": prompt},
            ],
            #max_tokens=256,
            #temperature=0.7,
            #top_p=1,
            "extra_body":dict(guided_json=schema),
            "stream":False,
        }
    kwargs.update(oai_opts)
    kwargs["extra_body"].update(other_opts)
    chat_response = oai_client.chat.completions.create(**kwargs)
    print(chat_response)
    return chat_response
    

def one_curl_request(prompt, schema, max_tokens,url="http://127.0.0.1:8000/generate",opts={}):
    data = {
        "prompt": prompt,
        "schema": schema,
        "max_tokens": max_tokens,
    }
    data.update(opts)
    return requests.post(url, data=json.dumps(data)).json()

def iterate_curl_requests(prompts, schema, max_tokens,url="http://127.0.0.1:8000/generate",opts={}):
    responses = []
    for prompt in prompts:
        responses.append(one_curl_request(prompt, schema, max_tokens,url,opts))
    return responses

def query_llm(prompts, kwarg_opts={},max_workers=8):
    if not isinstance(prompts, list):
        prompts = [prompts]
    opts = {
        "max_tokens": 4096,
        "schema": {"type": "string", "maxLength": 50}
    }
    for i in range(len(prompts)):
        #prompts[i] = "<|begin_of_text|><|start_header_id|>system<|end_header_id|>\n\nYou are a helpful AI assistant who replies only with perfectly formatted JSON<|eot_id|><|start_header_id|>user<|end_header_id|>\n\n" + prompts[i] + "<|eot_id|><|start_header_id|>assistant<|end_header_id|>"
        #prompts[i] = "<|im_start|>system\nYou are a helpful AI assistant who replies only with perfectly formatted JSON<|im_end|>\n<|im_start|>user\n" + prompts[i] + "<|im_end|>\n<|im_start|>assistant"
        #prompts[i] = "System: You are a helpful AI assistant who replies only with perfectly formatted JSON\n\nUser: {}\n\nAssistant:".format(prompts[i])
        #prompts[i] = "[INST] {} [/INST]".format(prompts[i])
        pass
    if "max_tokens" in kwarg_opts:
        opts["schema"]["maxLength"] = kwarg_opts["max_tokens"]*4
        del kwarg_opts["max_tokens"]
    opts.update(kwarg_opts)
    with multiprocessing.Pool(max_workers) as pool:
        #responses = pool.starmap(one_curl_request, [(prompt, opts["schema"], opts["max_tokens"]) for prompt in prompts])
        responses = pool.starmap(one_oai_request, [(prompt, opts["schema"], opts["max_tokens"]) for prompt in prompts])
    for i, response in enumerate(responses):
        #json_string = response["text"][0].split("<|end_header_id|>")[-1].strip()
        #json_string = response["text"][0].split("<|im_start|>assistant")[-1].strip()
        #json_string = response["text"][0].split("[/INST]")[-1].strip()
        json_string = response.choices[0].message.content
        try:
            responses[i] = json.loads(json_string)
        except:
            try:
                responses[i] = {
                        "original_string":json_string,
                        "repaired_json":json_repair.loads(json_string)
                    }
            except:
                print("ERROR: Could not parse JSON response from LLM server. Response is:", json_string)
    return responses



def test_vllm_outlines_server():
    test_questions = ["What is the capital of France?","What is the capital of Finland?",
            "What is the capital of Sweden?","What is the capital of Norway?","What is the capital of Denmark?",
            "What is the capital of Germany?","What is the capital of Italy?","What is the capital of Spain?",
            "What is the capital of Portugal?","What is the capital of the Netherlands?","What is the capital of Belgium?",
            "What is the capital of Luxembourg?","What is the capital of Switzerland?","What is the capital of Austria?",
            "What is the capital of Poland?","What is the capital of Czech Republic?","What is the capital of Slovakia?",
            "What is the capital of Hungary?","What is the capital of Slovenia?","What is the capital of Croatia?",
        ]
    kwargs = {
        #"schema": {"type": "string", "maxLength": 50},
        "schema":{
            "type": "object",
            "properties": {
                "capital": {
                    "type": "string"
                }
            },
            "required": ["capital"]
        }
    }

    responses = query_llm(test_questions, kwarg_opts=kwargs)

    print(responses)

#test_vllm_outlines_server()

schema_file = os.path.join(parent_dir, "controller_entity_schema.json")
with open(schema_file, "r") as f:
    schema = json.load(f)

if not os.path.exists(os.path.join(parent_dir, "outputs")):
    os.makedirs(os.path.join(parent_dir, "outputs"))

controllers_list = os.listdir(inputs_dir)

random.shuffle(controllers_list)

for input_filename in controllers_list:
    if input_filename.split(".")[-1] not in FILETYPES:
        continue
    #if input_filename in os.listdir(os.path.join(parent_dir, "outputs")):
    #    continue
    with open(os.path.join(inputs_dir, input_filename), "r") as f:
        controller_contents = f.read()
    if len(controller_contents) > truncate_middle_at:
        controller_contents = controller_contents[:int(truncate_middle_at/2)] + "...\n\n[TRUNCATED MIDDLE PORTION OF FILE]\n\n..." + controller_contents[-int(truncate_middle_at/2):]
        print("truncating input")
    prompt = "Extract all C# API controller file data from the following code:\n\n" + input_filename + "\n==========[Begin C# API controller file]==========\n" + controller_contents + "\n==========[End C# API controller file]==========\nNow, please provide the extracted API controller file data in JSON format."
    response = query_llm(prompt, kwarg_opts={"schema": schema})
    output_filename = os.path.join(parent_dir, "outputs", ".".join(input_filename.split(".")[:-1]) + ".json")
    with open(output_filename, "w") as f:
        json.dump(response, f, indent=4)
    
