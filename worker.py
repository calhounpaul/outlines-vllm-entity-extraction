import os, sys, json, re, time, datetime, random, string, requests, hashlib, multiprocessing

parent_dir = os.path.dirname(os.path.realpath(__file__))
docker_data_dir = parent_dir
inputs_dir = os.path.join(parent_dir, "inputs")

FILETYPES = [
        "cs",
        #"py"
    ]

def one_curl_request(prompt, schema, max_tokens,url="http://127.0.0.1:8000/generate",opts={}):
    data = {
        "prompt": prompt,
        "schema": schema,
        "max_tokens": max_tokens
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
        prompts[i] = \
f"""System: You are a helpful AI assistant who replies only with perfectly formatted JSON

User: {prompts[i]}

Assistant:"""
    if "max_tokens" in kwarg_opts:
        opts["schema"]["maxLength"] = kwarg_opts["max_tokens"]*4
        del kwarg_opts["max_tokens"]
    opts.update(kwarg_opts)
    with multiprocessing.Pool(max_workers) as pool:
        responses = pool.starmap(one_curl_request, [(prompt, opts["schema"], opts["max_tokens"]) for prompt in prompts])
    for i, response in enumerate(responses):
        #json_string = response["text"][0].split("<|end_header_id|>")[-1].strip()
        #json_string = response["text"][0].split("<|im_start|>assistant")[-1].strip()
        json_string = response["text"][0].split("Assistant:")[-1].strip()
        try:
            responses[i] = json.loads(json_string)
        except:
            responses[i] = json_string
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

prompt_template = """Please extract the following elements from the provided controller file data and respond with a JSON dict structured as follows:
Start by specifying the `controllerName`, which is the name of the controller class (e.g., "UserController"). Next, provide the `baseRoute`, the root URL path for the controller (e.g., "/api/users"). In the `routes` array, list each API endpoint with detailed properties: the `route` field for the specific URL path, `httpMethod` for the HTTP method (GET, POST, etc.), `action` for the controller action method that handles the request, `authRequired` to indicate if authentication is needed (true or false), and `roles` as an array of roles allowed to access the endpoint. Define any `dependencies` the controller relies on, listing the `name` and `type` (e.g., "Service", "Repository") of each dependency. Lastly, in `modelsUsed`, list all the model names the controller utilizes. Ensure all required fields—`controllerName`, `baseRoute`, and `routes`—are filled in to complete the schema properly.

Now, provide a JSON dictionary containing the following data:"""

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
    if len(controller_contents) > 20000:
        controller_contents = controller_contents[:10000] + "\n\n[TRUNCATED MIDDLE PORTION OF FILE]\n\n" + controller_contents[-10000:]
        print("truncating input")
    prompt = prompt_template + "\n\n" + input_filename + "\n\n" + controller_contents
    response = query_llm(prompt, kwarg_opts={"schema": schema})
    output_filename = os.path.join(parent_dir, "outputs", ".".join(input_filename.split(".")[:-1]) + ".json")
    with open(output_filename, "w") as f:
        json.dump(response, f, indent=4)
    
