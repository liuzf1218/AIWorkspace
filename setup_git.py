import paramiko

client = paramiko.SSHClient()
client.set_missing_host_key_policy(paramiko.AutoAddPolicy())
client.connect('172.19.12.4', username='root', password='act4', timeout=15)

stdin, stdout, stderr = client.exec_command('mkdir -p ~/git-repos && git init --bare ~/git-repos/AIWorkspace.git')
print("INIT OUT:", stdout.read().decode())
print("INIT ERR:", stderr.read().decode())

with open('/c/Users/jnqf_/.ssh/id_ed25519.pub', 'r') as f:
    pub_key = f.read().strip()

client.exec_command('mkdir -p ~/.ssh && chmod 700 ~/.ssh')
cmd = f"echo '{pub_key}' >> ~/.ssh/authorized_keys && chmod 600 ~/.ssh/authorized_keys"
stdin, stdout, stderr = client.exec_command(cmd)
print("KEY OUT:", stdout.read().decode())
print("KEY ERR:", stderr.read().decode())

client.close()
print("DONE")
