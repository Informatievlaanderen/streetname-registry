#!/usr/bin/python

import sys, os, subprocess, json, time

# variables
access_key = os.getenv('AWS_BUILD_USER_ACCESS_KEY_ID')
secret_key = os.getenv('AWS_BUILD_USER_SECRET_ACCESS_KEY')
build_api = os.environ.get('AWS_BUILD_API')
region = 'eu-west-1'
registry_endpoint = f'{build_api}/streetname-registry-api'
service = 'execute-api'
data = os.environ.get('BODY')

def exec(cmd):
    return subprocess.check_output(cmd, shell=True)

def storeValueInEnv(name, value):
    return exec(f'echo {name.upper()}={value} >> $GITHUB_ENV')

def execAndStoreInEnv(cmd, name):
    return storeValueInEnv(name, f'$({cmd})')

def getStatus():
    cmd_awscurl = f'awscurl --access_key {access_key} --secret_key {secret_key} --region {region} --service {service} -X POST -d {data} {registry_endpoint}'
    output = exec(cmd_awscurl)
    return json.loads(output)

def main():
    max_count = 10
    count = 0
    output = None
    while count <= max_count:
        output = getStatus()
        print(f'output: {output}')
        if output.statusCode.is_integer() and  output.statusCode == 200:
            break
        count += 1
        time.sleep(2) #sleep 2 seconds
    s_output = json.dumps(output)
    storeValueInEnv('RESPONSE_STATUS', s_output)
    print(s_output)


if __name__ == "__main__":
    main()
    sys.exit()